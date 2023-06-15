using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
[CreateAssetMenu(menuName = "Effects/DrawCardConditionedEffect")]
public class DrawCardsWithConditions : AtomicEffect
{
    
    [SerializeField] private List<Condition> conditions;
    public override void ActivateEffect(int specialAmount = 0, Queue<IIdentifiable> targets = null, TargetType targetType = TargetType.EnemyCreature)
    {
        Debug.Log("Activating Draw Card with Condition Effect");
        // Check that each condition returns true using All LINQ method
        if (!conditions.All(condition => condition.ConditionSatisfied(specialAmount, targets, targetType)))
        {
            Debug.Log("A condition was not satisfied. Effect will not be activated.");
            return;
        }
        Player p = TurnManager.Instance.whoseTurn;
        p.DrawCards(specialAmount);
        //Debug.Log("Player " + p.gameObject.name + " should draw " + specialAmount + " cards.");

    }

}
