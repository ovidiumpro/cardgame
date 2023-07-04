using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class Command
{
    public static LinkedList<Command> CommandList = new LinkedList<Command>();
    public static event Action OnQueueEmpty;
    public static bool playingQueue = false;
    public static bool queuePaused = false;
    //public static bool commandExecuting = false;

    public virtual void AddToQueue(bool addToFront = false)
    {
        Debug.Log("Adding command: " + this.ToString());
        if (addToFront)
        {
            CommandList.AddFirst(this);
        }
        else
        {
            CommandList.AddLast(this);
        }
        if (!playingQueue && !queuePaused)
        {
            //Debug.Log("Playing command from AddToQueue");
            playingQueue = true;
            PlayFirstCommandFromQueue();
        }

    }
    // public static void CommandExecutionFlagUpdate() {
    //     commandExecuting = false;
    // }

    public virtual void StartCommandExecution()
    {
        // list of everything that we have to do with this command (draw a card, play a card, play spell effect, etc...)
        // there are 2 options of timing : 
        // 1) use tween sequences and call CommandExecutionComplete in OnComplete()
        // 2) use coroutines (IEnumerator) and WaitFor... to introduce delays, call CommandExecutionComplete() in the end of coroutine
    }

    public static void CommandExecutionComplete()
    {
        if (CommandList.Count > 0)
        {

            PlayFirstCommandFromQueue();
        }

        else
            playingQueue = false;
        //Debug.Log("Invoking command empty event");
        OnQueueEmpty?.Invoke();
        // if (TurnManager.Instance.whoseTurn != null)
        //     TurnManager.Instance.whoseTurn.HighlightPlayableObjects();
    }

    public static void PlayFirstCommandFromQueue()
    {
        if (queuePaused)
        {
            playingQueue = false;
            return;
        }
        if (Command.CommandList.Count > 0)
        {
            playingQueue = true;
            Command c = CommandList.First.Value;
            CommandList.RemoveFirst();
            // commandExecuting = true;
            c.StartCommandExecution();
        }

    }

    public static void PauseQueueExecution()
    {
        queuePaused = true;
    }

    public static void ResumeQueueExecution()
    {
        queuePaused = false;
        if (!playingQueue)
            PlayFirstCommandFromQueue();
    }

    public static bool CardDrawPending()
    {
        foreach (Command c in CommandList)
        {
            if (c is DrawACardCommand)
                return true;
        }
        return false;
    }
}
