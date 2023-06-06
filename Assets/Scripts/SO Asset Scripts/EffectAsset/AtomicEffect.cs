using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AtomicEffect : ScriptableObject
{
    public abstract void ActivateEffect(int specialAmount = 0, IIdentifiable[] targets = null, TargetType targetType = TargetType.EnemyCreature);
}
