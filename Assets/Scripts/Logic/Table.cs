using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Table : MonoBehaviour
{
    public List<CreatureLogic> CreaturesOnTable = new List<CreatureLogic>();
    private int maxCreatures = 8;

    public void PlaceCreatureAt(int index, CreatureLogic creature)
    {
        if (canAddCreature())
        {
            CreaturesOnTable.Insert(index, creature);
        }
    }

    public void AddCreature(CreatureLogic creature)
    {
        if (canAddCreature())
        {
            CreaturesOnTable.Add(creature);
        }
    }
    private bool canAddCreature() {
        return CreaturesOnTable.Count < maxCreatures;
    }
    public void RemoveCreatureAt(int index) {
        if (index >=0 && index < CreaturesOnTable.Count) {
            CreaturesOnTable.RemoveAt(index);
        }
    }
    public void RemoveAllFromList(List<CreatureLogic> creaturesToRemove)
    {
        foreach (var creature in creaturesToRemove)
        {
            CreaturesOnTable.Remove(creature);
        }
    }
}
        
