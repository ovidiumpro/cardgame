using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SummonCreaturesCommand : Command
{
    Player p;
    List<CreatureLogic> cl;

    public SummonCreaturesCommand(Player p, List<CreatureLogic> cl) 
    {
        this.p = p;
        this.cl = cl;
    }

    public override void StartCommandExecution()
    {
        TableVisual tableVisual = p.PArea.tableVisual;
        List<int> UniqueIds = cl.ConvertAll(c => c.UniqueCreatureID);
        tableVisual.SummonCreaturesOnTable(cl[0].ca, UniqueIds);
    }
}
