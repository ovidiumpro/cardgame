using UnityEngine;
using UnityEditor;
using CG.Cards;
namespace CG.Cards
{
    [CustomEditor(typeof(CardAsset))]
    public class CardEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            CardAsset card = (CardAsset)target;

            // Draw default properties
            EditorGUILayout.PropertyField(serializedObject.FindProperty("characterAsset"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("Description"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("CardImage"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("ManaCost"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("CardType"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("CardImageRect"));


            // Draw properties specific to the Creature card type
            if (card.CardType == ECardType.Creature)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("MaxHealth"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("Attack"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("AttacksForOneTurn"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("Taunt"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("Charge"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("CreatureScriptName"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("specialCreatureAmount"));

            }
            if (card.CardType == ECardType.Spell || card.CardType == ECardType.FastSpell)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("SpellScriptName"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("specialSpellAmount"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("Targets"));
            }

            // Apply any changes made in the Inspector
            serializedObject.ApplyModifiedProperties();
        }
    }
}
