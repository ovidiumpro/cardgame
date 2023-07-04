using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CallbackCommand : Command
{
    private Action _action;

    public CallbackCommand(Action action)
    {
        Debug.Log("Creating callback command");
        _action = action;
    }

    public override void StartCommandExecution()
    {
        _action.Invoke();
        CommandExecutionComplete();
    }
}
