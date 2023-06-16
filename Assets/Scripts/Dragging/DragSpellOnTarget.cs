using UnityEngine;
using System.Collections;
using DG.Tweening;
using CG.Cards;
using System.Collections.Generic;
using System;

public class DragSpellOnTarget : DraggingActions
{

    public TargetingOptions Targets = TargetingOptions.AllCharacters;
    public SpriteRenderer sr;
    public Transform TargetGO;
    public LineRenderer lr;
    private WhereIsTheCardOrCreature whereIsThisCard;
    private VisualStates tempVisualState;
    public Transform triangle;
    public SpriteRenderer triangleSR;
    private GameObject Target;
    private CardAsset ca;
    private int currentlySelectingTarget = 0;
    private EffectTargetData currentTarget;
    private OneCardManager cm;
    private Draggable draggable;

    private List<GameObject> extraLRs = new List<GameObject>();

    // List of targets selected
    private List<int> selectedTargets = new List<int>();
    private List<EffectTargetData> targetsMetadata = new List<EffectTargetData>();
    private int lowPlayerID, topPlayerID;

    void Awake()
    {
        // sr = GetComponent<SpriteRenderer>();
        // lr = GetComponentInChildren<LineRenderer>();

        // triangle = transform.Find("Triangle");
        // triangleSR = triangle.GetComponent<SpriteRenderer>();
        lr.sortingLayerName = "AboveEverything";
        cm = GetComponent<OneCardManager>();
        draggable = GetComponent<Draggable>();
        whereIsThisCard = GetComponent<WhereIsTheCardOrCreature>();
        cm.OnCardLoaded += ReadCardAsset;
    }
    void Start()
    {
        
        lowPlayerID = GlobalSettings.Instance.LowPlayer.ID;
        topPlayerID = GlobalSettings.Instance.TopPlayer.ID;
    }
    private void Update() {
        if (selectedTargets.Count > 0) {
            draggable.SetTransformPositionToCursor(TargetGO);
            OnDraggingInUpdate();
        }
    }
    private void OnDestroy() {
        cm.OnCardLoaded -= ReadCardAsset;    
    }
    private void ReadCardAsset() {
        ca = cm.cardAsset;
        targetsMetadata = ca.SpellEffect?.PollTargetsRequired();
    }

    public override void OnStartDrag()
    {
        tempVisualState = whereIsThisCard.VisualState;
        whereIsThisCard.VisualState = VisualStates.Dragging;
        sr.enabled = true;
        lr.enabled = true;
        currentlySelectingTarget = 0;
        if (currentlySelectingTarget > targetsMetadata.Count) return;
        currentTarget = targetsMetadata[currentlySelectingTarget];
        MessageManager.Instance.ShowMessageInstant(currentTarget.promptMessage);

    }

    public override void OnDraggingInUpdate()
    {
        // This code only draws the arrow
        Vector3 notNormalized = TargetGO.position - transform.position;
        Vector3 direction = notNormalized.normalized;
        float distanceToTarget = (direction * 2.3f).magnitude;
        if (notNormalized.magnitude > distanceToTarget)
        {
            // draw a line between the creature and the target
            lr.SetPositions(new Vector3[] { transform.position, TargetGO.position - direction * 2.3f });
            lr.enabled = true;

            // position the end of the arrow between near the target.
            triangleSR.enabled = true;
            triangleSR.transform.position = TargetGO.position - 1.5f * direction;

            // proper rotarion of arrow end
            float rot_z = Mathf.Atan2(notNormalized.y, notNormalized.x) * Mathf.Rad2Deg;
            triangleSR.transform.rotation = Quaternion.Euler(0f, 0f, rot_z - 90);
        }
        else
        {
            // if the target is not far enough from creature, do not show the arrow
            lr.enabled = false;
            triangleSR.enabled = false;
        }
        // Right click cancels the operation
        if (Input.GetMouseButtonDown(1))
        {
            lr.positionCount = 0;
            DeactivateComponents();
        }
        if (selectedTargets.Count > 0 && Input.GetMouseButtonDown(0)) {
            OnEndDrag();
        }
    }

    public override void OnEndDrag()
    {
        Target = null;
        RaycastHit[] hits;
        // TODO: raycast here anyway, store the results in 
        hits = Physics.RaycastAll(origin: Camera.main.transform.position,
            direction: (-Camera.main.transform.position + this.TargetGO.position).normalized,
            maxDistance: 30f);

        foreach (RaycastHit h in hits)
        {
            if (h.transform.tag.Contains("Player"))
            {
                // selected a Player
                Target = h.transform.gameObject;
            }
            else if (h.transform.tag.Contains("Creature"))
            {
                // hit a creature, save parent transform
                Target = h.transform.parent.parent.gameObject;
            }
        }


        if (Target != null && isTargetValid(currentTarget))
        {
            // determine an owner of this card
            Player owner = null;
            if (tag.Contains("Low"))
                owner = GlobalSettings.Instance.LowPlayer;
            else
                owner = GlobalSettings.Instance.TopPlayer;

            // check of we should play this spell depending on targeting options
            int targetID = Target.GetComponent<IDHolder>().UniqueID;
            selectedTargets.Add(targetID);

            if (SelectedAllTargets() || NoEligibleTargetsLeftForNext())
            {
                //End effect here
                owner.PlayASpellFromHand(GetComponent<IDHolder>().UniqueID, selectedTargets);
                DeactivateComponents();
            }
            else
            {
                //continue with selecting targets by: drawing a line to current target, allowing target to continuously follow cursor untill all targets have been selected
                GameObject extraLR = CreateExtraLR(Target);
                extraLRs.Add(extraLR);
                MessageManager.Instance.RefreshMessagePanel(currentTarget.promptMessage, 0.1f);
            }
        }
        else
        {
            // not a valid target, return
            whereIsThisCard.VisualState = tempVisualState;
            whereIsThisCard.SetHandSortingOrder();
            DeactivateComponents();
        }
    }

    private GameObject CreateExtraLR(GameObject target)
    {
        int i = extraLRs.Count;
        // Create a new GameObject to hold the LineRenderer for this segment
        GameObject segment = new GameObject("Line Segment " + i);
        segment.transform.SetParent(transform);
        // Add a new LineRenderer to the segment GameObject
        LineRenderer segmentLineRenderer = segment.AddComponent<LineRenderer>();

        // Set the properties for the segment's LineRenderer
        segmentLineRenderer.positionCount = 2;
        segmentLineRenderer.SetPosition(0, transform.position);
        segmentLineRenderer.SetPosition(1, target.transform.position);
        segmentLineRenderer.startWidth = 0.5f;
        segmentLineRenderer.endWidth = 0.5f;
        segmentLineRenderer.material = lr.material;
        Gradient gradient = new Gradient();

        // Set up gradient colors, blue at the start and red at the end
        GradientColorKey[] colorKey = new GradientColorKey[2];
        colorKey[0].color = Color.blue;
        colorKey[0].time = 0.0f;
        colorKey[1].color = Color.red;
        colorKey[1].time = 1.0f;

        // Set up alpha, if you want the line to be fully visible you would set alpha to 1
        GradientAlphaKey[] alphaKey = new GradientAlphaKey[2];
        alphaKey[0].alpha = 1.0f;
        alphaKey[0].time = 0.0f;
        alphaKey[1].alpha = 1.0f;
        alphaKey[1].time = 0.15f;

        gradient.SetKeys(colorKey, alphaKey);

        segmentLineRenderer.colorGradient = gradient;
        segmentLineRenderer.sortingLayerName = "AboveEverything";
        segmentLineRenderer.sortingOrder = 1;
        return segment;

    }

    private bool NoEligibleTargetsLeftForNext()
    {
        List<int> enemyCreatures = GlobalSettings.Instance.TopPlayer.table.CreaturesOnTable.ConvertAll(cl => cl.ID);
        List<int> friendlyCreatures = GlobalSettings.Instance.LowPlayer.table.CreaturesOnTable.ConvertAll(cl => cl.ID);
        // we also have lowPLayerID and topPlayerID available
        // selectedTargets currently holds all ids of selected targets.
        // currentlySelectingTarget.targetType is of type TargetingOptions
        //TODO: based on the value of currentlySelectingTarget.targetType, decide whether there are ny unpicked IDs left to choose from
        // Removing already selected targets from the respective lists
        enemyCreatures.RemoveAll(id => selectedTargets.Contains(id));
        friendlyCreatures.RemoveAll(id => selectedTargets.Contains(id));
        switch (currentTarget.targetType)
        {
            case TargetingOptions.NoTarget:
                return true;  // If there's no target, then there's nothing left to pick

            case TargetingOptions.AllCreatures:
                // If the union of friendly and enemy creatures list is empty, then there's no target left
                return friendlyCreatures.Count == 0 && enemyCreatures.Count == 0;

            case TargetingOptions.EnemyCreatures:
                // If enemy creatures list is empty, then there's no target left
                return enemyCreatures.Count == 0;

            case TargetingOptions.YourCreatures:
                // If friendly creatures list is empty, then there's no target left
                return friendlyCreatures.Count == 0;
            case TargetingOptions.AllCharacters:
                return (friendlyCreatures.Count == 0 && enemyCreatures.Count == 0) && (selectedTargets.Contains(lowPlayerID) && selectedTargets.Contains(topPlayerID));
            case TargetingOptions.EnemyCharacters:
                return enemyCreatures.Count == 0 && selectedTargets.Contains(topPlayerID);
            case TargetingOptions.YourCharacters:
                // You'll need to include logic here for Characters if they are separate from Creatures
                // Assuming that "Characters" include both Creatures and Players,
                // If there's no target left among creatures and the players were already targeted, then return true
                return friendlyCreatures.Count == 0 && selectedTargets.Contains(lowPlayerID);
        }

        return false; // In case none of the options above were matched, you could return either true or false depending on what makes sense for your game. In this case, I chose to return false as a default.
    }

    private bool SelectedAllTargets()
    {
        if (selectedTargets.Count == targetsMetadata.Count)
        {
            return true;
        }
        else
        {
            currentlySelectingTarget++;
            if (currentlySelectingTarget >= targetsMetadata.Count) return true;
            currentTarget = targetsMetadata[currentlySelectingTarget];
            return false;
        }
    }

    private bool isTargetValid(EffectTargetData currentTarget)
    {
        switch (currentTarget.targetType)
        {
            case TargetingOptions.AllCharacters:
                return true;

            case TargetingOptions.AllCreatures:
                if (Target.tag.Contains("Creature"))
                {
                    return true;
                }
                return false;
            case TargetingOptions.EnemyCharacters:
                if (Target.tag.Contains("Creature") || Target.tag.Contains("Player"))
                {
                    // had to check that target is not a card
                    if ((tag.Contains("Low") && Target.tag.Contains("Top"))
                       || (tag.Contains("Top") && Target.tag.Contains("Low")))
                    {
                        return true;
                    }
                }
                return false;
            case TargetingOptions.EnemyCreatures:
                if (Target.tag.Contains("Creature"))
                {
                    // had to check that target is not a card or a player
                    if ((tag.Contains("Low") && Target.tag.Contains("Top"))
                        || (tag.Contains("Top") && Target.tag.Contains("Low")))
                    {
                        return true;
                    }
                }
                return false;
            case TargetingOptions.YourCharacters:
                if (Target.tag.Contains("Creature") || Target.tag.Contains("Player"))
                {
                    // had to check that target is not a card
                    if ((tag.Contains("Low") && Target.tag.Contains("Low"))
                        || (tag.Contains("Top") && Target.tag.Contains("Top")))
                    {
                        return true;
                    }
                }
                return false;
            case TargetingOptions.YourCreatures:
                if (Target.tag.Contains("Creature"))
                {
                    // had to check that target is not a card or a player
                    if ((tag.Contains("Low") && Target.tag.Contains("Low"))
                        || (tag.Contains("Top") && Target.tag.Contains("Top")))
                    {
                        return true;
                    }
                }
                return false;
            default:
                Debug.LogWarning("Reached default case in DragSpellOnTarget! Suspicious behaviour!!");
                return false;
        }
    }

    private void DeactivateComponents()
    {
        // return target and arrow to original position
        // this position is special for spell cards to show the arrow on top
        TargetGO.localPosition = new Vector3(0f, 0f, -0.1f);
        sr.enabled = false;
        lr.enabled = false;
        triangleSR.enabled = false;
        extraLRs.ForEach(g =>
        {
            Destroy(g);
        });
        extraLRs.Clear();
        selectedTargets.Clear();
        MessageManager.Instance.HideMessageInstant();
    }

    // NOT USED IN THIS SCRIPT
    protected override bool DragSuccessful()
    {
        return true;
    }

    public override Transform GetTargetTransform()
    {
        return TargetGO;
    }
}
