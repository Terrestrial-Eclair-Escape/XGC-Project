using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Values for functions available only to AI.
/// </summary>
[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/AIValues", order = 1)]
public class AIValues : ScriptableObject
{
    [Tooltip("Max distance from startPos that target is considered within range.")]
    public float DistanceToStartPos;
    [Tooltip("Max distance to target before AI loses track of them.")]
    public float DistanceToTarget;
    [Tooltip("The extra distance the AI follows target after losing sight of it.")]
    public float DistanceToSearch;

    [Tooltip("Time before AI stops engaging the target and starting to return to startPos.")]
    public float TimeToStopSearch;
    [Tooltip("Time it takes for AI to wake up/start attacking.")]
    public float TimeToWakeUp;
    [Tooltip("Time it takes for AI to go to sleep after returning to its startPos.")]
    public float TimeToSleep;
}
