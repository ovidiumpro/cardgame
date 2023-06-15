﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using DG.Tweening;
using System;

// this class will take care of switching turns and counting down time until the turn expires
public class TurnManager : MonoBehaviour
{

    private RopeTimer timer;

    // for Singleton Pattern
    public static TurnManager Instance;

    private Player _whoseTurn;
    public Player whoseTurn
    {
        get
        {
            return _whoseTurn;
        }

        set
        {
            _whoseTurn = value;
            timer.StartTimer();

            GlobalSettings.Instance.EnableEndTurnButtonOnStart(_whoseTurn);

            TurnMaker tm = whoseTurn.GetComponent<TurnMaker>();
            // player`s method OnTurnStart() will be called in tm.OnTurnStart();
            tm.OnTurnStart();
            // if (tm is PlayerTurnMaker)
            // {
            //     whoseTurn.HighlightPlayableObjects();
            // }
            // remove highlights for opponent.
            // whoseTurn.otherPlayer.HighlightPlayableObjects(true);

        }
    }

    void Awake()
    {
        Instance = this;
        timer = GetComponent<RopeTimer>();
    }

    void Start()
    {
        OnGameStart();
    }

    public void OnGameStart()
    {
        Debug.Log("In TurnManager.OnGameStart()");

        CardLogic.CardsCreatedThisGame.Clear();
        CreatureLogic.CreaturesCreatedThisGame.Clear();

        foreach (Player p in Player.Players)
        {
            p.ManaThisTurn = 0;
            p.ManaLeft = 0;
            p.LoadCharacterInfoFromAsset();
            p.TransmitInfoAboutPlayerToVisual();
            p.PArea.PDeck.CardsInDeck = p.deck.cards.Count;
            // move both portraits to the center
            p.PArea.Portrait.transform.position = p.PArea.handVisual.OtherCardDrawSourceTransform.position;
        }

        Sequence s = DOTween.Sequence();
        s.Append(Player.Players[0].PArea.Portrait.transform.DOMove(Player.Players[0].PArea.PortraitPosition.position, 1f).SetEase(Ease.InQuad));
        s.Insert(0f, Player.Players[1].PArea.Portrait.transform.DOMove(Player.Players[1].PArea.PortraitPosition.position, 1f).SetEase(Ease.InQuad));
        s.PrependInterval(1f);
        s.OnComplete(() =>
            {
                // determine who starts the game.
                int rnd = UnityEngine.Random.Range(0,2);  // 2 is exclusive boundary
                // Debug.Log(Player.Players.Length);
                Player whoGoesFirst = Player.Players[rnd];
                // Debug.Log(whoGoesFirst);
                Player whoGoesSecond = whoGoesFirst.otherPlayer;
                // Debug.Log(whoGoesSecond);

                // draw 4 cards for first player and 5 for second player
                int initDraw = 4;
                for (int i = 0; i < initDraw; i++)
                {
                    // second player draws a card
                    whoGoesSecond.DrawACard(true);
                    // first player draws a card
                    whoGoesFirst.DrawACard(true);
                }
                // add one more card to second player`s hand
                whoGoesSecond.DrawACard();
                //new GivePlayerACoinCommand(null, whoGoesSecond).AddToQueue();
                whoGoesSecond.DrawACoin();
                //TODO: Execute these 3 lines of code after a 0.3sec delay
                StartCoroutine(ExecuteAfterTime(0.3f, () =>
                {
                    whoGoesFirst.RegisterEvents();
                    whoGoesSecond.RegisterEvents();
                    new StartATurnCommand(whoGoesFirst).AddToQueue();
                }));


            });
    }
    private IEnumerator ExecuteAfterTime(float time, Action task)
    {
        yield return new WaitForSeconds(time);
        task();
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            EndTurn();
    }

    public void EndTurn()
    {
        // stop timer
        timer.StopTimer();
        // send all commands in the end of current player`s turn
        whoseTurn.OnTurnEnd();

        new StartATurnCommand(whoseTurn.otherPlayer).AddToQueue();
    }

    public void EndTurnTest()
    {
        timer.StopTimer();
        timer.StartTimer();
    }
    public void StopTheTimer()
    {
        timer.StopTimer();
    }

}

