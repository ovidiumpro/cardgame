using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PredictCreatureSlotCommand : Command
{
    private int tablePos;
    private Player p;

    public PredictCreatureSlotCommand(int tablePos, Player p) {
        this.tablePos = tablePos;
        this.p = p;
    }
     public override void StartCommandExecution()
    {
        // remove and destroy the card in hand 
        // enable Hover Previews Back
        // move this card to the spot 
        p.PArea.tableVisual.PredictCreatureSlot(tablePos);
    }
}
