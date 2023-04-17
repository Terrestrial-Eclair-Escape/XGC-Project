using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Character specific stats.
/// </summary>
[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/CharacterValues", order = 1)]
public class CharacterValues : ScriptableObject
{
    [Tooltip("Character max health.")]
    public int HealthMax;

    [Tooltip("How close to the character objects get recognized.")]
    public float PickupRadius;
    [Tooltip("Time for pickup to reach position. Lower value means slower pickup."), Range(0, 1)]
    public float PickupSpeed;
    [Tooltip("Max distance the character checks for collisions.")]
    public float PickupThrowMaxDistance;
    [Tooltip("Force to throw pickup.")]
    public float PickupForce;

    [Tooltip("Character move speed.")]
    public float MoveSpeed;
    [Tooltip("Acceleration value for character movement. Lower value means slower acceleration. Should be lower than MoveDecceleration."), Range(0, 1)]
    public float MoveAcceleration;
    [Tooltip("Decceleration value for character movement. Lower value means slower decceleration. Should be higher than MoveAcceleration."), Range(0, 1)]
    public float MoveDecceleration;
    [Tooltip("Character turn value. Lower value increases turn radius."), Range(0, 1)]
    public float MoveTurnValue;
    [Tooltip("Time before character moves after beginning moving.")]
    public float MoveTurnTime;

    [Tooltip("The number of times the character can jump.")]
    public float JumpAmount;
    [Tooltip("Initial jump force.")]
    public float JumpSpeed;
    [Tooltip("Gravity value used on ascent. Should be lower than JumpGravityDescend.")]
    public float JumpGravityAscend;
    [Tooltip("Gravity value used on descent. Should be higher than JumpGravityAscend.")]
    public float JumpGravityDescend;

}
