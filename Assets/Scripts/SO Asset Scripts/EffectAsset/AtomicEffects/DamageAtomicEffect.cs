using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Effects/DamageEffect")]
public class DamageAtomicEffect : AtomicEffect
{

    public override void ActivateEffect(int specialAmount = 0, Queue<IIdentifiable> targets = null, TargetType targetType = TargetType.EnemyCreature)
    {
        Debug.Log("Activating Deal Damage Effect");
        IIdentifiable target;
        if (!targets.Any())
        {
            return;
        }
        if (propagateTarget)
            target = targets.Peek();
        else target = targets.Dequeue();
        if (target is ICharacter characterTarget)
        {
            new DealDamageCommand(characterTarget.ID, specialAmount, healthAfter: characterTarget.Health - specialAmount).AddToQueue();
        }
    }

    public override EffectTargetData TargetInfo()
    {
        return new EffectTargetData(targetingOptions, "Deal damage to a character. ");
    }
}