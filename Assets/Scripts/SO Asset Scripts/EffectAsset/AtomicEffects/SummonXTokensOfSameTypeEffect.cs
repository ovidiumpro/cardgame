using System.Collections;
using System.Collections.Generic;
using CG.Cards;
using UnityEngine;
[CreateAssetMenu(menuName = "Effects/SummonXTokensOfSameType")]
public class SummonXTokensOfSameTypeEffect : AtomicEffect
{
    public CardAsset creatureAsset;
    public override void ActivateEffect(int specialAmount = 0, Queue<IIdentifiable> targets = null, TargetType targetType = TargetType.EnemyCreature)
    {
        //Summon specialAmount number of creatureAssets on this player's board.
        //possibly need to create a command for this as it requires visual and logical changes.
        //call logic layer and it will handle the command to talk to visual itself
        Player p = TurnManager.Instance.whoseTurn;
        p.SummonCreatures(creatureAsset, specialAmount);
    }

    public override EffectTargetData TargetInfo()
    {
        return null;
    }
}
