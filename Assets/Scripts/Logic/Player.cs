using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CG.Cards;
using DG.Tweening;
using System;

public class Player : MonoBehaviour, ICharacter
{
    public int PlayerID;
    public CharacterAsset charAsset;
    public PlayerArea PArea;
    public SpellEffect HeroPowerEffect;

    public Deck deck;
    public Hand hand;
    public Table table;

    private int bonusManaThisTurn = 0;
    public bool usedHeroPowerThisTurn = false;
    private int testTracker = 0;

    public int ID
    {
        get { return PlayerID; }
    }

    private int manaThisTurn;
    public int ManaThisTurn
    {
        get { return manaThisTurn; }
        set
        {
            manaThisTurn = value;
            //PArea.ManaBar.TotalCrystals = manaThisTurn;
            new UpdateManaCrystalsCommand(this, manaThisTurn, manaLeft).AddToQueue();
        }
    }

    private int manaLeft;
    public int ManaLeft
    {
        get
        { return manaLeft; }
        set
        {
            manaLeft = value;
            //PArea.ManaBar.AvailableCrystals = manaLeft;
            new UpdateManaCrystalsCommand(this, ManaThisTurn, manaLeft).AddToQueue();
            //Debug.Log(ManaLeft);
            // if (TurnManager.Instance.whoseTurn == this)
            //     HighlightPlayableObjects();
        }
    }

    public Player otherPlayer
    {
        get
        {
            if (Players[0] == this)
                return Players[1];
            else
                return Players[0];
        }
    }

    private int health;
    public int Health
    {
        get { return health; }
        set
        {
            health = value;
            if (value <= 0)
                Die();
        }
    }

    public delegate void VoidWithNoArguments();
    //public event VoidWithNoArguments CreaturePlayedEvent;
    //public event VoidWithNoArguments SpellPlayedEvent;
    //public event VoidWithNoArguments StartTurnEvent;
    public event VoidWithNoArguments EndTurnEvent;

    public static Player[] Players;

    void Awake()
    {
        Players = GameObject.FindObjectsOfType<Player>();
        PlayerID = IDFactory.GetUniqueID();
    }

    public void RegisterEvents()
    {
        this.PArea.tableVisual.OnTableChange += HighlightCreatures;
        this.PArea.handVisual.OnHandChange += HighlightCards;
        this.PArea.handVisual.OnHandChange += HighlightHeroPower;
    }

    public virtual void OnTurnStart()
    {
        // add one mana crystal to the pool;
        Debug.Log("In ONTURNSTART for " + gameObject.name);
        usedHeroPowerThisTurn = false;
        ManaThisTurn++;
        ManaLeft = ManaThisTurn;
        foreach (CreatureLogic cl in table.CreaturesOnTable)
            cl.OnTurnStart();
        PArea.HeroPower.WasUsedThisTurn = false;
        HighlightPlayableObjects();

    }

    public void GetBonusMana(int amount)
    {
        bonusManaThisTurn += amount;
        ManaThisTurn += amount;
        ManaLeft += amount;
    }

    public void OnTurnEnd()
    {
        if (EndTurnEvent != null)
            EndTurnEvent.Invoke();
        ManaThisTurn -= bonusManaThisTurn;
        bonusManaThisTurn = 0;
        GetComponent<TurnMaker>().StopAllCoroutines();
        RemoveHighlights();
    }


    public void DrawACard(bool fast = false)
    {
        if (deck.cards.Count > 0)
        {
            if (hand.CardsInHand.Count < PArea.handVisual.slots.Children.Length)
            {
                // 1) save index to place a visual card into visual hand
                int indexToPlaceACard = hand.CardsInHand.Count;
                // 2) logic: add card to hand
                CardLogic newCard = new CardLogic(deck.cards[0]);
                newCard.owner = this;
                hand.CardsInHand.Add(newCard);
                // Debug.Log(hand.CardsInHand.Count);
                // 3) logic: remove the card from the deck
                deck.cards.RemoveAt(0);
                // 4) create a command
                new DrawACardCommand(hand.CardsInHand[indexToPlaceACard], this, indexToPlaceACard, fast, fromDeck: true).AddToQueue();
            }
        }
        else
        {
            Debug.Log("No cards left: " + deck.cards.Count);
            // there are no cards in the deck, take fatigue damage.
        }

    }
    public void DrawCards(int nr) {
        bool fast = nr > 3;
        for (int i=0;i<nr; i++) {
            DrawACard();
        }
        

    }
    public void SummonCreatures(CardAsset creature, int nr) {
        List<CreatureLogic> summons = new List<CreatureLogic>();
        for (int i = 0; i<nr; i++ ) {
            CreatureLogic cl = new CreatureLogic(this, creature);
            table.AddCreature(cl);
            summons.Add(cl);
        }

        //Invoke a command to inform visual layer about changes. (have to create new)
        new SummonCreaturesCommand(this, summons).AddToQueue();
    }

    public void DrawACoin()
    {
        if (hand.CardsInHand.Count < PArea.handVisual.slots.Children.Length)
        {
            // 1) logic: add card to hand
            CardLogic newCard = new CardLogic(GlobalSettings.Instance.CoinCard);
            newCard.owner = this;
            hand.CardsInHand.Add(newCard);
            // 2) send message to the visual Deck
            new DrawACardCommand(hand.CardsInHand[hand.CardsInHand.Count - 1], this, hand.CardsInHand.Count - 1, fast: true, fromDeck: false).AddToQueue();
        }
        // no removal from deck because the coin was not in the deck
    }

    public void PlayASpellFromHand(int SpellCardUniqueID, int TargetUniqueID)
    {
       // ManaLeft -= playedCard.CurrentManaCost;
        GameObject card = IDHolder.GetGameObjectWithID(SpellCardUniqueID);
        // Assuming `gameObject` is your GameObject
        BetterCardRotation betterCardRotation = card.GetComponent<BetterCardRotation>();

        // Check if the component exists
        if (betterCardRotation != null)
        {
            // Disable the script
            betterCardRotation.enabled = false;
        }
        Sequence seq = CreateMoveToPlayPositionSequence(card, 0.25f, 1f, () =>
        {
            // TODO: !!!
            // if TargetUnique ID < 0 , for example = -1, there is no target.
            if (TargetUniqueID < 0)
                PlayASpellFromHand(CardLogic.CardsCreatedThisGame[SpellCardUniqueID], null);
            else if (TargetUniqueID == ID)
            {
                PlayASpellFromHand(CardLogic.CardsCreatedThisGame[SpellCardUniqueID], this);
            }
            else if (TargetUniqueID == otherPlayer.ID)
            {
                PlayASpellFromHand(CardLogic.CardsCreatedThisGame[SpellCardUniqueID], this.otherPlayer);
            }
            else
            {
                // target is a creature
                PlayASpellFromHand(CardLogic.CardsCreatedThisGame[SpellCardUniqueID], CreatureLogic.CreaturesCreatedThisGame[TargetUniqueID]);
            }
        });
        seq.Play();
    }

    public void PlayASpellFromHand(CardLogic playedCard, ICharacter target)
    {
        ManaLeft -= playedCard.CurrentManaCost;
        //TODO: Idenitify how we can get the targets for all atomic effects, this method will be called with a specific target, but we need to be able to have all targets here so we can call composite effect will all targets.
        // cause effect instantly:
        if (playedCard.effect != null)
        {
            Queue<IIdentifiable> targets = new Queue<IIdentifiable>();
            targets.Enqueue(target);
            playedCard.effect.ActivateEffects(targets);
        }

        else
        {
            Debug.LogWarning("No effect found on card " + playedCard.ca.name);
        }
        // no matter what happens, move this card to PlayACardSpot
        new PlayASpellCardCommand(this, playedCard).AddToQueue();
        // remove this card from hand
        hand.CardsInHand.Remove(playedCard);
        // check if this is a creature or a spell
    }

    public void PlayACreatureFromHand(int UniqueID, int tablePos)
    {
        PlayACreatureFromHand(CardLogic.CardsCreatedThisGame[UniqueID], tablePos);
    }

    public void PlayACreatureFromHand(CardLogic playedCard, int tablePos)
    {
        //Debug.Log(ManaLeft);
        //Debug.Log(playedCard.CurrentManaCost);
        ManaLeft -= playedCard.CurrentManaCost;
        GameObject card = IDHolder.GetGameObjectWithID(playedCard.UniqueCardID);
        // Assuming `gameObject` is your GameObject
        BetterCardRotation betterCardRotation = card.GetComponent<BetterCardRotation>();

        // Check if the component exists
        if (betterCardRotation != null)
        {
            // Disable the script
            betterCardRotation.enabled = false;
        }
        Sequence seq = CreateMoveToPlayPositionSequence(card, 0.25f, 1f, () =>
        {
            // Debug.Log("Mana Left after played a creature: " + ManaLeft);
            // create a new creature object and add it to Table
            CreatureLogic newCreature = new CreatureLogic(this, playedCard.ca);
            table.CreaturesOnTable.Insert(tablePos, newCreature);
            // no matter what happens, move this card to PlayACardSpot
            new PlayACreatureCommand(playedCard, this, tablePos, newCreature.UniqueCreatureID).AddToQueue();
            // remove this card from hand
            hand.CardsInHand.Remove(playedCard);
            //TODO: Creature effect trigger here
            //HighlightPlayableObjects();
        });
        seq.Play();
    }
    private Sequence CreateMoveToPlayPositionSequence(GameObject card, float transitionTime, float waitTime, Action onCompleteCallback)
    {
        Sequence seq = DOTween.Sequence();
        seq.Append(card.transform.DOMove(PArea.PlayCardStackPosition.position, transitionTime));
        seq.Insert(0f, card.transform.DORotate(Vector3.zero, transitionTime));
        seq.AppendInterval(waitTime);
        seq.OnComplete(() => onCompleteCallback());
        return seq;
    }
    public void PredictNewSlotForCreatureOnTable(int tablePos)
    {

    }

    public void Die()
    {
        // game over
        // block both players from taking new moves 
        PArea.ControlsON = false;
        otherPlayer.PArea.ControlsON = false;
        TurnManager.Instance.StopTheTimer();
        new GameOverCommand(this).AddToQueue();
    }

    private void RemoveHighlights()
    {
        PArea.HeroPower.Highlighted = false;
         foreach (CreatureLogic crl in table.CreaturesOnTable)
        {
            GameObject g = IDHolder.GetGameObjectWithID(crl.UniqueCreatureID);
            if (g != null)
                g.GetComponent<OneCreatureManager>().CanAttackNow = false;
        }
        foreach (CardLogic cl in hand.CardsInHand)
        {
            GameObject g = IDHolder.GetGameObjectWithID(cl.UniqueCardID);
            if (g != null)
                g.GetComponent<OneCardManager>().CanBePlayedNow = false;
        }

    }
    // METHODS TO SHOW GLOW HIGHLIGHTS
    public void HighlightPlayableObjects(bool removeAllHighlights = false)
    {
        HighlightCards();
        HighlightCreatures();
        HighlightHeroPower();
    }

    private void HighlightHeroPower()
    {
        if (TurnManager.Instance.whoseTurn != this) return;
        // highlight hero power
        PArea.HeroPower.Highlighted = (!usedHeroPowerThisTurn) && (ManaLeft > 1);
    }

    private void HighlightCreatures()
    {
        if (TurnManager.Instance.whoseTurn != this) return;
        foreach (CreatureLogic crl in table.CreaturesOnTable)
        {
            GameObject g = IDHolder.GetGameObjectWithID(crl.UniqueCreatureID);
            if (g != null)
                g.GetComponent<OneCreatureManager>().CanAttackNow = crl.CanAttack;
        }
    }

    private void HighlightCards()
    {
        if (TurnManager.Instance.whoseTurn != this) return;
        //TODO: Fix bug, after spell played that summons, some summoned creatures show up highlighted.
        //Debug.Log("HighlightPlayable remove: "+ removeAllHighlights);
        foreach (CardLogic cl in hand.CardsInHand)
        {
            GameObject g = IDHolder.GetGameObjectWithID(cl.UniqueCardID);
            if (g != null)
                g.GetComponent<OneCardManager>().CanBePlayedNow = (cl.CurrentManaCost <= ManaLeft);
        }
    }

    // START GAME METHODS
    public void LoadCharacterInfoFromAsset()
    {
        Health = charAsset.MaxHealth;
        // change the visuals for portrait, hero power, etc...
        PArea.Portrait.charAsset = charAsset;
        PArea.Portrait.ApplyLookFromAsset();
        // TODO: insert the code to attach hero power script here. 
        if (charAsset.HeroPowerName != null && charAsset.HeroPowerName != "")
        {
            // HeroPowerEffect = System.Activator.CreateInstance(System.Type.GetType(charAsset.HeroPowerName)) as SpellEffect;
        }
        else
        {
            Debug.LogWarning("Check hero powr name for character " + charAsset.ClassName);
        }
    }

    public void TransmitInfoAboutPlayerToVisual()
    {
        PArea.Portrait.gameObject.AddComponent<IDHolder>().UniqueID = PlayerID;
        if (GetComponent<TurnMaker>() is AITurnMaker)
        {
            // turn off turn making for this character
            PArea.AllowedToControlThisPlayer = false;
        }
        else
        {
            // allow turn making for this character
            PArea.AllowedToControlThisPlayer = true;
        }
    }

    public void UseHeroPower()
    {
        ManaLeft -= 2;
        usedHeroPowerThisTurn = true;
        HeroPowerEffect.ActivateEffect();
    }
}
