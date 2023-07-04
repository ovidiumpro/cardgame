using System.Collections;
using System.Collections.Generic;
using CG.Cards;
using UnityEngine;

public class TargetSelector : MonoBehaviour
{
    public SpriteRenderer sr;
    public Transform TargetGO;
    public LineRenderer lr;
    // the pointy end of the arrow, should be called "Triangle" in the Hierarchy
    public Transform triangle;
    // SpriteRenderer of triangle. We need this to disable the pointy end if the target is too close.
    public SpriteRenderer triangleSR;
    // when we stop dragging, the gameObject that we were targeting will be stored in this variable.
    private GameObject Target;
    // Reference to creature manager, attached to the parent game object
    private OneCreatureManager manager;
    private Draggable draggable;

    private int currentlySelectingTarget = 0;
    private EffectTargetData currentTarget;

    private List<EffectTargetData> targetsMetadata = new List<EffectTargetData>();
    private bool selectingTargets = false;
    public bool SelectingTargets {
        get {
            return selectingTargets;
        }
    }
    private List<int> selectedTargetsForEffect = new List<int>();

    private int lowPlayerID, topPlayerID;

    private List<GameObject> extraLRs = new List<GameObject>();
    // Start is called before the first frame update
    void Start()
    {
        draggable = GetComponent<Draggable>();
    }

    private void Update()
    {

        if (currentTarget == null || !selectingTargets) return;
        //enable and update targeting position
        //input left mouse button listener to select target
        //input right mouse button to cancel effect
        draggable.SetTransformPositionToCursor(TargetGO);
        DraggingActions.UpdateTargetPosition(transform, TargetGO, lr, triangleSR);
        if (Input.GetMouseButtonDown(0))
        {
            OnTargetSelected();
        }
        if (Input.GetMouseButtonDown(1))
        {
            //lr.positionCount = 0;
            // not a valid target, return
            DeactivateComponentsAfterAttack();
        }
    }

    private void OnTargetSelected()
    {
        Target = null;
        RaycastHit[] hits = GetRaycastHits();
        Target = GetSelectedTargetFromHits(hits);

        if (Target != null && isTargetValid(currentTarget))
        {
            // check of we should play this spell depending on targeting options
            int targetID = Target.GetComponent<IDHolder>().UniqueID;
            selectedTargetsForEffect.Add(targetID);

            if (SelectedAllTargets() || NoEligibleTargetsLeftForNext())
            {
                selectingTargets = false;
                //lr.positionCount = 0;
                DeactivateComponentsAfterAttack();
            }
            else
            {
                //continue with selecting targets by: drawing a line to current target, allowing target to continuously follow cursor untill all targets have been selected
                GameObject extraLR = CreateExtraLR(Target);
                extraLRs.Add(extraLR);
                MessageManager.Instance.RefreshMessagePanel(currentTarget.promptMessage, 0.1f);
            }
        }
        else if (Target == null)
        {
            Debug.Log("Not a valid target");
        }
        Target = null;

    }

    private bool SelectedAllTargets()
    {
        if (selectedTargetsForEffect.Count == targetsMetadata.Count)
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

    private bool NoEligibleTargetsLeftForNext()
    {
        Player owner = GetEffectOwner();
        List<int> enemyCreatures = owner.otherPlayer.table.CreaturesOnTable.ConvertAll(cl => cl.ID);
        List<int> friendlyCreatures = owner.table.CreaturesOnTable.ConvertAll(cl => cl.ID);
        // we also have lowPLayerID and topPlayerID available
        // selectedTargets currently holds all ids of selected targets.
        // currentlySelectingTarget.targetType is of type TargetingOptions
        //TODO: based on the value of currentlySelectingTarget.targetType, decide whether there are ny unpicked IDs left to choose from
        // Removing already selected targets from the respective lists
        enemyCreatures.RemoveAll(id => selectedTargetsForEffect.Contains(id));
        friendlyCreatures.RemoveAll(id => selectedTargetsForEffect.Contains(id));
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
                return (friendlyCreatures.Count == 0 && enemyCreatures.Count == 0) && (selectedTargetsForEffect.Contains(owner.ID) && selectedTargetsForEffect.Contains(owner.otherPlayer.ID));
            case TargetingOptions.EnemyCharacters:
                return enemyCreatures.Count == 0 && selectedTargetsForEffect.Contains(owner.otherPlayer.ID);
            case TargetingOptions.YourCharacters:
                // You'll need to include logic here for Characters if they are separate from Creatures
                // Assuming that "Characters" include both Creatures and Players,
                // If there's no target left among creatures and the players were already targeted, then return true
                return friendlyCreatures.Count == 0 && selectedTargetsForEffect.Contains(owner.ID);
        }

        return false; // In case none of the options above were matched, you could return either true or false depending on what makes sense for your game. In this case, I chose to return false as a default.
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

    private Player GetEffectOwner()
    {
        // determine an owner of this card
        Player owner = null;
        if (tag.Contains("Low"))
            owner = GlobalSettings.Instance.LowPlayer;
        else
            owner = GlobalSettings.Instance.TopPlayer;
        return owner;
    }

    public void UpdateTargetsMetadataFromEffect(CompositeEffect effect)
    {
        targetsMetadata = effect?.PollTargetsRequired();
        currentlySelectingTarget = 0;
        selectedTargetsForEffect.Clear();
        if (targetsMetadata.Count == 0) return;
        selectingTargets = true;
        currentTarget = targetsMetadata[currentlySelectingTarget];
        MessageManager.Instance.ShowMessageInstant(currentTarget.promptMessage);
        sr.enabled = true;
        lr.enabled = true;
        draggable.InitializePositions();

    }

    public List<int> GetSelectedTargets()
    {
        return selectedTargetsForEffect;
    }

    private void DeactivateComponentsAfterAttack()
    {
        // return target and arrow to original position
        TargetGO.localPosition = new Vector3(0f, 0f, -0.1f);
        sr.enabled = false;
        lr.enabled = false;
        triangleSR.enabled = false;
        currentTarget = null;
        selectingTargets = false;
        extraLRs.ForEach(g =>
       {
           Destroy(g);
       });
        extraLRs.Clear();
        targetsMetadata.Clear();
        MessageManager.Instance.HideMessageInstant();
    }

    private GameObject GetSelectedTargetFromHits(RaycastHit[] hits)
    {
        foreach (RaycastHit h in hits)
        {
            if (h.transform.tag.Contains("Player") || h.transform.tag.Contains("Creature"))
            {
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
}
