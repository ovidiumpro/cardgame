using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FindByType : MonoBehaviour
{
   void Start()
    {
        // This will find all GameObjects in the scene that are using the YourTargetScript as a component.
        DamageEffect[] objects = FindObjectsOfType<DamageEffect>();
        
        foreach (DamageEffect obj in objects)
        {
            Debug.Log(obj.gameObject.name);
        }
    }
}
