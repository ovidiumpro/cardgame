using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Condition : ScriptableObject
{
   public abstract bool ConditionSatisfied(int specialAmount = 0, Queue<IIdentifiable> targets = null, TargetType targetType = TargetType.EnemyCreature);
}
