using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using CG.Cards;
using System.Linq;
using System;

public class TableVisual : MonoBehaviour
{
    // PUBLIC FIELDS

    // an enum that mark to whish caracter this table belongs. The alues are - Top or Low
    public AreaPosition owner;
    public CardAsset testAsset;
    
    [SerializeField] public int maxCreatures = 8;

    // a referense to a game object that marks positions where we should put new Creatures
    public SameDistanceChildren slots;

    public event Action OnTableChange;

    // PRIVATE FIELDS

    // list of all the creature cards on the table as GameObjects
    private List<GameObject> CreaturesOnTable = new List<GameObject>();

    // are we hovering over this table`s collider with a mouse
    private bool cursorOverThisTable = false;
    private GameObject placeHolder;
    [SerializeField] public GameObject placeHolderPrefab;
    [SerializeField] public float shakeDuration = 0.5f;

    // A 3D collider attached to this game object
    private BoxCollider col;
    private int UniqueID = 0;
    private UniqueIdGenerator generator = new UniqueIdGenerator();
    // PROPERTIES

    // returns true if we are hovering over any player`s table collider
    public static bool CursorOverSomeTable
    {
        get
        {
            TableVisual[] bothTables = GameObject.FindObjectsOfType<TableVisual>();
            // Debug.Log("Collide flag: " + CursorOverSomeTable );
            return (bothTables[0].CursorOverThisTable || bothTables[1].CursorOverThisTable);
        }
    }

    // returns true only if we are hovering over this table`s collider
    public bool CursorOverThisTable
    {
        get { return cursorOverThisTable; }
    }

    // METHODS

    // MONOBEHAVIOUR SCRIPTS (mouse over collider detection)
    void Awake()
    {
        col = GetComponent<BoxCollider>();
    }
    private void Start()
    {
        ShiftSlotsGameObjectAccordingToNumberOfCreatures();
        DraggingActions.DragFailed += OnDragFailed;
    }

    // CURSOR/MOUSE DETECTION
    void Update()
    {
        // we need to Raycast because OnMouseEnter, etc reacts to colliders on cards and cards "cover" the table
        // create an array of RaycastHits
        RaycastHit[] hits;
        // raycst to mousePosition and store all the hits in the array
        hits = Physics.RaycastAll(Camera.main.ScreenPointToRay(Input.mousePosition), 30f);

        bool passedThroughTableCollider = false;
        foreach (RaycastHit h in hits)
        {
            // check if the collider that we hit is the collider on this GameObject
            if (h.collider == col)
                passedThroughTableCollider = true;
        }
        cursorOverThisTable = passedThroughTableCollider;

    }
    private void OnDragFailed()
    {
        if (placeHolder != null && CreaturesOnTable.Contains(placeHolder))
        {
            CreaturesOnTable.Remove(placeHolder);
            Destroy(placeHolder);
            placeHolder = null;
            ShiftSlotsGameObjectAccordingToNumberOfCreatures();
            PlaceCreaturesOnNewSlots();
        }
    }
    public void PredictCreatureSlot(int tablePos)
    {
        if (placeHolder != null && CreaturesOnTable.Contains(placeHolder))
        {
            CreaturesOnTable.Remove(placeHolder);
        }
        else
        {
            placeHolder = GameObject.Instantiate(placeHolderPrefab, slots.Children[tablePos].transform.position, Quaternion.identity, slots.transform) as GameObject;
        }
        CreaturesOnTable.Insert(tablePos, placeHolder);
        ShiftSlotsGameObjectAccordingToNumberOfCreatures();
        PlaceCreaturesOnNewSlots();
        Command.CommandExecutionComplete();

    }
    // method to create a new creature and add it to the table
    public void AddCreatureAtIndex(CardAsset ca, int UniqueID, int index)
    {
        DestroyPlaceholder();

        // create a new creature from prefab
        GameObject creature = CreateCreatureGO(index);
        ReadCardAssetForCreature(ca, creature);

        ApplyTagsOnCreature(creature);
        SetParentForCreature(creature);
        AddCreatureToTable(index, creature);
        AddIDToCreature(UniqueID, creature);

        UpdateTablePlacing();
        // Trigger the OnTableChange event
        OnTableChange?.Invoke();

        // end command execution
        Command.CommandExecutionComplete();
    }
    public void SummonCreaturesOnTable(CardAsset ca, List<int> UniqueIDs)
    {
        List<int> indexes = new List<int>();
        int i = 0;
        while (CreaturesOnTable.Count + i < maxCreatures && indexes.Count < UniqueIDs.Count)
        {
            indexes.Add(CreaturesOnTable.Count + i);
            i++;
        }
        DestroyPlaceholder();
        List<GameObject> creatures = new List<GameObject>();
        for (int j = 0; j < indexes.Count; j++)
        {
            int index = indexes[j];
            int id = UniqueIDs[j];
            // create a new creature from prefab
            GameObject creature = CreateCreatureGO(index);
            creatures.Add(creature);
            //fade creature

            ReadCardAssetForCreature(ca, creature);

            ApplyTagsOnCreature(creature);
            SetParentForCreature(creature);
            AddCreatureToTable(index, creature);
            AddIDToCreature(id, creature);
            CanvasGroup canvasGroup = creature.GetComponentInChildren<CanvasGroup>();
            canvasGroup.alpha = 0;
            canvasGroup.DOFade(1, 1f);
        }
        UpdateTablePlacing();
        // Trigger the OnTableChange event
        OnTableChange?.Invoke();
        Command.CommandExecutionComplete();

    }

    private void UpdateTablePlacing()
    {
        // after a new creature is added update placing of all the other creatures
        ShiftSlotsGameObjectAccordingToNumberOfCreatures();
        PlaceCreaturesOnNewSlots();
    }

    private static void AddIDToCreature(int UniqueID, GameObject creature)
    {
        // add our unique ID to this creature
        IDHolder id = creature.AddComponent<IDHolder>();
        id.UniqueID = UniqueID;
    }

    private void AddCreatureToTable(int index, GameObject creature)
    {
        // add a new creature to the list
        CreaturesOnTable.Insert(index, creature);

        // let this creature know about its position
        WhereIsTheCardOrCreature w = creature.GetComponent<WhereIsTheCardOrCreature>();
        w.Slot = index;
        w.VisualState = VisualStates.LowTable;
    }

    private void SetParentForCreature(GameObject creature)
    {
        // parent a new creature gameObject to table slots
        creature.transform.SetParent(slots.transform);
    }

    private void ApplyTagsOnCreature(GameObject creature)
    {
        // add tag according to owner
        foreach (Transform t in creature.GetComponentsInChildren<Transform>())
            t.tag = owner.ToString() + "Creature";
    }

    private static void ReadCardAssetForCreature(CardAsset ca, GameObject creature)
    {
        // apply the look from CardAsset
        OneCreatureManager manager = creature.GetComponent<OneCreatureManager>();
        manager.cardAsset = ca;
        manager.ReadCreatureFromAsset();
    }

    private GameObject CreateCreatureGO(int index)
    {
        return GameObject.Instantiate(GlobalSettings.Instance.CreaturePrefab, slots.Children[index].transform.position, Quaternion.identity) as GameObject;
    }

    private void DestroyPlaceholder()
    {
        if (placeHolder != null && CreaturesOnTable.Contains(placeHolder))
        {
            CreaturesOnTable.Remove(placeHolder);
            Destroy(placeHolder);
            placeHolder = null;
        }
    }


    // returns an index for a new creature based on mousePosition
    // included for placing a new creature to any positon on the table
    public int TablePosForNewCreature(float MouseX)
    {
        // if there are no creatures or if we are pointing to the right of all creatures with a mouse.
        // right - because the table slots are flipped and 0 is on the right side.
        int pos = 0;
        if (CreaturesOnTable.Count == 0 || MouseX > slots.Children[0].transform.position.x)
            return 0;
        else if (MouseX < slots.Children[CreaturesOnTable.Count - 1].transform.position.x) // cursor on the left relative to all creatures on the table
            pos = CreaturesOnTable.Count;
        for (int i = 0; i < CreaturesOnTable.Count; i++)
        {
            if (MouseX < slots.Children[i].transform.position.x && MouseX > slots.Children[i + 1].transform.position.x)
            {
                pos = i + 1;
                break;
            }

        }

        if (placeHolder != null && CreaturesOnTable.Contains(placeHolder))
        {
            return Mathf.Clamp(pos - 1, 0, CreaturesOnTable.Count);
        }
        //Debug.Log("Suspicious behavior. Reached end of TablePosForNewCreature method. Returning 0");
        return pos;
    }

    public static void MassRemoveCreatures(IEnumerable<IGrouping<Player, CreatureLogic>> creatureGroupsByOwner)
    {
        int totalAnimations = 0;
        int completedAnimations = 0;
        List<Player> players = new List<Player>();
        foreach (var creatureGroup in creatureGroupsByOwner)
        {
            // creatureGroup.Key is the owner (Player)
            Player owner = creatureGroup.Key;
            players.Add(owner);
            // creatureGroup is the list of creatures belonging to this owner
            List<CreatureLogic> creatures = creatureGroup.ToList();
            // Count the total number of animations
            totalAnimations += creatures.Count;

            // Now you can handle each group of creatures separately
            // For example, call a method that handles creatures' death for a specific owner
            owner.PArea.tableVisual.RemoveCreaturesByIDs(creatureGroup.ToList().Select(c => c.ID).ToList(), () =>
            {
                // Increment the number of completed animations
                completedAnimations++;

                // If all animations have completed, call CommandExecutionComplete
                if (completedAnimations >= totalAnimations)
                {
                    players.ForEach(p =>
                    {
                        p.PArea.tableVisual.ShiftSlotsGameObjectAccordingToNumberOfCreatures();
                        p.PArea.tableVisual.PlaceCreaturesOnNewSlots();
                        p.PArea.tableVisual.OnTableChange?.Invoke();
                    });

                    Command.CommandExecutionComplete();
                }
            });
        }
    }
    // Destroy a creature
    public void RemoveCreatureWithID(int IDToRemove)
    {
        // TODO: Here we visually remove a creature from the board. Use DOTween to slightly shake the position, then perform the rest of the code after the interval (interval should be public and editable from unity)
        GameObject creatureToRemove = IDHolder.GetGameObjectWithID(IDToRemove);
        creatureToRemove.transform.DOShakePosition(shakeDuration)
       .OnComplete(() =>
       {
           // After the shaking is done, remove and destroy the creature
           CreaturesOnTable.Remove(creatureToRemove);
           Destroy(creatureToRemove);

           ShiftSlotsGameObjectAccordingToNumberOfCreatures();
           PlaceCreaturesOnNewSlots();
           // Trigger the OnTableChange event
        OnTableChange?.Invoke();
       });
    }
    public void RemoveCreaturesByIDs(List<int> IDs, Action onCompletion)
    {
        List<GameObject> creaturesToRemove = IDHolder.GetGameObjects(IDs);
        creaturesToRemove.ForEach(c =>
        {
            c.transform.DOShakePosition(shakeDuration, 1, 50)
                .OnComplete(() =>
                {
                    // After the shaking is done, remove and destroy the creature
                    CreaturesOnTable.Remove(c);
                    Destroy(c);
                    // Call the completion action
                    onCompletion();
                });
        });
    }

    /// <summary>
    /// Shifts the slots game object according to number of creatures.
    /// </summary>
    void ShiftSlotsGameObjectAccordingToNumberOfCreatures()
    {
        float posX;
        if (CreaturesOnTable.Count > 0)
            posX = (slots.Children[0].transform.localPosition.x - slots.Children[CreaturesOnTable.Count - 1].transform.localPosition.x) / 2f;
        else
            posX = 0f;

        slots.gameObject.transform.DOLocalMoveX(posX, 0.3f);
    }

    /// <summary>
    /// After a new creature is added or an old creature dies, this method
    /// shifts all the creatures and places the creatures on new slots.
    /// </summary>
    void PlaceCreaturesOnNewSlots()
    {
        foreach (GameObject g in CreaturesOnTable)
        {
            g.transform.DOLocalMoveX(slots.Children[CreaturesOnTable.IndexOf(g)].transform.localPosition.x, 0.3f);
            // apply correct sorting order and HandSlot value for later 
            // TODO: figure out if I need to do something here:
            // g.GetComponent<WhereIsTheCardOrCreature>().SetTableSortingOrder() = CreaturesOnTable.IndexOf(g);
        }
    }

}
