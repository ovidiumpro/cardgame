using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using CG.Cards;

public class DragCreatureAttack : DraggingActions
{

    public SpriteRenderer sr;
    public Transform TargetGO;
    public LineRenderer lr;
    // reference to WhereIsTheCardOrCreature to track this object`s state in the game
    private WhereIsTheCardOrCreature whereIsThisCreature;
    // the pointy end of the arrow, should be called "Triangle" in the Hierarchy
    public Transform triangle;
    // SpriteRenderer of triangle. We need this to disable the pointy end if the target is too close.
    public SpriteRenderer triangleSR;
    // when we stop dragging, the gameObject that we were targeting will be stored in this variable.
    private GameObject Target;
    // Reference to creature manager, attached to the parent game object
    private OneCreatureManager manager;
    private Draggable draggable;

    private int lowPlayerID, topPlayerID;

    void Awake()
    {

        lr.sortingLayerName = "AboveEverything";
        manager = GetComponentInParent<OneCreatureManager>();
        whereIsThisCreature = GetComponentInParent<WhereIsTheCardOrCreature>();
        draggable = GetComponent<Draggable>();
    }
    private void Start() {
         lowPlayerID = GlobalSettings.Instance.LowPlayer.ID;
        topPlayerID = GlobalSettings.Instance.TopPlayer.ID;
    }
    public override Transform GetTargetTransform()
    {
        return TargetGO;
    }

    public override bool CanDrag
    {
        get
        {
            // we can drag this card if 
            // a) we can control this our player (this is checked in base.canDrag)
            // b) creature "CanAttackNow" - this info comes from logic part of our code into each creature`s manager script
            return base.CanDrag && manager.CanAttackNow;
            //return true;
        }
    }
    public override void OnStartDrag()
    {
        whereIsThisCreature.VisualState = VisualStates.Dragging;
        // enable target graphic
        sr.enabled = true;
        // enable line renderer to start drawing the line.
        lr.enabled = true;
    }
   
   
    public override void OnDraggingInUpdate()
    {
        DraggingActions.UpdateTargetPosition(transform,TargetGO, lr, triangleSR);

    }

    public override void OnEndDrag()
    {
        Target = null;
        RaycastHit[] hits = GetRaycastHits();
        Target = GetTargetForAttack(hits);

        bool targetValid = false;

        if (Target != null)
        {
            int targetID = Target.GetComponent<IDHolder>().UniqueID;
            Debug.Log("Target ID: " + targetID);
            if (targetID == GlobalSettings.Instance.LowPlayer.PlayerID || targetID == GlobalSettings.Instance.TopPlayer.PlayerID)
            {
                // attack character
                Debug.Log("Attacking " + Target);
                Debug.Log("TargetID: " + targetID);
                CreatureLogic.CreaturesCreatedThisGame[GetComponentInParent<IDHolder>().UniqueID].GoFace();
                targetValid = true;
            }
            else if (CreatureLogic.CreaturesCreatedThisGame[targetID] != null)
            {
                // if targeted creature is still alive, attack creature
                targetValid = !CreatureLogic.CreaturesCreatedThisGame[targetID].isDead;
                if (targetValid)
                {
                    CreatureLogic.CreaturesCreatedThisGame[GetComponentInParent<IDHolder>().UniqueID].AttackCreatureWithID(targetID);
                    Debug.Log("Attacking " + Target);
                }

            }

        }

        if (!targetValid)
        {
            // not a valid target, return
            whereIsThisCreature.VisualState = VisualStates.LowTable;
            whereIsThisCreature.SetTableSortingOrder();
        }

        DeactivateComponentsAfterAttack();

    }

    private void DeactivateComponentsAfterAttack()
    {
        // return target and arrow to original position
        TargetGO.localPosition = new Vector3(0f, 0f, -0.1f);
        sr.enabled = false;
        lr.enabled = false;
        triangleSR.enabled = false;
    }

    private GameObject GetTargetForAttack(RaycastHit[] hits)
    {
        foreach (RaycastHit h in hits)
        {
            if ((h.transform.tag == "TopPlayer" && this.tag == "LowCreature") ||
                (h.transform.tag == "LowPlayer" && this.tag == "TopCreature"))
            {
                // go face
                return h.transform.gameObject;
            }
            else if ((h.transform.tag == "TopCreature" && this.tag == "LowCreature") ||
                    (h.transform.tag == "LowCreature" && this.tag == "TopCreature"))
            {
                // hit a creature, save parent transform
                return h.transform.gameObject;
            }

        }
        return null;
    }
   

    private RaycastHit[] GetRaycastHits()
    {
        RaycastHit[] hits;
        Vector3 origin = Camera.main.transform.position;
        Vector3 direction = (-Camera.main.transform.position + TargetGO.position).normalized;
        float maxDistance = 30f;
        // TODO: raycast here anyway, store the results in 
        hits = Physics.RaycastAll(origin, direction, maxDistance);
        return hits;
    }

    // NOT USED IN THIS SCRIPT
    protected override bool DragSuccessful()
    {
        return true;
    }
}
