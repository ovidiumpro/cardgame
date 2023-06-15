using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CG.Cards;
[System.Serializable]
public class CreatureLogic : ICharacter
{
    //TODO: This needs to hold ref to all creatures that are about to die and it needs to trigger die for all  with one command.
    //TODO: This new command has to be triggered at a certain point, maybe after the Command.Queue is empty?
    // PUBLIC FIELDS
    public Player owner;
    public CardAsset ca;
    public CreatureEffect effect;
    private static List<CreatureLogic> CreaturesToDie = new List<CreatureLogic>();
    public int UniqueCreatureID;
    public int ID
    {
        get { return UniqueCreatureID; }
    }
    public bool Frozen = false;

    // the basic health that we have in CardAsset
    private int baseHealth;
    // health with all the current buffs taken into account
    public int MaxHealth
    {
        get { return baseHealth; }
    }

    private int health;

    public int Health
    {
        get { return health; }

        set
        {
            health = Mathf.Min(value, MaxHealth);
            if (value <= 0)
            {
                CreaturesToDie.Add(this);
                isDead = true;
            }


        }
    }
    private bool IsDead = false;
    public bool isDead
    {
        get { return IsDead; }
        set
        {
            IsDead = value;
        }
    }

    public bool CanAttack
    {
        get
        {
            bool ownersTurn = (TurnManager.Instance.whoseTurn == owner);
            return (ownersTurn && (AttacksLeftThisTurn > 0) && !Frozen);
        }
    }

    private int baseAttack;
    // attack with buffs
    public int Attack
    {
        get { return baseAttack; }

    }

    private int attacksForOneTurn = 1;
    public int AttacksLeftThisTurn
    {
        get;
        set;
    }

    // CONSTRUCTOR
    public CreatureLogic(Player owner, CardAsset ca)
    {
        this.ca = ca;
        baseHealth = ca.MaxHealth;
        Health = ca.MaxHealth;
        baseAttack = ca.Attack;
        attacksForOneTurn = ca.AttacksForOneTurn;
        // AttacksLeftThisTurn is now equal to 0
        if (ca.Charge)
            AttacksLeftThisTurn = attacksForOneTurn;
        this.owner = owner;
        UniqueCreatureID = IDFactory.GetUniqueID();
        if (ca.CreatureScriptName != null && ca.CreatureScriptName != "")
        {
            effect = CreatureEffectFactory.CreateCreatureEffect(ca.CreatureScriptName, owner, this, ca.specialCreatureAmount);
            effect.RegisterEffect();
        }
        CreaturesCreatedThisGame.Add(UniqueCreatureID, this);
    }
    static CreatureLogic()
    {
        // Subscribe to the OnQueueEmpty event with a static method
        Command.OnQueueEmpty += HandleQueueEmpty;
    }


    public void OnTurnStart()
    {
        AttacksLeftThisTurn = attacksForOneTurn;
    }

    static void HandleQueueEmpty()
    {
        //Debug.Log("Calling Command Empty event handler");
        // Handle the event. Create a CreaturesDieCommand when the event is raised.
        if (CreatureLogic.CreaturesToDie.Count > 0)
        {
            Debug.Log("Creatures need to die");
            List<CreatureLogic> creaturesToDieCopy = new List<CreatureLogic>(CreatureLogic.CreaturesToDie);
            new AllCreaturesDieCommand(creaturesToDieCopy).AddToQueue();
            CreatureLogic.CreaturesToDie.Clear();
        }
    }
    public void Die()
    {
        owner.table.CreaturesOnTable.Remove(this);

        new CreatureDieCommand(UniqueCreatureID, owner).AddToQueue();
    }

    public void GoFace()
    {
        AttacksLeftThisTurn--;
        int targetHealthAfter = owner.otherPlayer.Health - Attack;
        new CreatureAttackCommand(owner.otherPlayer.PlayerID, UniqueCreatureID, 0, Attack, Health, targetHealthAfter).AddToQueue();
        owner.otherPlayer.Health -= Attack;
    }

    public void AttackCreature(CreatureLogic target)
    {
            AttacksLeftThisTurn--;
        // calculate the values so that the creature does not fire the DIE command before the Attack command is sent
        int targetHealthAfter = target.Health - Attack;
        int attackerHealthAfter = Health - target.Attack;
        new CreatureAttackCommand(target.UniqueCreatureID, UniqueCreatureID, target.Attack, Attack, attackerHealthAfter, targetHealthAfter).AddToQueue();

        target.Health -= Attack;
        Health -= target.Attack;
    }

    public void AttackCreatureWithID(int uniqueCreatureID)
    {
        CreatureLogic target = CreatureLogic.CreaturesCreatedThisGame[uniqueCreatureID];
        AttackCreature(target);
    }

    // STATIC For managing IDs
    public static Dictionary<int, CreatureLogic> CreaturesCreatedThisGame = new Dictionary<int, CreatureLogic>();

}
