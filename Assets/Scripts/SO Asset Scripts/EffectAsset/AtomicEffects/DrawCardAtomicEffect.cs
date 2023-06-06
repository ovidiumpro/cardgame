using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Effects/DrawCardEffect")]
public class DrawCardsEffect : AtomicEffect
{
    public override void ActivateEffect(int specialAmount = 0, IIdentifiable[] targets = null, TargetType targetType = TargetType.PlayerCard)
    {
        // Assuming there is a method to handle drawing cards
        DrawCards(specialAmount);
    }
    
    private void DrawCards(int amount)
    {
        Debug.Log($"Drawing {amount} cards");
        // Logic to draw cards
    }
}
