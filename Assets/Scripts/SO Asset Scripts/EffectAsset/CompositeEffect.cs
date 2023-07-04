using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Effects/CompositeEffect")]
public class CompositeEffect : ScriptableObject
{
    [SerializeField] private List<AtomicEffect> atomicEffects;
    [SerializeField] private List<int> specialAmounts;

    public void ActivateEffects(Queue<IIdentifiable> targets = null, TargetType targetType = TargetType.EnemyCreature)
    {
       if (atomicEffects.Count != specialAmounts.Count)
        {
            Debug.LogError("The number of atomic effects does not match the number of special amounts!");
            return;
        }

        for (int i = 0; i < atomicEffects.Count; i++)
        {
            if (i > 0 && atomicEffects[i-1].canDestroyCreatures && !atomicEffects[i].canDestroyCreatures) {
                new CallbackCommand(() => CreatureLogic.HandleQueueEmpty()).AddToQueue();
            }
            atomicEffects[i].ActivateEffect(specialAmounts[i], targets, targetType);
        }
    }
    //targetType, promptMessage
    public List<EffectTargetData> PollTargetsRequired() {
        List<EffectTargetData> targets = new List<EffectTargetData>();
        for (int i = 0; i < atomicEffects.Count; i++)
        {
            EffectTargetData targetData = atomicEffects[i].TargetInfo();
            if (targetData != null) {
                if (i>0 && atomicEffects[i-1].propagateTarget) continue;
                targets.Add(targetData);
            }
        } 
        return targets;
    }
}
