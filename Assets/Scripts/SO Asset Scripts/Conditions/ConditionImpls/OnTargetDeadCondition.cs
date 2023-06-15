using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Conditions/OnTargetDead")]
public class OnTargetDeadCondition : Condition
{
    public override bool ConditionSatisfied(int specialAmount = 0, Queue<IIdentifiable> targets = null, TargetType targetType = TargetType.EnemyCreature)
    {
        IIdentifiable obj;
        if (targets == null || !targets.TryPeek(out obj)) {
            return false;
        }
        if (obj is CreatureLogic creature) {
            return creature.Health <= 0;
        }
        return false;

    }
}
