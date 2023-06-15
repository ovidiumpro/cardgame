using UnityEngine;
using UnityEditor;
using CG.Cards;
static class CardUnityIntegration 
{

	[MenuItem("Assets/Create/CardAsset")]
	public static void CreateYourScriptableObject() {
		ScriptableObjectUtility2.CreateAsset<CardAsset>();
	}

}
