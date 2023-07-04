using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using CG.Cards;
using System.Text;
using System;
// holds the refs to all the Text, Images on the card
public class OneCardManager : MonoBehaviour {

    public CardAsset cardAsset;
    public OneCardManager PreviewManager;
    [Header("Text Component References")]
    public Text NameText;
    public Text ManaCostText;
    public Text DescriptionText;
    public Text HealthText;
    public Text AttackText;
    public Text CardTypeText;
 
    [Header("Image References")]
    public Image CardTopRibbonImage;
    public Image CardLowRibbonImage;
    public Image CardGraphicImage;
    
    public Image CardBodyImage;
    public Image CardFaceFrameImage;
    public Image CardFaceGlowImage;
    public Image CardBackGlowImage;

    public event Action OnCardLoaded;

    void Awake()
    {
        if (cardAsset != null)
            ReadCardFromAsset();
    }

    private bool canBePlayedNow = false;
    public bool CanBePlayedNow
    {
        get
        {
            return canBePlayedNow;
        }

        set
        {
            canBePlayedNow = value;

            CardFaceGlowImage.enabled = value;
        }
    }
    public void SetCardAsset(CardAsset ca) {
        cardAsset = ca;
        ReadCardFromAsset();
    }

    public void ReadCardFromAsset()
    {
        // universal actions for any Card
        // 1) apply tint
        if (cardAsset.characterAsset != null)
        {
            CardBodyImage.color = cardAsset.characterAsset.ClassCardTint;
            CardFaceFrameImage.color = cardAsset.characterAsset.ClassCardTint;
            CardTopRibbonImage.color = cardAsset.characterAsset.ClassRibbonsTint;
            CardLowRibbonImage.color = cardAsset.characterAsset.ClassRibbonsTint;
        }
        else
        {
            CardFaceFrameImage.color = Color.white;
        }
        // 2) add card name
        NameText.text = cardAsset.name;
        // 3) add mana cost
        ManaCostText.text = cardAsset.ManaCost.ToString();
        // 4) add description
        DescriptionText.text = cardAsset.Description;
        // 5) Change the card graphic sprite
        CardGraphicImage.sprite = cardAsset.CardImage;
        CardTypeText.text = EnumToStringWithSpaces(cardAsset.CardType);

        if (cardAsset.MaxHealth != 0)
        {
            // this is a creature
            AttackText.text = cardAsset.Attack.ToString();
            HealthText.text = cardAsset.MaxHealth.ToString();
        }

        if (PreviewManager != null)
        {
            // this is a card and not a preview
            // Preview GameObject will have OneCardManager as well, but PreviewManager should be null there
            PreviewManager.cardAsset = cardAsset;
            PreviewManager.ReadCardFromAsset();
        }
        OnCardLoaded?.Invoke();
    }
    private void Start() {
        OnCardLoaded?.Invoke();
    }
    public static string EnumToStringWithSpaces(ECardType value)
{
    string input = value.ToString();
    StringBuilder output = new StringBuilder();

    for (int i = 0; i < input.Length; i++)
    {
        if (i > 0 && char.IsUpper(input[i]))
        {
            output.Append(' ');
        }
        output.Append(input[i]);
    }

    return output.ToString();
}
}
