using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Constant values, e.g. tags and paths.
/// </summary>
public class Constants
{
    public static string OmnipotentName = "_OMNIPOTENT";

    public static Vector2 DefaultMousePos = new Vector2(-9999999, -9999999);

    public enum MenuStates
    {
        None,
        Pause,
        Death,
        Victory,
        Title,
        Settings
    }

    public enum Inputs
    {
        Move,
        Look,
        Fire,
        Jump,
        Interact,
    }
    public enum UIInputs
    {
        Navigate,
        Submit,
        Cancel,
        Point,
        Click,
        ScrollWheel,
        MiddleClick,
        RightClick,
        TrackedDevicePosition,
        TrackedDeviceOrientation,
        Pause,
    }

    public enum Timers
    {
        CoyoteTimer,
        Invincibility,
        Searching,
        AIUniqueAttack,
    }

    public enum Tags
    {
        Untagged,
        Respawn,
        Finish,
        EditorOnly,
        MainCamera,
        Player,
        GameController,
        Pickup,
        MainObjective,
        UILoadingFillBar,
        UIButton,
        Goal,
    }

    public enum Layers
    {
        Default,
        TransparentFX,
        IgnoreRaycast,
        Ground,
        Water,
        UI,
    }

    public enum MaterialKeywords
    {
        _EMISSION,
        _EmissionColor,
    }

    public enum Scenes
    {
        InitialScene,
        TitleScreen,
        Loading,
        SampleScene,
        SebastianScene,
        Level,
        LevelAdjusted,
    }

    public enum CharacterAudioList
    {
        JumpVoice,
        JumpSfx,

        PickupVoice,
        PickupSfx,

        ThrowVoice,
        ThrowSfx,

        DropVoice,
        DropSfx,

        AttackVoice,
        AttackSfx,

        TakeDamageVoice,
        TakeDamageSfx,

        DieVoice,
        DieSfx,
    }

    public enum AnimatorBooleans
    {
        IsWalking,
        IsRunning,
        IsFalling,
    }
}
