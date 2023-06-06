using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CreatureEffectFactory
{
    private static readonly Dictionary<string, Func<Player, CreatureLogic, int, CreatureEffect>> registry =
        new Dictionary<string, Func<Player, CreatureLogic, int, CreatureEffect>>();
    static CreatureEffectFactory()
    {
        // RegisterCreatureEffect("CreatureEffectType1", (owner, creature, specialAmount) => new CreatureEffect(owner, creature, specialAmount));
        // RegisterCreatureEffect("CreatureEffectType2", (owner, creature, specialAmount) => new CreatureEffect(owner, creature, specialAmount));
        // Add other CreatureEffect types here.
    }
    public static void RegisterCreatureEffect(string scriptName, Func<Player, CreatureLogic, int, CreatureEffect> creator)
    {
        registry.Add(scriptName, creator);
    }
    public static CreatureEffect CreateCreatureEffect(string scriptName, Player owner, CreatureLogic creature, int specialAmount)
    {
        if (registry.ContainsKey(scriptName))
        {
            return registry[scriptName](owner, creature, specialAmount);
        }
        else
        {
            // Possibly throw an exception here or handle the case when a creature effect with a given name is not found.
            return null;
        }
    }
}
