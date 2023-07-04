using UnityEngine;
using System.Collections;
using DG.Tweening;

public class PlayACreatureCommand : Command
{
    private CardLogic cl;
    private int tablePos;
    private Player p;
    private int creatureID;

    public PlayACreatureCommand(CardLogic cl, Player p, int tablePos, int creatureID)
    {
        this.p = p;
        this.cl = cl;
        this.tablePos = tablePos;
        this.creatureID = creatureID;
    }

    public override void StartCommandExecution()
    {
        // remove and destroy the card in hand 
        //HandVisual PlayerHand = p.PArea.handVisual;
        GameObject card = IDHolder.GetGameObjectWithID(cl.UniqueCardID);
        //PlayerHand.RemoveCard(card);
        TableVisual tableVisual = p.PArea.tableVisual;
        // Get Slots gameobject
        Transform slotsTransform = tableVisual.transform.Find("Slots");

        // Get the desired slot position
        Transform targetSlot = slotsTransform.GetChild(tablePos);
        if (!tableVisual.ThereIsRoomOnTable())
        {
            Command.CommandExecutionComplete();
        }
        else
        {
            // Create a sequence for movement and scaling
            Sequence seq = DOTween.Sequence();
            seq.Append(card.transform.DOMove(targetSlot.position, 0.5f));
            seq.Insert(0f, card.transform.DOScale(0.1f, 0.5f));
            seq.InsertCallback(0.3f, () =>
            {
                HoverPreview.PreviewsAllowed = true;
                // move this card to the spot 
                p.PArea.tableVisual.AddCreatureAtIndex(cl.ca, creatureID, tablePos);
            });
            // Destroy the card at the end of the sequence
            seq.OnComplete(() =>
            {
                tableVisual.TriggerOnCreatureEnter(creatureID);

            });
            seq.Play();
        }


    }
}
