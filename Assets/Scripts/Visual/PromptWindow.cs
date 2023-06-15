using System.Collections;
using System.Collections.Generic;
using CG.Cards;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class PromptWindow : MonoBehaviour
{
    public static PromptWindow Instance { get; private set; }

    public GameObject Canvas;
    public GameObject SpellCardPrefab;
    public GameObject CreatureCardPrefab;
    public GameObject CardDisplayArea;
    public Button PreviousButton;
    public Button NextButton;
    public Text DisplayTextPrompt;
    public int itemsPerPage = 6;
    private int totalItems = 0;
    private int currentPage = 1;
    private int totalPages = 1;
    private int requiredTargets = 3;
    //private List<CardLogic> cards = new List<CardLogic>();
    public List<CardAsset> cardData = new List<CardAsset>();
    private List<CardAsset> selectedCards = new List<CardAsset>();
    private List<int> selectedIndexes = new List<int>();
    private List<GameObject> cardDisplays = new List<GameObject>();
    // Start is called before the first frame update

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            // Assuming this script is attached to the Canvas GameObject itself
            this.Canvas.transform.localScale = new Vector3(0, this.Canvas.transform.localScale.y, this.Canvas.transform.localScale.z);
            this.Canvas.SetActive(false);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void OnEnable()
    {
        SelectableCardGlowController.OnCardClicked += HandleCardClick;
    }

    private void OnDisable()
    {
        SelectableCardGlowController.OnCardClicked -= HandleCardClick;
    }

    private void HandleCardClick(int cardIndex, bool selected)
    {
        // Handle the card click event here
        Debug.Log("Card at index " + cardIndex + " was clicked. Its selected state is " + selected);
        if (selected)
        {
            selectedIndexes.Add(cardIndex);
            if (selectedIndexes.Count == requiredTargets)
            {

                selectedIndexes.ForEach(i =>
                {
                    selectedCards.Add(cardData[i]);
                });
                Debug.Log("Selected cards: ");
                selectedCards.ForEach(c =>
                {
                    Debug.Log(c.name);
                });
                ClosePrompt();
            }
        }
        else
        {
            selectedIndexes.Remove(cardIndex);
        }
    }
    public void DisplayPrompt()
    {
        Canvas.SetActive(true);
        DisplayTextPrompt.text = "Select 3 different cards.";
        Canvas.GetComponent<RectTransform>().DOScaleX(0.01f, 0.5f); // animate over 1 second

        //cards = newCards;
        //requiredTargets = numTargets;
        foreach (var card in cardData)
        {
            GameObject cardAsset = null;
            switch (card.CardType)
            {
                case ECardType.Spell:
                    {
                        cardAsset = GameObject.Instantiate(SpellCardPrefab, CardDisplayArea.transform);
                        break;
                    }
                case ECardType.Creature:
                    {
                        cardAsset = GameObject.Instantiate(CreatureCardPrefab, CardDisplayArea.transform);
                        break;
                    }
            }
            if (card.CardType == ECardType.Spell) { }
            OneCardManager manager = cardAsset.GetComponent<OneCardManager>();
            manager.cardAsset = card;
            manager.ReadCardFromAsset();
            cardAsset.SetActive(false);

            cardDisplays.Add(cardAsset);

        };
        Debug.Log("Updating display");
        totalItems = cardDisplays.Count;
        totalPages = (totalItems + itemsPerPage - 1) / itemsPerPage;
        UpdatePagination();
    }
    private void UpdatePagination()
    {
        // Ceiling division

        PreviousButton.interactable = currentPage > 1;
        NextButton.interactable = currentPage < totalPages;
        string s = "";
        // Enable/Disable cards based on the current page
        for (int i = 0; i < cardDisplays.Count; i++)
        {
            bool shouldShow = i >= (currentPage - 1) * itemsPerPage && i < currentPage * itemsPerPage;
            cardDisplays[i].SetActive(shouldShow);
            if (shouldShow) s += " " + i;
        }
        Debug.Log("Showing cards: " + s);
    }

    public void ClosePrompt()
    {
        Canvas.GetComponent<RectTransform>().DOScaleX(0f, 0.5f).OnComplete(() =>
        {
            Canvas.SetActive(false);

        });
        cardDisplays.ForEach(Destroy);
        cardDisplays = new List<GameObject>();
        currentPage = 1;
        totalPages = 1;
        totalItems = 0;
        selectedCards = new List<CardAsset>();
        selectedIndexes = new List<int>();
        // Execute the completion command.
        //Command.CommandExecutionComplete();
    }
    public void ChangePage(int value)
    {
        currentPage += value;
        UpdatePagination();
    }

}
