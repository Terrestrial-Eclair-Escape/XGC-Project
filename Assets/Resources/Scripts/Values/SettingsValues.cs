using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// More globally used changeable values
/// </summary>
[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/SettingsValues", order = 1)]
public class SettingsValues : ScriptableObject
{
    [Tooltip("Minimum value of \"stick\" inputs before it's registered.")]
    public float StickDeadZone;
    [Tooltip("Time until a buffered input is forgotten.")]
    public float BufferLeniency;
    [Tooltip("Time until coyote time period runs out and a jump is lost.")]
    public float CoyoteTimeLeniency;
    [Tooltip("Length of time a character is not blinking while still immune to damage.")]
    public float DamageImmunityLeniency;
    [Tooltip("How frequently a character is blinking while in damage immunity")]
    public float DamageImmunityBlinkTimer;
}
