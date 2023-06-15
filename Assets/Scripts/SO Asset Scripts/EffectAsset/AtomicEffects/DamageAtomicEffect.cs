using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Effects/DamageEffect")]
public class DamageAtomicEffect : AtomicEffect
{

    public override void ActivateEffect(int specialAmount = 0, Queue<IIdentifiable> targets = null, TargetType targetType = TargetType.EnemyCreature)
    {
        Debug.Log("Activating Deal Damage Effect");
        IIdentifiable target = targets.Peek();
        if (target is ICharacter characterTarget)
        {
            new DealDamageCommand(characterTarget.ID, specialAmount, healthAfter: characterTarget.Health - specialAmount).AddToQueue();
        }
    }
}