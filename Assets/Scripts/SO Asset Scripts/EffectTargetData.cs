using System.Collections;
using System.Collections.Generic;
using CG.Cards;
using UnityEngine;

public class EffectTargetData 
{
   public TargetingOptions targetType;
   public string promptMessage;

    public EffectTargetData(TargetingOptions targetType, string promptMessage)
    {
        this.targetType = targetType;
        this.promptMessage = promptMessage;
    }
}
