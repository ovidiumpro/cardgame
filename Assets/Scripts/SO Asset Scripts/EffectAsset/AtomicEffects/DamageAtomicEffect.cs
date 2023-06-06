using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Effects/DamageEffect")]
public class DamageAtomicEffect : AtomicEffect
{
    public int damageAmount = 1;

    public override void ActivateEffect(int specialAmount = 0, IIdentifiable[] targets = null, TargetType targetType = TargetType.EnemyCreature)
    {
        // code to deal damage
    }
}