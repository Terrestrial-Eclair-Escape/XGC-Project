using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Extra values used for lists on various ScriptableObjects, e.g. CharacterValues to have unique change-friendly values for each character,
/// </summary>
[System.Serializable]
public class ExtraValues
{
    [Tooltip("AKA Name. The purpose for this value, only used for clarification in the Inspector.")]
    public string Purpose;
    [Tooltip("The value for this ExtraValue entry.")]
    public float Value;
}
