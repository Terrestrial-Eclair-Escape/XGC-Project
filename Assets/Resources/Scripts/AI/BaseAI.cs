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
    [HideInInspector] public bool IsMounted;
    [HideInInspector] public bool IsAttacking;
    [HideInInspector] public GameObject childObject;
    [HideInInspector] public GameObject parentObject;

    public AIValues aValues;

    private bool isTargetVisible;

    public void GetTarget()
    {
        target = GameObject.FindGameObjectWithTag(Constants.Tags.PlayerPositionTarget.ToString());
    }

    public bool GetTargetLastKnownLocation()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, target.transform.position - transform.position, out hit))
        {
            isTargetVisible = (hit.collider.CompareTag(Constants.Tags.Player.ToString()));

            targetLastKnownLocation = target.transform.position;
            variousTimers[(int)Constants.Timers.Searching] = aValues.TimeToStopSearch;
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

    public float DistanceToTarget { get { return Vector3.Distance(transform.position, target.transform.position); } }

    public bool IsTargetWithinRange 
    {
        get
        {
            if (target != null)
            {
                if (DistanceToTarget < aValues.DistanceToTarget)
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

        if (!IsMounted)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, Vector3.down, out hit))
            {
                startPos = new Vector3(transform.position.x, hit.point.y + transform.localScale.y / 2, transform.position.z);
                transform.position = startPos;
            }
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
        if(!IsStartWithinRange && !IsTargetWithinRange && variousTimers[(int)Constants.Timers.Searching] <= 0)
        {
            IsAggro = false;
            CharacterMove((startPos - transform.position).normalized);
        }
    }

    public void SetMount(Transform parent)
    {
        parentObject = parent.gameObject;
        transform.position = parentObject.transform.position;
        transform.eulerAngles = parentObject.transform.eulerAngles;
        IsMounted = true;
        rb.isKinematic = true;
        SetAnimValue(Constants.AnimatorBooleans.IsMounted, true);
    }

    public void AIKnockBack(Vector3 normal, float velocity)
    {
        parentObject = null;
        IsMounted = false;
        SetAnimValue(Constants.AnimatorBooleans.IsMounted, false);

        KnockBack(normal, velocity);
    }
}
