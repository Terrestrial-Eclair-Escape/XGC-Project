using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Base character movement script. Should not be placed directly on a character, do something like CharacterScript instead.
/// </summary>
public class BaseCharacterMovement : MonoBehaviour
{
    public CharacterValues cValues;     // character values
    public SettingsValues sValues;      // setting values
    public CapsuleCollider cCollider;   // capsule collider
    public Rigidbody rb;                // rigidbody
    public Transform pickupPosition;

    RaycastHit hit;
    public bool NearGround => Physics.SphereCast(transform.position, cCollider.radius * .9f, Vector3.down, out hit, (transform.localScale.y / 2) * 1.5f);
    [HideInInspector] public bool IsGrounded;           // is the character on the cround?
    [HideInInspector] public bool IsCoyoteTimeActive;   // does the character have a chance to perform the first jump from falling?
    [HideInInspector] public bool IsUTurn;              // is the character performing a u-turn?
    [HideInInspector] public List<float> coyoteTimer;   // list of coyotetimer values (realistically only Jump is used)
    [HideInInspector] public List<float> bufferTimer;   // list of timers for input buffers
    [HideInInspector] public Collider[] ObjectsInProximity => Physics.OverlapSphere(transform.position, cValues.PickupRadius, 1 << (int)Constants.Layers.Pickup);   // objects close to the character

    private GameObject pickedUpObject;
    private int healthCurrent;      // current health
    private int timesJumped;        // how many times in the current air session the character has jumped
    private Vector3 maxMoveValue;   // move value for acceleration, max 1
    private float moveSpeedModifierPickup = 1;
    private GameObject latestClosest;

    public void CharacterStart()
    {
        coyoteTimer = GlobalScript.Instance.GenerateInputList();
        bufferTimer = GlobalScript.Instance.GenerateInputList();
    }

    public void CharacterFixedUpdate()
    {
        BufferUpdate();
    }

    public void CharacterUpdate(Vector3 moveDir, Vector3 throwTarget, bool highlightPickup = false)
    {
        CharacterMove(moveDir);
        CharacterJump();

        ThrowObject(throwTarget);
        PickUpObject(highlightPickup);
    }

    public void CharacterLateUpdate()
    {
        CharacterOnAirborne();
    }

    /// <summary>
    /// Used for timers
    /// </summary>
    public void BufferUpdate()
    {
        for (int i = 0; i < bufferTimer.Count; i++)
        {
            if (bufferTimer[i] > 0)
            {
                bufferTimer[i] -= Time.deltaTime;
            }

            // the coyoteTimer list is the same length as the bufferTimer list, so run it in the same loop
            if (coyoteTimer[i] > 0)
            {
                coyoteTimer[i] -= Time.deltaTime;
            }
        }
    }

    public void CharacterMove(Vector3 moveDir)
    {
        // if we do a roughly 180 degree turn, set to true to adjust values to allow for a good transition
        IsUTurn = Vector3.Angle(transform.forward, moveDir) > 150 && maxMoveValue.magnitude > sValues.StickDeadZone;

        // is moveinput higher than our deadzone value
        if (moveDir.magnitude >= sValues.StickDeadZone && !IsUTurn)
        {
            // accelerate
            maxMoveValue = Vector3.Slerp(maxMoveValue, moveDir, cValues.MoveAcceleration);

            // rotate towards move input direction
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, Quaternion.LookRotation(moveDir), cValues.MoveTurnValue));
        }
        else
        {
            // deccelerate
            maxMoveValue = Vector3.Slerp(maxMoveValue, Vector3.zero, cValues.MoveDecceleration);

            // wait time before character starts moving (unnecessary?)
            bufferTimer[(int)Constants.Inputs.Move] = cValues.MoveTurnTime;
        }

        /* use this code if we want delay from input to movement (remove accelerate/deccelerate from above)
        if(bufferTimer[(int)Constants.Inputs.Move] <= 0 && moveDir.magnitude >= sValues.StickDeadZone && !IsUTurn)
        {
            // accelerate
            maxMoveValue = Vector3.Slerp(maxMoveValue, moveDir, cValues.MoveAcceleration);
        }
        else
        {
            // deccelerate
            maxMoveValue = Vector3.Slerp(maxMoveValue, Vector3.zero, cValues.MoveDecceleration);
        }*/

        // set maxMoveValue to 0 if lower than certain value
        if(maxMoveValue.magnitude < sValues.StickDeadZone)
        {
            maxMoveValue = Vector3.zero;
        }

        // move only if we have input
        if (maxMoveValue.magnitude > 0)
        {
            // if transform.forward, player always moves toward the direction they're looking
            // if maxMoveValue, adds inertia to movement 
            rb.MovePosition(rb.position + maxMoveValue * maxMoveValue.magnitude * Time.deltaTime * cValues.MoveSpeed);
        }
    }

    public void CharacterJump()
    {
        // if we have activated the buffer for jump
        if (bufferTimer[(int)Constants.Inputs.Jump] > 0)
        {
            // if we just landed, reset the amount of jumps remaining
            if (IsGrounded && timesJumped >= cValues.JumpAmount)
            {
                timesJumped = 0;
            }

            // jump if we have the ability to do so
            if (timesJumped < cValues.JumpAmount)
            {
                SetYVelocity(cValues.JumpSpeed);
                timesJumped++;
                bufferTimer[(int)Constants.Inputs.Jump] = 0;
            }

            IsGrounded = false;
            IsCoyoteTimeActive = false;
        }
    }

    void HighlightPickup(GameObject obj, bool highlight)
    {
        if (highlight)
        {
            obj.GetComponent<Renderer>().material.EnableKeyword(Constants.MaterialKeywords._EMISSION.ToString());
        }
        else
        {
            obj.GetComponent<Renderer>().material.DisableKeyword(Constants.MaterialKeywords._EMISSION.ToString());
        }
    }

    void SelectPickup(GameObject pickup)
    {
        pickedUpObject = pickup;
        if (pickedUpObject != null)
        {
            moveSpeedModifierPickup = 1 / pickedUpObject.GetComponent<Rigidbody>().mass;
            pickedUpObject.GetComponent<Renderer>().material.DisableKeyword(Constants.MaterialKeywords._EMISSION.ToString());
        }
    }

    void DeselectPickup()
    {
        moveSpeedModifierPickup = 1;
        pickedUpObject = null;
    }

    void PickUpObject(bool highlight = false)
    {
        GameObject closest = (pickedUpObject == null) ? GetClosestObjectOfType(highlight) : null;
        if(closest != null)
        {
            HighlightPickup(closest, highlight);
            latestClosest = closest;
        }

        if (bufferTimer[(int)Constants.Inputs.Interact] > 0)
        {
            if (pickedUpObject == null)
            {
                SelectPickup(closest);
            }
            else
            {
                DeselectPickup();
            }

            bufferTimer[(int)Constants.Inputs.Interact] = 0;
        }

        if (closest == null && latestClosest != null)
        {
            HighlightPickup(latestClosest, false);
            latestClosest = null;
        }

        if (pickedUpObject != null)
        {
            Rigidbody prb = pickedUpObject.GetComponent<Rigidbody>();
            prb.MovePosition(Vector3.Slerp(pickedUpObject.transform.position, pickupPosition.position, cValues.PickupSpeed));
            prb.velocity = new Vector3(prb.velocity.x, 0, prb.velocity.z);

            /* build on this? - drop object if another object is between object and character
            RaycastHit pHit;
            if(Physics.Raycast(pickedUpObject.transform.position, pickedUpObject.transform.position - transform.position, out pHit))
            {
                Debug.Log(pHit.collider.gameObject.name);
                if(pHit.collider.gameObject == gameObject)
                {
                }
            }
            else
            {
                pickedUpObject = null;
            }*/
        }
    }

    private void ThrowObject(Vector3 throwTarget)
    {
        if (bufferTimer[(int)Constants.Inputs.Fire] > 0)
        {
            if (pickedUpObject != null)
            {
                Vector3 startPos = pickedUpObject.transform.position;
                Vector3 target = Vector3.zero;
                
                /* aim at closest target (WIP)
                float dist = -1;

                foreach(RaycastHit h in Physics.RaycastAll(startPos, transform.TransformPoint(throwTarget)                                                                - startPos, cValues.PickupThrowMaxDistance))
                {
                    if (h.transform.CompareTag(Constants.Tags.Player.ToString()))
                    {
                        continue;
                    }

                    float compareDist = Vector3.Distance(startPos, h.point);
                    if (compareDist < dist || dist < 0)
                    {
                        dist = compareDist;
                        target = hit.point;
                    }
                }*/

                if (target == Vector3.zero)
                {
                    target = throwTarget; 
                }
                target -= startPos;

                Vector3 force = target.normalized;
                force.y += 0.1f;

                // TODO: Decide on velocity or AddForce
                pickedUpObject.GetComponent<Rigidbody>().velocity = (force * cValues.PickupForce);
                DeselectPickup();
            }

            bufferTimer[(int)Constants.Inputs.Fire] = 0;
        }
    }

    public GameObject GetClosestObjectOfType(bool highlight = false)
    {
        GameObject toReturn = null;

        if (ObjectsInProximity.Length == 1)
        {
            toReturn = ObjectsInProximity[0].gameObject;
        }
        else if (ObjectsInProximity.Length > 1)
        {
            GameObject closest = null;
            float closestDistance = -1;
            float dist = -1;

            foreach (Collider c in ObjectsInProximity)
            {
                if (highlight)
                {
                    HighlightPickup(c.gameObject, false);
                }

                dist = Vector3.Distance(transform.position, c.transform.position);
                if (dist < closestDistance && closestDistance >= 0 || closestDistance < 0)
                {
                    if(highlight && closest != null)
                    {
                        HighlightPickup(closest, false);
                    }

                    closestDistance = dist;
                    closest = c.gameObject;
                }
            }
            toReturn = closest;
        }

        return toReturn;
    }

    /// <summary>
    /// Set velocity's Y value directly to add an impulse in character movement (for e.g. jumps).
    /// </summary>
    /// <param name="yVel"></param>
    public void SetYVelocity(float yVel)
    {
        rb.velocity = new Vector3(rb.velocity.x, yVel, rb.velocity.z);
    }

    /// <summary>
    /// Accelerate fall speed of character
    /// </summary>
    void ApplyGravity()
    {
        // fall faster when fall has begun
        rb.velocity -= new Vector3(0, (rb.velocity.y < 0) ? cValues.JumpGravityDescend : cValues.JumpGravityAscend, 0) * Time.deltaTime;
    }

    public void TakeDamage(int damage)
    {
        healthCurrent -= damage;

        // if we recieve negative damage (gain health), cap it at max
        if (healthCurrent > cValues.HealthMax)
        {
            healthCurrent = cValues.HealthMax;
        }

        // check if we have lost all health
        if (healthCurrent <= 0)
        {
            Die();
        }
    }

    public void Die()
    {

    }

    public void CharacterOnGrounded()
    {
        timesJumped = 0;
        IsGrounded = true;
        IsCoyoteTimeActive = false;
    }

    public void CharacterOnAirborne()
    {
        if (!IsGrounded)
        {
            // if we haven't jumped yet
            if (timesJumped == 0)
            {
                // if coyotetimer runs out, act as if first jump has been made
                if (coyoteTimer[(int)Constants.Inputs.Jump] <= 0 && IsCoyoteTimeActive)
                {
                    timesJumped++;
                    IsCoyoteTimeActive = false;
                }
            }

            // apply extra gravity values for more satisfying jump arc/fall speed
            ApplyGravity();
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (NearGround)
        {
            timesJumped = 0;
            IsCoyoteTimeActive = false;
        }
    }

    void OnCollisionStay(Collision collision)
    {
        if (NearGround)
        {
            if (!IsGrounded)
            {
                CharacterOnGrounded();
            }
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (IsGrounded)
        {
            IsGrounded = false;
            coyoteTimer[(int)Constants.Inputs.Jump] = sValues.CoyoteTimeLeniency;
            IsCoyoteTimeActive = true;
        }
    }

#if UNITY_EDITOR
    private Color gizmoColor = new Color32(255, 0, 0, 100);

    private void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;

        // ground check
        Gizmos.DrawWireSphere(new Vector3(transform.position.x, transform.position.y - (transform.localScale.y / 2) * 1.5f, transform.position.z), 0.45f);

        // pickup radius
        Gizmos.DrawWireSphere(transform.position, cValues.PickupRadius);

        Gizmos.DrawRay(Camera.main.transform.position, Camera.main.transform.forward * cValues.PickupThrowMaxDistance);
    }
#endif
}
