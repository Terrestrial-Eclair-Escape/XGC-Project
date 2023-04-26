using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseAI : BaseCharacterMovement
{
    [HideInInspector] public GameObject target;
    [HideInInspector] public Vector3 startPos;
    [HideInInspector] public Vector3 targetLastKnownLocation;
    [HideInInspector] public bool IsAggro;
    [HideInInspector] public bool IsAwake;
    public AIValues aValues;

    private bool isTargetVisible;

    public void GetTarget()
    {
        target = GameObject.FindGameObjectWithTag(Constants.Tags.Player.ToString());
    }

    public bool GetTargetLastKnownLocation()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, target.transform.position - transform.position, out hit))
        {
            isTargetVisible = (hit.collider.CompareTag(Constants.Tags.Player.ToString()));

            targetLastKnownLocation = target.transform.position;
            coyoteTimer[(int)Constants.Inputs.Move] = aValues.TimeToStopSearch;
        }
        else
        {
            if (isTargetVisible)
            {
                Vector3 dir = target.transform.eulerAngles;
                targetLastKnownLocation = new Vector3(target.transform.position.x + dir.x * aValues.DistanceToSearch, 
                    target.transform.position.y + dir.y * aValues.DistanceToSearch, 
                    target.transform.position.z + dir.z * aValues.DistanceToSearch);
            }

            isTargetVisible = false;
        }

        return isTargetVisible;
    }

    public bool IsTargetWithinRange 
    {
        get
        {
            if (target != null)
            {
                if (Vector3.Distance(transform.position, target.transform.position) < aValues.DistanceToTarget)
                {
                    return GetTargetLastKnownLocation();
                }
            }

            return false;
        }
    }

    public bool IsStartWithinRange
    {
        get
        {
            if (target != null)
            {
                if (Vector3.Distance(transform.position, startPos) < aValues.DistanceToStartPos)
                {

                    return true;
                }
            }

            return false;
        }
    }
    public void AIStart()
    {
        CharacterStart();

        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit))
        {
            startPos = new Vector3(transform.position.x, hit.point.y + transform.localScale.y/2, transform.position.z);
            transform.position = startPos;
        }
    }

    public void AIUpdate()
    {
        CharacterFallOffRespawnDebug();

        if (target == null)
        {
            GetTarget();
        }

        if (target != null)
        {
            if (!IsTargetWithinRange)
            {
                ReturnToStartPos();
            }
        }
    }

    public void ReturnToStartPos()
    {
        if(!IsStartWithinRange && !IsTargetWithinRange && coyoteTimer[(int)Constants.Inputs.Move] <= 0)
        {
            IsAggro = false;
            CharacterMove((startPos - transform.position).normalized);
        }
    }
}
