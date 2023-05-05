using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/CharacterAnimations", order = 1)]
public class CharacterAnimations : ScriptableObject
{
    public AnimationClip Idle;
    public AnimationClip Walk;
    public AnimationClip Run;
    public AnimationClip Jump;
}
