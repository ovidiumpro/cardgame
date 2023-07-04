using UnityEngine;
using System.Collections;

public enum AreaPosition{Top, Low}

public class PlayerArea : MonoBehaviour 
{
    public AreaPosition owner;
    public bool ControlsON = true;
    public PlayerDeckVisual PDeck;
    public ManaPoolVisual ManaBar;
    public HandVisual handVisual;
    public PlayerPortraitVisual Portrait;
    public HeroPowerButton HeroPower;
    public TableVisual tableVisual;
    public Transform PortraitPosition;
    public Transform PlayCardStackPosition;

    public GameObject PlayStackControls;

    public bool AllowedToControlThisPlayer
    {
        get;
        set;
    }      



}
