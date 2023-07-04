using UnityEngine;
using System.Collections;
using System.Collections.Generic;
namespace CG.Cards
{
     public enum ECardType
        {
            Creature,
            FlashCreature,
            Spell,
            FastSpell,
            Equipment,
            Artifact,
            Hero
        }
        public enum TargetingOptions
        {
            NoTarget,
            AllCreatures,
            EnemyCreatures,
            YourCreatures,
            AllCharacters,
            EnemyCharacters,
            YourCharacters
        }

    [CreateAssetMenu(fileName = "New Card", menuName = "Card Game/Card")]
    public class CardAsset : ScriptableObject
    {

        // this object will hold the info about the most general card
        [Header("General info")]
        public CharacterAsset characterAsset;  // if this is null, it`s a neutral card
        [TextArea(2, 3)]
        public string Description;  // Description for spell or character
        public Sprite CardImage;
        
        public RectTransform CardImageRect;
        public int ManaCost;

        public ECardType CardType;


        [Header("Creature Info")]
        public int MaxHealth;
        public int Attack;
        public int AttacksForOneTurn = 1;
        public bool Taunt;
        public bool Charge;
        public string CreatureScriptName;
        public int specialCreatureAmount;
        public Sprite CreatureImage;

        public CompositeEffect OnCreatureEnterEffect;

        [Header("SpellInfo")]
        public CompositeEffect SpellEffect;
        public bool Fast
        {
            get
            {
                return CardType == ECardType.FastSpell;
            }
        }
        public TargetingOptions Targets;

    }
}

