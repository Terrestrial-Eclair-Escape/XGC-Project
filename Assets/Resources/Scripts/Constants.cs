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
    public static Vector3 InvalidVector3 = new Vector3(-9999999, -9999999, -9999999);

    public static Vector3 HalfVector = new Vector3(0.5f, 0.5f, 0);
    public static Vector3 CenterOfScreen = new Vector3(Screen.width/2, Screen.height/2, 0);
    public enum MenuStates
    {
        None,
        Pause,
        Death,
        Victory,
        Title,
        Settings,
        Credits,
        Gallery,
    }
    public enum MenuOptions
    {
        Button_Start,
        Button_Quit,
        Button_ReturnToTitle,
        Button_Resume,
        Button_Restart,
        Button_Gallery,
        Button_Credits,
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

    public enum UIElements
    {
        Health,
        Reticle,
        TitleScreen,
        TitleMenu,
        PauseMenu,
        DeathMenu,
        VictoryMenu,
    }

    public enum Timers
    {
        CoyoteTimer,
        Invincibility,
        Searching,
        AIUniqueAttack,
        AICooldown,
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
        Enemy,
        PlayerPositionTarget,
    }

    public enum Layers
    {
        Default,
        TransparentFX,
        IgnoreRaycast,
        Ground,
        Water,
        UI,
        Pickup,
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
        IsDead,
        IsMounted,
    }
}
