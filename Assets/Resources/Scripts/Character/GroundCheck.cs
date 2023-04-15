using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Script to ´check if an object is grounded or not.
/// <br/><br/>
/// TODO: Get it working with Physics.CheckSphere/Physics.SphereCast instead?
/// </summary>
public class GroundCheck : MonoBehaviour
{
    CharacterScript parent;
    bool isGrounded;

    public void Awake()
    {
        parent = transform.parent.GetComponent<CharacterScript>();
    }

    public bool IsGrounded {
        get
        {
            return isGrounded;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.tag.Equals("Enemy"))
        {
            isGrounded = true;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!other.tag.Equals("Enemy"))
        {
            isGrounded = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.tag.Equals("Enemy"))
        {
            isGrounded = false;
        }
    }
}
