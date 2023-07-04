using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using CG.Cards;
using System;
public class HandVisual : MonoBehaviour
{
    // PUBLIC FIELDS
    public AreaPosition owner;
    public bool TakeCardsOpenly = true;
    public PositionManager slots;
    [SerializeField]
    public CardAsset testCardAsset;

    [Header("Transform References")]
    public Transform DrawPreviewSpot;
    public Transform DeckTransform;
    public Transform OtherCardDrawSourceTransform;
    public Transform PlayPreviewSpot;
    public event Action OnHandChange;
    private int UniqueID;

    // PRIVATE : a list of all card visual representations as GameObjects
    private List<GameObject> CardsInHand = new List<GameObject>();
    // Define the list to track the slots
    List<GameObject> slotTracker = new List<GameObject>();
    private readonly int MAX_CARDS_IN_HAND = 9;
    // public event Action CommandComplete = delegate {};

    // ADDING OR REMOVING CARDS FROM HAND
    void Start()
    {
        // Initialize the slot tracker with nulls
        for (int i = 0; i < 9; i++)
        {
            slotTracker.Add(null);
        }
        UniqueID = 0;
        //CommandComplete += Command.CommandExecutionFlagUpdate;
    }
    private void NullifySlots()
    {
        for (int i = 0; i < 9; i++)
        {
            slotTracker[i] = null;
        }
    }
    // add a new card GameObject to hand
    public int AddCard(GameObject card)
    {
        CardsInHand.Add(card);

        // parent this card to our Slots GameObject
        card.transform.SetParent(slots.transform);

        // re-calculate the position of the hand
        PlaceCardsOnNewSlotsVirtual();
        return slotTracker.IndexOf(card);
    }

    // remove a card GameObject from hand
    public void RemoveCard(GameObject card)
    {
        // remove a card from the list
        CardsInHand.Remove(card);


        // re-calculate the position of the hand
        PlaceCardsOnNewSlotsVirtual();
        PlaceCardsOnNewSlotsAnimate(0.5f);
        OnHandChange?.Invoke();
    }

    // remove card with a given index from hand
    public void RemoveCardAtIndex(int index)
    {
        CardsInHand.RemoveAt(index);
        // re-calculate the position of the hand
        PlaceCardsOnNewSlotsVirtual();
    }

    // get a card GameObject with a given index in hand
    public GameObject GetCardAtIndex(int index)
    {
        return CardsInHand[index];
    }

    // MANAGING CARDS AND SLOTS

    void PlaceCardsOnNewSlotsVirtual()
    {
        int middleIndex = 4;
        int totalCards = CardsInHand.Count;
        int startIndex, endIndex;
        bool isEven = totalCards % 2 == 0;

        NullifySlots();
        if (isEven)
        {
            startIndex = middleIndex - totalCards / 2;
            endIndex = middleIndex + totalCards / 2;
        }
        else
        {
            startIndex = middleIndex - totalCards / 2;
            endIndex = middleIndex + totalCards / 2 + 1;
        }

        for (int i = 0; i < totalCards; i++)
        {
            int targetSlotIndex = startIndex + i;
            if (targetSlotIndex == middleIndex && isEven)
            {
                startIndex++;
                targetSlotIndex++;
            }
            GameObject g = CardsInHand[i];

            // Update the slot tracker
            slotTracker[targetSlotIndex] = g;

            WhereIsTheCardOrCreature w = g.GetComponent<WhereIsTheCardOrCreature>();
            w.Slot = targetSlotIndex;
            w.SetHandSortingOrder();
        }
    }

    private void PlaceCardsOnNewSlotsAnimate(float duration)
    {
        // Iterate over the slotTracker list and move the cards in parallel
        for (int i = 0; i < slotTracker.Count; i++)
        {
            GameObject g = slotTracker[i];
            if (g != null) // Check that the slot isn't empty
            {
                if (owner == AreaPosition.Top)
                {
                    // Use DOMove to move the card to the position of its new slot
                    // Quaternion slotRotation = slots.Children[i].transform.rotation;
                    // Quaternion newCardRotation = Quaternion.Euler(slotRotation.eulerAngles.x, slotRotation.eulerAngles.y + 180f, slotRotation.eulerAngles.z);
                    g.transform.DOMove(slots.Children[i].transform.position, duration);
                    g.transform.DORotate(slots.Children[i].transform.rotation.eulerAngles, 0.1f);
                }
                else
                {
                    // Sequence seq = DOTween.Sequence();
                    // seq.Append(g.transform.DOMove(slots.Children[i].transform.position, duration));
                    // seq.Insert(0f, g.transform.DORotate(slots.Children[i].transform.rotation.eulerAngles, 0.1f));
                    // seq.OnComplete(() => {
                    //     CommandComplete();
                    //     Command.CommandExecutionComplete();
                    // });
                    // Use DOMove to move the card to the position of its new slot
                    g.transform.DOMove(slots.Children[i].transform.position, duration);
                    g.transform.DORotate(slots.Children[i].transform.rotation.eulerAngles, 0.1f);
                    // Command.CommandExecutionComplete();
                }

            }
        }
        //CommandComplete();
        Command.CommandExecutionComplete();
    }

    // CARD DRAW METHODS

    // creates a card and returns a new card as a GameObject
    GameObject CreateACardAtPosition(CardAsset c, Vector3 position, Vector3 eulerAngles)
    {
        // Instantiate a card depending on its type
        GameObject card;
        if (c.MaxHealth > 0)
        {
            // this card is a creature card
            card = GameObject.Instantiate(GlobalSettings.Instance.CreatureCardPrefab, position, Quaternion.Euler(eulerAngles)) as GameObject;
        }
        else
        {
            // this is a spell: checking for targeted or non-targeted spell
            if (c.Targets == TargetingOptions.NoTarget)
                card = GameObject.Instantiate(GlobalSettings.Instance.NoTargetSpellCardPrefab, position, Quaternion.Euler(eulerAngles)) as GameObject;
            else
            {
                card = GameObject.Instantiate(GlobalSettings.Instance.TargetedSpellCardPrefab, position, Quaternion.Euler(eulerAngles)) as GameObject;
                // pass targeting options to DraggingActions
                DragSpellOnTarget dragSpell = card.GetComponentInChildren<DragSpellOnTarget>();
                dragSpell.Targets = c.Targets;
            }

        }

        // apply the look of the card based on the info from CardAsset
        OneCardManager manager = card.GetComponent<OneCardManager>();
        manager.cardAsset = c;
        manager.ReadCardFromAsset();

        return card;
    }

    // gives player a new card from a given position
    public void GivePlayerACard(CardAsset c, int UniqueID, bool fast = false, bool fromDeck = true)
    {
        GameObject card;
        if (fromDeck)
            card = CreateACardAtPosition(c, DeckTransform.position, new Vector3(0f, -179f, 0f));
        else
            card = CreateACardAtPosition(c, OtherCardDrawSourceTransform.position, new Vector3(0f, 0f, 0f));
        // Check if hand is full
        // Suppose 'transform' is the Transform you want to change
        if (CardsInHand.Count >= MAX_CARDS_IN_HAND) // Assuming MAX_CARDS_IN_HAND is a constant representing max hand size
        {
            // Shake and destroy the card at the preview spot
            Sequence seq = DOTween.Sequence();
            seq.Append(card.transform.DOMove(DrawPreviewSpot.position, GlobalSettings.Instance.CardTransitionTime));
            if (TakeCardsOpenly)
                seq.Append(card.transform.DORotate(Vector3.zero, 0.2f));
            seq.Append(card.transform.DOShakePosition(1, 0.5f, 10, 40, true, false)) // Shake for 1 second with a strength of 0.5
              .OnComplete(() =>
              {
                  HoverPreview.PreviewsAllowed = true;
                  Destroy(card);
              }); // Destroy the card after shaking
            return;
        }
        // Set a tag to reflect where this card is
        foreach (Transform t in card.GetComponentsInChildren<Transform>())
            t.tag = owner.ToString() + "Card";
        // pass this card to HandVisual class
        int newIndex = AddCard(card);

        // Bring card to front while it travels from draw spot to hand
        WhereIsTheCardOrCreature w = card.GetComponent<WhereIsTheCardOrCreature>();
        w.BringToFront();
        w.Slot = newIndex;
        w.VisualState = VisualStates.Transition;

        // pass a unique ID to this card.
        IDHolder id = card.AddComponent<IDHolder>();
        id.UniqueID = UniqueID;
        float slotsAnimate = 0.5f;
        // move card to the hand;
        Sequence s = DOTween.Sequence();
        if (!fast)
        {
            // Debug.Log ("Not fast!!!");
            s.Append(card.transform.DOMove(DrawPreviewSpot.position, GlobalSettings.Instance.CardTransitionTime));
            if (TakeCardsOpenly)
                s.Append(card.transform.DORotate(Vector3.zero, 0.2f));
            // else
            //     s.Insert(0f, card.transform.DORotate(new Vector3(0f, 179f, 0f), GlobalSettings.Instance.CardTransitionTime));
            s.Insert(1f, card.transform.DOScale(2f, 0.05f)); // scale up in half of the transition time
            s.AppendInterval(GlobalSettings.Instance.CardPreviewTime);
            s.Append(card.transform.DOScale(1f, 0.05f)); // scale up in half of the transition time
            // displace the card so that we can select it in the scene easier.
            //s.Append(card.transform.DOLocalMove(slots.Children[newIndex].transform.position, GlobalSettings.Instance.CardTransitionTime));
        }
        else
        {
            // displace the card so that we can select it in the scene easier.
            //s.Append(card.transform.DOLocalMove(slots.Children[newIndex].transform.localPosition, GlobalSettings.Instance.CardTransitionTimeFast));
            transform.rotation = Quaternion.identity;
            slotsAnimate = GlobalSettings.Instance.CardTransitionTimeFast;
            s.AppendInterval(0.35f);
        }

        s.OnComplete(() =>
        {
            ChangeLastCardStatusToInHand(card, w);
            PlaceCardsOnNewSlotsAnimate(slotsAnimate);
            OnHandChange?.Invoke();
        });
    }

    // this method will be called when the card arrived to hand 
    void ChangeLastCardStatusToInHand(GameObject card, WhereIsTheCardOrCreature w)
    {
        //Debug.Log("Changing state to Hand for card: " + card.gameObject.name);
        if (owner == AreaPosition.Low)
            w.VisualState = VisualStates.LowHand;
        else
            w.VisualState = VisualStates.TopHand;

        // set correct sorting order
        w.SetHandSortingOrder();
        // end command execution for DrawACArdCommand

    }


    // PLAYING SPELLS

    // 2 Overloaded method to show a spell played from hand
    public void PlayACardFromHand(int CardID)
    {
        GameObject card = IDHolder.GetGameObjectWithID(CardID);
        PlayACardFromHand(card);
    }

    public void PlayACardFromHand(GameObject CardVisual)
    {

        CardVisual.GetComponent<WhereIsTheCardOrCreature>().VisualState = VisualStates.Transition;
        RemoveCard(CardVisual);
        Command.CommandExecutionComplete();

        // CardVisual.transform.SetParent(null);

        // Sequence s = DOTween.Sequence();
        // s.Append(CardVisual.transform.DOMove(PlayPreviewSpot.position, 1f));
        // s.Insert(0f, CardVisual.transform.DORotate(Vector3.zero, 1f));
        // s.AppendInterval(2f);
        // s.OnComplete(() =>
        //     {
        //         //Command.CommandExecutionComplete();
        //         Destroy(CardVisual);
        //     });
    }

    internal void DestroyCardVisual(int uniqueCardID)
    {
        GameObject card = IDHolder.GetGameObjectWithID(uniqueCardID);
        StartCoroutine(DestroyWithDelay(card, 0.2f, () =>
{
    Command.CommandExecutionComplete();
    // additional code here
}));

    }
    private IEnumerator DestroyWithDelay(GameObject gameObject, float delay, Action callback)
    {
        yield return new WaitForSeconds(delay);
        new CallbackCommand(() =>
        {
            PlayStackPositionManager.Instance.RemoveCard(gameObject);
        }).AddToQueue();

        callback?.Invoke();
    }
}
