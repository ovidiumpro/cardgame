using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UniqueIdGenerator
{
    private HashSet<int> usedIds;
    private int currentMaxId;
    private System.Random random;

    public UniqueIdGenerator()
    {
        usedIds = new HashSet<int>();
        currentMaxId = 1;
        random = new System.Random();
    }

    public int GenerateUniqueId(int maxId)
    {
        // If the maxId is greater than the currentMaxId, update the currentMaxId
        if(maxId > currentMaxId)
        {
            currentMaxId = maxId;
        }

        int newId;
        // Generate new ids until we find one that isn't used
        do
        {
            newId = random.Next(1, currentMaxId + 1);
        }
        while (usedIds.Contains(newId));

        // Add the new id to the set of used ids
        usedIds.Add(newId);

        return newId;
    }
}

