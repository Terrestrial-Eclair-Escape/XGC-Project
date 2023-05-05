using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface AIInterface
{
    IEnumerator WakeUp();
    IEnumerator Sleep();
}
