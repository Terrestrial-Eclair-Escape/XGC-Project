using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// More globally used changeable values
/// </summary>
[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/SettingsValues", order = 1)]
public class SettingsValues : ScriptableObject
{
    [Tooltip("Time until a buffered input is forgotten.")]
    public float BufferLeniency;
    [Tooltip("Time until coyote time period runs out and a jump is lost.")]
    public float CoyoteTimeLeniency;
    [Tooltip("Minimum value of \"stick\" inputs before it's registered.")]
    public float StickDeadZone;
}
