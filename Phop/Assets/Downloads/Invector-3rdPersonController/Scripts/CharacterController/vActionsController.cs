using UnityEngine;
using System.Collections;
using System;

[System.Serializable]
public class vActionsController
{
    [Header("--- Character Actions ---")]
    public Interact Interact;
    public DragBox DragBox;
    public Attack Attack;
    public Defense Defense;
    public LockOn LockOn;
    public Strafe Strafe;
    public Jump Jump;
    public Roll Roll;
    public Crouch Crouch;
    public Sprint Sprint;
}
[System.Serializable]
public abstract class Actions
{
    public bool use = true;
    public bool options;   
}

[System.Serializable]
public class Interact : Actions
{
    public InputName input = InputName.A;
}

[System.Serializable]
public class DragBox : Actions
{
    public InputName input = InputName.B;
}

[System.Serializable]
public class Attack : Actions
{
    public InputName input = InputName.RB;
}

[System.Serializable]
public class Defense : Actions
{
    public InputName input = InputName.LB;
}

[System.Serializable]
public class Strafe : Actions
{
    public InputName input = InputName.RightStickClick;
}

[System.Serializable]
public class LockOn : Actions
{
    public InputName input = InputName.RightStickClick;
}

[System.Serializable]
public class Roll : Actions
{
    public InputName input = InputName.B;    
    public float staminaCost = 25f;
    public float recoveryDelay = 2f;
}

[System.Serializable]
public class Crouch : Actions
{
    public InputName input = InputName.Y;
    [Tooltip("Press the button to crouch or just hit the button once - leave this bool UNCHECK if you are using MOBILE.")]
    public bool pressToCrouch = true;
}

[System.Serializable]
public class Sprint : Actions
{
    public InputName input = InputName.LeftStickClick;
    public float staminaCost = 30f;
    public float recoveryDelay = 0.25f;
}

[System.Serializable]
public class Jump : Actions
{
    public InputName input = InputName.X;

    [Tooltip("Check to control the character while jumping")]
    public bool jumpAirControl = true;

    [Tooltip("Add Extra jump speed, based on your speed input the character will move forward")]
    public float jumpForward = 4f;

    [Tooltip("Add Extra jump height, if you want to jump only with Root Motion leave the value with 0.")]
    public float jumpForce = 4f;
 
    public float staminaCost = 30f;
    public float recoveryDelay = 1f;

}

public enum InputName
{
    A,
    B,
    X,
    Y,
    RB,
    LB,
    RT,
    LT,
    LeftStickClick,
    RightStickClick, 
    Start, 
    Back
}