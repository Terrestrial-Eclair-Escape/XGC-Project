using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Constant values, e.g. tags and paths.
/// </summary>
public class Constants
{
    public enum Inputs
    {
        Move,
        Look,
        Fire,
        Jump,
        Interact,
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
}
