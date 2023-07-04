using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AllCreaturesDieCommand : Command
{
    private List<CreatureLogic> CreaturesToDie;
    public AllCreaturesDieCommand(List<CreatureLogic> creatures)
    {
        this.CreaturesToDie = creatures;

    }
    public override void StartCommandExecution()
    {
        if (CreaturesToDie.Count == 0) return;
        var creatureGroupsByOwner = CreaturesToDie.GroupBy(creature => creature.owner);
        foreach (var group in creatureGroupsByOwner)
    {
        group.Key.table.RemoveAllFromList(group.ToList());
    }
        //TODO access each owner's table property and call the RemoveAllFromList(List<CreatureLogic> creatures) method. 
        Debug.Log("Accessing Table Visual to kill creatures");
        TableVisual.MassRemoveCreatures(creatureGroupsByOwner);
    }
}
