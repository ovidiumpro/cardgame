using System.Collections;
using System.Collections.Generic;
using CG.Cards;
using UnityEngine;

public abstract class AtomicEffect : ScriptableObject
{
     public bool propagateTarget = false;
     public TargetingOptions targetingOptions = TargetingOptions.NoTarget;
    public abstract void ActivateEffect(int specialAmount = 0, Queue<IIdentifiable> targets = null, TargetType targetType = TargetType.EnemyCreature);
    public abstract EffectTargetData TargetInfo();
}
