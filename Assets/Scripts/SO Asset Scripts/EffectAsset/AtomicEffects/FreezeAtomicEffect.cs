using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Effects/FreezeEffect")]
public class FreezeAtomicEffect : AtomicEffect
{
    public override void ActivateEffect(int specialAmount = 0, IIdentifiable[] targets = null, TargetType targetType = TargetType.EnemyCreature)
    {
        // code to freeze target
    }
}
