using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SpellFactory
{
    // Start is called before the first frame update
    private static Dictionary<string, Func<SpellEffect>> registry = new Dictionary<string, Func<SpellEffect>>();
    static SpellFactory()
    {
        // Register each type of spell with its factory method
        // registry["Fireball"] = () => new FireballSpell();
        // registry["IceBlast"] = () => new IceBlastSpell();
        // ... Add the rest here
    }

    public static SpellEffect CreateSpell(string spellName)
    {
        if (registry.TryGetValue(spellName, out Func<SpellEffect> constructor))
        {
            return constructor();
        }
        else
        {
            Debug.Log($"No Spell with the name {spellName} found!");
            return null;
        }
    }
}
