using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class IDHolder : MonoBehaviour {

    private int uniqueId;
    public int UniqueID {
        get {
            return uniqueId;
        }
        set {
            uniqueId = value;
            allIDHolders.Add(uniqueId, this);   

        }
    }
    private static Dictionary<int, IDHolder> allIDHolders = new Dictionary<int, IDHolder>();

    void Awake()
    {
       
    }

    public static GameObject GetGameObjectWithID(int ID)
    {
        IDHolder thing = allIDHolders.GetValueOrDefault(ID, null);
        if (thing != null) {
            return thing.gameObject;
        }
        Debug.Log("Thing not found: " + ID);
        //Debug.Log("Call Stack: " + Environment.StackTrace);
        return null;
    }
    public static List<GameObject> GetGameObjects(List<int> IDs) {
        List<GameObject> objects = new List<GameObject>();
        IDs.ForEach(id => {
            GameObject thing = GetGameObjectWithID(id);
            if (thing != null) objects.Add(thing);
        });
        return objects;
    }

    public static void ClearIDHoldersList()
    {
        allIDHolders.Clear();
    }
}
