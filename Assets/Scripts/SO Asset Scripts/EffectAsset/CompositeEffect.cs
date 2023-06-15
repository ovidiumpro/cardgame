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
            atomicEffects[i].ActivateEffect(specialAmounts[i], targets, targetType);
        }
    }
}
