using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class PlayStackPositionManager : MonoBehaviour
{
    // Singleton instance
    public static PlayStackPositionManager Instance { get; private set; }

    // Constant value to define the overlap of cards
    public float MAX_STACK_WIDTH = 6f;
    public float CARD_WIDTH = 2f;
    public float CARD_OVERLAP = 0.7f;
    public float MIN_CARD_OVERLAP = 0.5f;
    public float waitTime = 0.5f;
    private const float ANIMATION_TIME = 0.2f; // Time taken for the card to move to its position in stack

    // Prefab reference
    public GameObject cardPrefab;
    private TurnManager tm;
    // List to store all the cards in the play stack
    // List to store all elements on the stack
    private List<PlayStackElement> playStack = new List<PlayStackElement>();
    // Event to be triggered when a card is added
    public delegate void CardAddedEventHandler(PlayStackElement e);
    public event CardAddedEventHandler OnCardAdded;

    // Event to be triggered when a card is added or removed
    public delegate void CardStackChangeEventHandler(PlayStackElement e, int change);
    public event CardStackChangeEventHandler OnCardStackChange;
    private bool stackBeingPlayed = false;
    //TODO: may have discovered that PlayTopCard is called twice when addCard is called from a creature effect. Need to identify why and maybe need to decouple add card from play
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }
    private void Start()
    {
        tm = TurnManager.Instance;
    }

    private void Update()
    {

    }
    // Function to add a card to the play stack
    public void AddCard(PlayStackElement playStackElement)
    {
        GameObject card = playStackElement.Card;
        HoverPreview preview = card.GetComponent<HoverPreview>();
        if (preview != null)
        {
            preview.TargetScale = 1.2f;
            preview.TargetPosition = new Vector3(0f, 1.5f, 0f);
        }
        Draggable draggable = card.GetComponent<Draggable>();
        if (draggable != null)
        {
            draggable.enabled = false;
        }
        TurnManager.Instance.PushPlayer(playStackElement.Player);
        Canvas canvas = card.GetComponentInChildren<Canvas>();
        canvas.sortingLayerName = "Everything else";
        canvas.sortingOrder = 500;
        // Add the card to the list
        playStack.Insert(0, playStackElement);
        // Invoke OnCardStackChange event with 1 as card is added
        OnCardStackChange?.Invoke(playStackElement, 1);


        // Set the card's parent to the play stack game object
        card.transform.SetParent(transform);
        UpdateCardPositions();
        // Trigger the event
        int cardCount = playStack.Count;
        OnCardAdded?.Invoke(playStackElement);
        // Recalculate the positions of all cards in the stack
        if (cardCount == playStack.Count)
        {
            stackBeingPlayed = true;
            UpdatePosAndPlay();
        }

        //check if other player can respond.
        //if it can, use TurnManager to set the active player to the other one and enable the "Play Stack" button for him.
        //in another method, create a method to respond to click event on the "Play stack" button. Same method will be called when it's time to play a card's callback.
    }

    private void UpdatePosAndPlay()
    {
        Sequence seq = DOTween.Sequence();
        seq.AppendInterval(waitTime);
        seq.OnComplete(() =>
        {
            if (playStack.Count > 0 && stackBeingPlayed == true)
            {
                PlayTopCard();
            }
        });
        seq.Play();
    }

    private void PlayTopCard()
    {
        PlayStackElement playStackElement = playStack[0];
        GameObject card = playStackElement.Card;
        Action cb = playStackElement.Callback;
        Debug.Log("Card in Stack: " + card.name);
        if (cb == null) Debug.Log("Callback in Stack: " + cb);
        else Debug.Log("Callback in Stack: " + cb.Method.Name);
        Debug.Log("Stack being played right now: " + stackBeingPlayed);

        bool otherPlayerCanRespond = playStackElement.Player.otherPlayer.PlayerCanPlayCardsOnOpponentTurn();
        if (otherPlayerCanRespond)
        {
            stackBeingPlayed = false;
            // Save the current player to the stack before changing turns

            playStackElement.Player.OnTemporaryTurnEnd();
            tm.SetCurrentPlayer(playStackElement.Player.otherPlayer, false);
        }
        else
           PlayTopCardNoResponse();
    }

    private void PlayTopCardNoResponse()
    {
        PlayStackElement playStackElement = playStack[0];
        bool lastCardInStack = playStack.Count == 1;
        tm.SetCurrentPlayer(playStackElement.Player, lastCardInStack);

        stackBeingPlayed = true;
        Action callback = playStackElement.Callback;
        Sequence seq = DOTween.Sequence();
        seq.AppendInterval(waitTime);
        seq.AppendCallback(() =>
        {
            if (callback != null) callback();
        });
        seq.Play();

    }

    public void PlayStack() {
        PlayTopCardNoResponse();
    }

    // Function to remove a card from the play stack
    public void RemoveCard(GameObject card)
    {
        // Remove the card from the list
        PlayStackElement element = playStack.Find(p => p.Card == card);
        playStack.Remove(element);
        Destroy(card);
        ResetStackPlayedFlag();
        UpdateCardPositions();
        // Recalculate the positions of all cards in the stack
        UpdatePosAndPlay();
    }

    private void ResetStackPlayedFlag()
    {
        if (playStack.Count == 0) stackBeingPlayed = false;
    }

    // Function to remove a card from the play stack
    public void RemoveCard()
    {
        if (playStack.Count > 0)
        {
            GameObject cardToRemove = playStack[0].Card;
            playStack.RemoveAt(0);  // Removes the first card in the list
            Destroy(cardToRemove);
            UpdateCardPositions();
            UpdatePosAndPlay();
        }
        else
        {
            ResetStackPlayedFlag();
        }
    }
    // Overloaded RemoveCard method to remove a card at a specific position
    public void RemoveCard(int index)
    {
        if (index >= 0 && index < playStack.Count)
        {
            GameObject cardToRemove = playStack[index].Card;
            playStack.RemoveAt(index);  // Removes the card at the specified index
            Destroy(cardToRemove);
            ResetStackPlayedFlag();
            UpdateCardPositions();
        }
    }

    // Function to recalculate the positions of all cards in the stack
    private void UpdateCardPositions()
    {
        float currentStackWidth = playStack.Count * CARD_WIDTH - (playStack.Count - 1) * CARD_OVERLAP;

        // If the current stack width exceeds the maximum stack width, increase the overlap
        if (currentStackWidth > MAX_STACK_WIDTH)
        {
            CARD_OVERLAP += (currentStackWidth - MAX_STACK_WIDTH) / (playStack.Count - 1);
        }
        // If the current stack width is less than the maximum stack width, decrease the overlap
        else if (currentStackWidth < MAX_STACK_WIDTH && CARD_OVERLAP > MIN_CARD_OVERLAP)
        {
            CARD_OVERLAP -= Math.Min((MAX_STACK_WIDTH - currentStackWidth) / (playStack.Count - 1), CARD_OVERLAP - MIN_CARD_OVERLAP);
        }

        for (int i = 0; i < playStack.Count; i++)
        {
            Vector3 newPosition = CalculateCardPosition(i);
            playStack[i].Card.transform.DOLocalMove(newPosition, ANIMATION_TIME);
        }
    }

    // Function to calculate the position of a card in the play stack
    private Vector3 CalculateCardPosition(int index)
    {
        // The x position is determined by the index of the card in the list
        // The card's index is multiplied by the card width and the overlap is subtracted to ensure the cards overlap correctly
        float xPos = index * (CARD_WIDTH - CARD_OVERLAP);

        // The z position ensures the left card is on top of the right card.
        // To achieve this, you can just use the index of the card, as cards with a higher index (to the right) will have a lower z position
        float zPos = -(index * 0.01f);

        // The y position can remain the same, so we use the current y position of the play stack
        float yPos = transform.position.y;

        // Return the new position
        return new Vector3(xPos, yPos, zPos);
    }
}
