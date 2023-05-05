using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface CharacterInterface
{
    void OnDead();
    IEnumerator Dying();
}
