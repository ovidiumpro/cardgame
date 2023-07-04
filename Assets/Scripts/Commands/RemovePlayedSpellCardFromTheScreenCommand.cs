using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemovePlayedSpellCardFromTheScreenCommand : Command
{
    private Player p;
    private CardLogic cl;

    public RemovePlayedSpellCardFromTheScreenCommand(Player p, CardLogic cl) {
        this.p = p;
        this.cl = cl;
    }
     public override void StartCommandExecution()
    {
        // remove and destroy the card in hand 
        // enable Hover Previews Back
        // move this card to the spot 
        p.PArea.handVisual.DestroyCardVisual(cl.UniqueCardID);
    }
}
