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


    void OnDisable()
    {
        TurnManager.Instance.OnTurnChange -= HandleTurnChange;
        this.PArea.tableVisual.OnTableChange -= HighlightCreatures;
        this.PArea.handVisual.OnHandChange -= HighlightCards;
        this.PArea.handVisual.OnHandChange -= HighlightHeroPower;
    }

    public void RegisterEvents()
    {
        TurnManager.Instance.OnTurnChange += HandleTurnChange;
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
        bonusManaThisTurn = 0;
        GetComponent<TurnMaker>().StopAllCoroutines();
    }
    public void OnTemporaryTurnEnd()
    {

    }


    void HandleTurnChange(Player player, bool isTurnStart)
    {
        // Highlight or unhighlight playable based on the value of the parameter
        if (player == this)
        {
            Debug.Log("During stack: " + !isTurnStart + "; for player: " + player.gameObject.name);
            if (isTurnStart)
            {
                HighlightPlayableObjects();
            }
            else
            {
                HighlightCardsFast();
                this.PArea.PlayStackControls.SetActive(true);
            }

        }
        else
        {
            RemoveHighlights();
            this.PArea.PlayStackControls.SetActive(false);
        }
    }



    public void DrawACard(bool fast = false, bool addToFront = false, int nr = 1)
    {
        //we are getting nr of cards drawn here
        //then call gamestate to get which cards will be drawn
        //then we will create a draw card command for each card logic, adding them to queue 1 by 1, when reaching the last one, we call AddToQueue
        //
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
                new DrawACardCommand(hand.CardsInHand[indexToPlaceACard], this, indexToPlaceACard, fast, fromDeck: true).AddToQueue(addToFront);
            }
        }
        else
        {
            Debug.Log("No cards left: " + deck.cards.Count);
            // there are no cards in the deck, take fatigue damage.
        }

    }
    public void DrawCards(int nr, bool addToFront = false)
    {
        bool fast = nr > 3;
        for (int i = 0; i < nr; i++)
        {
            DrawACard(fast, addToFront);
        }


    }
    public void SummonCreatures(CardAsset creature, int nr)
    {
        List<CreatureLogic> summons = new List<CreatureLogic>();
        for (int i = 0; i < nr; i++)
        {
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

    public void PlayASpellFromHand(int SpellCardUniqueID, List<int> ids)
    {
        // ManaLeft -= playedCard.CurrentManaCost;
        GameObject card = IDHolder.GetGameObjectWithID(SpellCardUniqueID);
        // Assuming `gameObject` is your GameObject
        BetterCardRotation betterCardRotation = card.GetComponent<BetterCardRotation>();
        List<int> idBackups = null;
        if (ids != null)
            idBackups = new List<int>(ids);
        // Check if the component exists
        if (betterCardRotation != null)
        {
            // Disable the script
            betterCardRotation.enabled = false;
        }
        PlayASpellFromHand(CardLogic.CardsCreatedThisGame[SpellCardUniqueID], GetCharactersFromIds(idBackups));
    }

    private List<IIdentifiable> GetCharactersFromIds(List<int> ids)
    {
        if (ids == null || ids.Count == 0) return null;
        List<IIdentifiable> chars = new List<IIdentifiable>();
        foreach (int id in ids)
        {
            if (id == ID)
            {
                chars.Add(this);
                continue;
            }
            if (id == otherPlayer.ID)
            {
                chars.Add(this.otherPlayer);
                continue;
            }
            chars.Add(CreatureLogic.CreaturesCreatedThisGame[id]);
        }
        return chars;
    }

    public void PlayASpellFromHand(CardLogic playedCard, List<IIdentifiable> targets)
    {
        ManaLeft -= playedCard.CurrentManaCost;
        //Visually and logically remove the card from hand before activating the effects
        // remove this card from hand
        hand.CardsInHand.Remove(playedCard);
        // no matter what happens, move this card to PlayACardSpot
        new PlayASpellCardCommand(this, playedCard).AddToQueue();
        GameObject card = IDHolder.GetGameObjectWithID(playedCard.UniqueCardID);
        new CallbackCommand(() =>
        {
            Sequence seq = CreateMoveToPlayPositionSequence(card, 0.25f, 1f, GetCardType(playedCard.ca.CardType),() =>
                   {
                       if (playedCard.effect != null)
                       {
                           Queue<IIdentifiable> targetsQueue = new Queue<IIdentifiable>();
                           if (targets != null)
                           {
                               targets.ForEach(t => targetsQueue.Enqueue(t));
                           }
                           Command.PauseQueueExecution();
                           playedCard.effect.ActivateEffects(targetsQueue);
                           Command.ResumeQueueExecution();
                       }

                       else
                       {
                           Debug.LogWarning("No effect found on card " + playedCard.ca.name);
                       }

                       new RemovePlayedSpellCardFromTheScreenCommand(this, playedCard).AddToQueue();
                   });
            seq.Play();
        }).AddToQueue();


        // check if this is a creature or a spell
    }

    public void PlayACreatureFromHand(int UniqueID, int tablePos)
    {
        PlayACreatureFromHand(CardLogic.CardsCreatedThisGame[UniqueID], tablePos);
    }
    private CardType GetCardType(ECardType cardType) {
        switch (cardType) {
            case ECardType.Creature: return CardType.CreatureCard;
            case ECardType.FlashCreature: return CardType.FlashCreatureCard;
            case ECardType.Spell: return CardType.SpellCard;
            case ECardType.FastSpell: return CardType.FastSpellCard;
            default: return CardType.None;
        }
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
        // remove this card from hand
        hand.CardsInHand.Remove(playedCard);
        //remove card visually
        new PlayASpellCardCommand(this, playedCard).AddToQueue();
        //destroy placeholder
        this.PArea.tableVisual.DestroyPlaceholder();
        new CallbackCommand(() =>
        {
            Sequence seq = CreateMoveToPlayPositionSequence(card, 0.25f, 1f, GetCardType(playedCard.ca.CardType),() =>
                    {
                        // Debug.Log("Mana Left after played a creature: " + ManaLeft);
                        // create a new creature object and add it to Table
                        CreatureLogic newCreature = new CreatureLogic(this, playedCard.ca);
                        table.CreaturesOnTable.Insert(tablePos, newCreature);
                        // no matter what happens, move this card to PlayACardSpot


                        new PlayACreatureCommand(playedCard, this, tablePos, newCreature.UniqueCreatureID).AddToQueue(true);

                        //TODO: Creature effect trigger here
                        //HighlightPlayableObjects();
                    });
            seq.Play();
        }).AddToQueue();

    }
    private Sequence CreateMoveToPlayPositionSequence(GameObject card, float transitionTime, float waitTime, CardType cardType,Action onCompleteCallback)
    {
        //card.transform.DOMove(PArea.PlayCardStackPosition.position, transitionTime)
        Sequence seq = DOTween.Sequence();
        seq.InsertCallback(0f, () => PlayStackPositionManager.Instance.AddCard(new PlayStackElement(card, onCompleteCallback, cardType, this )));
        seq.Insert(0f, card.transform.DORotate(Vector3.zero, transitionTime));
        // seq.AppendInterval(waitTime);
        // seq.OnComplete(() => onCompleteCallback());
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

    public void RemoveHighlights()
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
    public bool PlayerCanPlayCardsOnOpponentTurn()
    {
        bool hasCardsToPlay = false;
        foreach (CardLogic cl in hand.CardsInHand)
        {
            GameObject g = IDHolder.GetGameObjectWithID(cl.UniqueCardID);
            if (g != null)
            {
                bool canBePlayed = (cl.CurrentManaCost <= ManaLeft) && cl.IsFast();
                hasCardsToPlay = hasCardsToPlay || canBePlayed;
                g.GetComponent<OneCardManager>().CanBePlayedNow = canBePlayed;
            }

        }
        return hasCardsToPlay;
    }
    public void HighlightCardsFast()
    {
        foreach (CardLogic cl in hand.CardsInHand)
        {
            GameObject g = IDHolder.GetGameObjectWithID(cl.UniqueCardID);
            if (g != null)
            {
                bool canBePlayed = (cl.CurrentManaCost <= ManaLeft) && cl.IsFast();
                g.GetComponent<OneCardManager>().CanBePlayedNow = canBePlayed;
            }

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
