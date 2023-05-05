using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The audio files a character can have and use
/// </summary>
[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/CharacterAudio", order = 1)]
public class CharacterAudio : ScriptableObject
{
    public AudioClip[] JumpVoice;
    public AudioClip[] JumpSfx;

    public AudioClip[] AttackVoice;
    public AudioClip[] AttackSfx;

    public AudioClip[] TakeDamageVoice;
    public AudioClip[] TakeDamageSfx;

    public AudioClip[] DieVoice;
    public AudioClip[] DieSfx;

    public AudioClip[] PickupVoice;
    public AudioClip[] PickupSfx;

    public AudioClip[] ThrowVoice;
    public AudioClip[] ThrowSfx;

    public AudioClip[] DropVoice;
    public AudioClip[] DropSfx;
}
