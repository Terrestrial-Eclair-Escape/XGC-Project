using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HazardRollingEnemy : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit))
        {
            transform.position = new Vector3(transform.position.x, hit.point.y + transform.localScale.y / 2, transform.position.z);
        }

        transform.GetChild(0).localEulerAngles = GlobalScript.Instance.GetStartRot(transform, transform.GetChild(0));
    }
}
