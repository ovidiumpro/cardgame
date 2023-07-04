using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CardType
{
    CreatureCard, 
    SpellCard, 
    FlashCreatureCard, 
    FastSpellCard, 
    BattlecryEffect, 
    DeathrattleEffect, 
    TriggeredEffect, 
    ActivatedEffect,
    None
}

public class PlayStackElement
{
    public GameObject Card { get; set; }
    public Action Callback { get; set; }
    public CardType Type { get; set; }
    public Player Player {get; set; }

    public PlayStackElement(GameObject card, Action callback, CardType type, Player p)
    {
        this.Card = card;
        this.Callback = callback;
        this.Type = type;
        this.Player = p;
    }
}
