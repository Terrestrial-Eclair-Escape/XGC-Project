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
    }

    public enum Tags
    {
        Untagged,
        Respawn,
        Finish,
        EditorOnly,
        MainCamera,
        Player,
        GameController
    }

    public enum Layers
    {
        Default,
        TransparentFX,
        IgnoreRaycast,
        Ground,
        Water,
        UI
    }
}
