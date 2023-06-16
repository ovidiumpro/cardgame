using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Effects/DrawCardEffect")]
public class DrawCardAtomicEffect : AtomicEffect
{
    public override void ActivateEffect(int specialAmount = 0, Queue<IIdentifiable> targets = null, TargetType targetType = TargetType.PlayerCard)
    {
        // Assuming there is a method to handle drawing cards
        DrawCards(specialAmount);
    }

    public override EffectTargetData TargetInfo()
    {
        return null;
    }

    private void DrawCards(int amount)
    {
        Player p = TurnManager.Instance.whoseTurn;
        p.DrawCards(amount);
        //Debug.Log("Player " + p.gameObject.name + " should draw " + specialAmount + " cards.");
    }
}
