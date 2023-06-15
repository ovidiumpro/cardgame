using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SelectableCardGlowController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
     public GameObject GlowImage; // Assign this in the inspector.
    public Color SelectedColor;
    public Color HoverColor;

    private bool isGlowing;
    private Image glowImageComponent;

    public delegate void CardClicked(int cardIndex, bool selected);
public static event CardClicked OnCardClicked;

    private void Start() 
    {
        // Make sure the glow is off at the start.
        GlowImage.SetActive(false);

        glowImageComponent = GlowImage.GetComponent<Image>();
        glowImageComponent.color = HoverColor;
    }

    public void OnPointerEnter(PointerEventData eventData) 
    {
        // When the mouse enters the card, activate the glow.
        GlowImage.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData) 
    {
        // When the mouse leaves the card, deactivate the glow unless it's selected.
        if (!isGlowing) {
            GlowImage.SetActive(false);
        }
    }

    public void OnPointerClick(PointerEventData eventData) 
    {
        // When the card is clicked, toggle whether it's selected or not.
        isGlowing = !isGlowing;
        GlowImage.SetActive(isGlowing);

        if (isGlowing) {
            // Change color and scale up the glow image when selected
            glowImageComponent.DOColor(SelectedColor, 0.3f);
            GlowImage.transform.DOScale(1.1f, 0.3f);
        }
        else {
            // Change color and scale down the glow image when deselected
            glowImageComponent.DOColor(HoverColor, 0.3f);
            GlowImage.transform.DOScale(1.0f, 0.3f);
        }
        // If any listeners are subscribed to the OnCardClicked event, raise the event.
    if (OnCardClicked != null)
    {
        int cardIndex = transform.GetSiblingIndex();
        OnCardClicked(cardIndex, isGlowing);
    }
    }
}
