using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Base character movement script. Should not be placed directly on a character, do something like CharacterScript instead.
/// </summary>
public class BaseCharacterMovement : MonoBehaviour
{
    public CharacterValues cValues;     // character values
    public CharacterAudio cAudio;
    public CharacterAnimations cAnims;
    public SettingsValues sValues;      // setting values
    public CapsuleCollider cCollider;   // capsule collider
    public Rigidbody rb;                // rigidbody
    public AudioSource audioSource;
    public Animator anim;
    public Transform pickupPosition;
    public Transform characterModelPosition;
    public Transform characterModelMeshParent;

    RaycastHit hit;
    public bool NearGround => Physics.SphereCast(transform.position, cCollider.radius * .9f, Vector3.down, out hit, (transform.localScale.y / 2) * sValues.MaxDistanceCharacterGrounded);
    [HideInInspector] public bool IsGrounded;           // is the character on the cround?
    [HideInInspector] public bool IsCoyoteTimeActive;   // does the character have a chance to perform the first jump from falling?
    [HideInInspector] public bool IsUTurn;              // is the character performing a u-turn?
    [HideInInspector] public bool IsDead;               // character death has started
    [HideInInspector] public bool HasDied;              // should character death start?
    [HideInInspector] public float[] variousTimers;   // list of variousTimers values (realistically only Jump is used)
    [HideInInspector] public float[] bufferTimers;   // list of timers for input buffers
    [HideInInspector] public Collider[] ObjectsInProximity => Physics.OverlapSphere(transform.position, cValues.PickupRadius).Where(x => x.CompareTag(Constants.Tags.Pickup.ToString()) || x.CompareTag(Constants.Tags.MainObjective.ToString())).ToArray();   // objects close to the character

    private GameObject pickedUpObject;
    private int healthCurrent;      // current health
    private int timesJumped;        // how many times in the current air session the character has jumped
    private Vector3 maxMoveValue;   // move value for acceleration, max 1
    private float moveSpeedModifierPickup = 1;
    private GameObject latestClosest;
    private Vector3 debugStartPos;
    private Vector3 moveDirGlobal;
    private float latestDamageImmunityBlinkTimerValue = -1;
    private Vector3 lastPos;

    public void CharacterStart()
    {
        variousTimers = GlobalScript.Instance.GenerateTimerList();
        bufferTimers = GlobalScript.Instance.GenerateInputList();
        debugStartPos = transform.position;
        healthCurrent = cValues.HealthMax;
    }

    public void CharacterFixedUpdate()
    {
        BufferUpdate();
        CharacterJump();
        CharacterMove(moveDirGlobal);
    }

    public void CharacterUpdate(Vector3 moveDir, Vector3 throwTarget, bool highlightPickup = false)
    {
        this.moveDirGlobal = moveDir;
        
        CharacterFallOffRespawnDebug();

        DamageImmunityVisualizer();

        ThrowObject(throwTarget);
        PickUpObject(highlightPickup);
    }

    public void CharacterLateUpdate()
    {
        CharacterOnAirborne();
    }

    public void CharacterFallOffRespawnDebug()
    {
        if(transform.position.y < -50)
        {
            transform.position = debugStartPos;
        }
    }

    /// <summary>
    /// Used for timers
    /// </summary>
    public void BufferUpdate()
    {
        for (int i = 0; i < bufferTimers.Length; i++)
        {
            if (bufferTimers[i] > 0)
            {
                bufferTimers[i] -= Time.deltaTime;
            }
        }
        for (int i = 0; i < variousTimers.Length; i++)
        {
            if (variousTimers[i] > 0)
            {
                variousTimers[i] -= Time.deltaTime;
            }
        }
    }

    /// <summary>
    /// Play audio from the characters audio list.
    /// </summary>
    /// <param name="audioList"></param>
    /// <param name="specifiedAudio"></param>
    public void PlayAudio(Constants.CharacterAudioList audioList, int specifiedAudio = -1)
    {
        if(audioSource != null)
        {
            AudioClip[] clips = (AudioClip[])cAudio.GetType().GetField(audioList.ToString()).GetValue(cAudio);
            if (clips.Any())
            {
                if(specifiedAudio >= 0 && specifiedAudio < clips.Length)
                {
                    audioSource.PlayOneShot(clips[specifiedAudio]);
                }
                else
                {
                    audioSource.PlayOneShot(clips[UnityEngine.Random.Range(0, clips.Length)]);
                }
            }
        }
    }

    void SetAnimValue(Constants.AnimatorBooleans animat, object value)
    {
        if (anim != null)
        {
            if (value is bool)
            {
                anim.SetBool(animat.ToString(), (bool)value);
            }
            if (value is int)
            {
                anim.SetInteger(animat.ToString(), (int)value);
            }
            if (value is float)
            {
                anim.SetFloat(animat.ToString(), (float)value);
            }
        }
    }

    void SetMaxAnimSpeed(float speed)
    {
        if(anim != null)
        {
            if (anim.speed > speed)
            {
                anim.speed = speed;
            }
        }
    }

    public void CharacterLeanAngleOnMove(Vector3 moveDir)
    {
        float maxAngle = 90;
        float angle = Quaternion.Angle(rb.rotation, Quaternion.LookRotation(moveDir)); 
        if (angle > 100) { angle = 100; }
        //angle /= 100;
        float angleDir = GlobalScript.Instance.AngleDir(rb.rotation.eulerAngles, Quaternion.LookRotation(moveDir).eulerAngles, transform.up);
        // Debug.Log($"{angle} {angleDir}");

        characterModelPosition.localEulerAngles = new Vector3(0, 0, Mathf.LerpAngle(-maxAngle, maxAngle, 0.5f + angle * angleDir));
        
    }

    public void CharacterRotateTowards(Vector3 rotation)
    {
        Quaternion lookRot = Quaternion.LookRotation(rotation, Vector3.up);
        Quaternion rotateTo = Quaternion.RotateTowards(rb.rotation, lookRot, Mathf.Lerp(cValues.MoveTurnAngleSlow, cValues.MoveTurnAngleFast, GlobalScript.Instance.NullYAxis(rb.velocity).magnitude));
        rb.MoveRotation(rotateTo);

        float maxAngle = 90;
        float angleDir = GlobalScript.Instance.AngleDir(rb.rotation.eulerAngles, Quaternion.LookRotation(rotation).eulerAngles, transform.up);
        characterModelPosition.localRotation = Quaternion.Euler(0, 0, Mathf.LerpAngle(-maxAngle, maxAngle, 0.5f + rotateTo.z * angleDir));

        // slightly angle the player on rotation (WIP)
        //CharacterLeanAngleOnMove(rotateTo.eulerAngles);
    }

    public void CharacterMove(Vector3 moveDir)
    {
        // if we do a roughly 180 degree turn, set to true to adjust values to allow for a good transition
        IsUTurn = Vector3.Angle(transform.forward, moveDir) > 150 && maxMoveValue.magnitude > sValues.StickDeadZone;

        // is moveinput higher than our deadzone value
        if (moveDir.magnitude >= sValues.StickDeadZone && !IsUTurn)
        {
            // accelerate
            maxMoveValue = Vector3.MoveTowards(maxMoveValue, moveDir, cValues.MoveAcceleration);

            // rotate towards move input direction
            CharacterRotateTowards(moveDir);
        }
        else
        {
            // deccelerate
            maxMoveValue = Vector3.MoveTowards(maxMoveValue, Vector3.zero, cValues.MoveDecceleration);
        }


        // set maxMoveValue to 0 if lower than certain value
        if(maxMoveValue.magnitude < sValues.StickDeadZone)
        {
          //  maxMoveValue = Vector3.zero;
        }

        // move only if we have input
        if (maxMoveValue.magnitude > 0)
        {
            // if transform.forward, player always moves toward the direction they're looking
            // if maxMoveValue, adds inertia to movement 
            //                              vvvv
            rb.MovePosition(rb.position + transform.forward * maxMoveValue.magnitude * Time.deltaTime * cValues.MoveSpeed * moveSpeedModifierPickup);
        }

        // apply extra gravity values for more satisfying jump arc/fall speed
        ApplyGravity();

        Vector3 currentPos = GlobalScript.Instance.NullYAxis(transform.position);

        if (NearGround && IsGrounded)
        {
            float currentSpeed = Vector3.Distance(lastPos, currentPos);
            float maxSpeed = cValues.MoveSpeed* Time.deltaTime;

            if (currentSpeed < maxSpeed * sValues.AnimationThresholdWalk && moveDir.magnitude < sValues.StickDeadZone)
            {
                anim.speed = 1;
                SetAnimValue(Constants.AnimatorBooleans.IsWalking, false);
                SetAnimValue(Constants.AnimatorBooleans.IsRunning, false);
            }
            else if (currentSpeed > maxSpeed * sValues.AnimationThresholdRun)
            {
                anim.speed = sValues.AnimationThresholdRun + moveDir.magnitude * (1 - sValues.AnimationThresholdRun);
                SetAnimValue(Constants.AnimatorBooleans.IsWalking, true);
                SetAnimValue(Constants.AnimatorBooleans.IsRunning, true);
            }
            else
            {
                anim.speed = 0.5f + moveDir.magnitude;
                SetMaxAnimSpeed(1);
                SetAnimValue(Constants.AnimatorBooleans.IsWalking, true);
                SetAnimValue(Constants.AnimatorBooleans.IsRunning, false);
            }
        }

        lastPos = currentPos;
    }

    public void ResetMaxMoveValue()
    {
        maxMoveValue = Vector3.zero;
    }

    #region Y-velocity
    public void CharacterJump()
    {
        // if we have activated the buffer for jump
        if (bufferTimers[(int)Constants.Inputs.Jump] > 0)
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
                bufferTimers[(int)Constants.Inputs.Jump] = 0;
                PlayAudio(Constants.CharacterAudioList.JumpVoice);
                PlayAudio(Constants.CharacterAudioList.JumpSfx);
            }

            IsGrounded = false;
            IsCoyoteTimeActive = false;
        }
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
        // limit fall speed
        if(rb.velocity.y < -cValues.JumpGravityDescend)
        {
            rb.velocity = new Vector3(rb.velocity.x, -cValues.JumpGravityDescend, rb.velocity.z);
        }
        // fall faster when fall has begun
        else
        {
            rb.velocity -= new Vector3(0, (rb.velocity.y < 0) ? cValues.JumpGravityDescend : cValues.JumpGravityAscend, 0) * Time.deltaTime;
        }
    }
    #endregion

    #region PickUp
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
            moveSpeedModifierPickup = 1f / pickedUpObject.GetComponent<Rigidbody>().mass;
            pickedUpObject.GetComponent<Renderer>().material.DisableKeyword(Constants.MaterialKeywords._EMISSION.ToString());

            PlayAudio(Constants.CharacterAudioList.PickupVoice);
            PlayAudio(Constants.CharacterAudioList.PickupSfx);
        }
    }

    void DeselectPickup()
    {
        pickedUpObject = null;
        moveSpeedModifierPickup = 1f;
    }

    void PickUpObject(bool highlight = false)
    {
        if (pickupPosition != null)
        {
            GameObject closest = (pickedUpObject == null) ? GetClosestObjectOfType(highlight) : null;
            if (closest != null)
            {
                HighlightPickup(closest, highlight);
                latestClosest = closest;
            }

            if (bufferTimers[(int)Constants.Inputs.Interact] > 0)
            {
                if (pickedUpObject == null)
                {
                    SelectPickup(closest);
                }
                else
                {
                    DeselectPickup();

                    PlayAudio(Constants.CharacterAudioList.DropVoice);
                    PlayAudio(Constants.CharacterAudioList.DropSfx);
                }

                bufferTimers[(int)Constants.Inputs.Interact] = 0;
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
                prb.velocity = GlobalScript.Instance.NullYAxis(prb.velocity);

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
    }

    private void ThrowObject(Vector3 throwTarget)
    {
        if (bufferTimers[(int)Constants.Inputs.Fire] > 0)
        {
            if (pickedUpObject != null)
            {
                Vector3 startPos = pickedUpObject.transform.position;
                Vector3 target = Vector3.zero;
                
                /* aim at closest target according to center of screen (WIP)
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
                pickedUpObject.GetComponent<Rigidbody>().AddForce(force * cValues.PickupForce, ForceMode.Impulse);
                DeselectPickup();

                PlayAudio(Constants.CharacterAudioList.ThrowVoice);
                PlayAudio(Constants.CharacterAudioList.ThrowSfx);
            }

            bufferTimers[(int)Constants.Inputs.Fire] = 0;
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
            float dist;

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
    #endregion

    #region Damage
    public void TakeDamage(int damage)
    {
        if (variousTimers[(int)Constants.Timers.Invincibility] <= 0)
        {
            healthCurrent -= damage;

            // if we recieve negative damage (gain health) for some reason, cap it at max
            if (healthCurrent > cValues.HealthMax)
            {
                healthCurrent = cValues.HealthMax;
            }

            // check if we have lost all health
            if (healthCurrent <= 0)
            {
                healthCurrent = 0;
                Die();
            }
            else
            {
                variousTimers[(int)Constants.Timers.Invincibility] = cValues.HealthDamageImmunity;

                PlayAudio(Constants.CharacterAudioList.TakeDamageVoice);
                PlayAudio(Constants.CharacterAudioList.TakeDamageSfx);
            }
        }
    }

    /// <summary>
    /// Visual indicator on the players current damage immunity. AKA Flash of Pain
    /// </summary>
    void DamageImmunityVisualizer()
    {
        float modBlinkTimer = (variousTimers[(int)Constants.Timers.Invincibility] % sValues.DamageImmunityBlinkTimer);
        
        // 0: not hurt, 1: hurt (still fresh), 2: hurt (wearing off)
        int blinkState = (variousTimers[(int)Constants.Timers.Invincibility] >= sValues.DamageImmunityLeniency && modBlinkTimer > latestDamageImmunityBlinkTimerValue) ? 1 : 
            (variousTimers[(int)Constants.Timers.Invincibility] > 0 && variousTimers[(int)Constants.Timers.Invincibility] < sValues.DamageImmunityLeniency) ? 2 : 0;

        if (blinkState != 0)
        {
            // blink while taking damage
            foreach (Transform child in characterModelMeshParent.transform)
            {
                MeshRenderer r = child.GetComponent<MeshRenderer>();
                if (r != null)
                {
                    r.enabled = (blinkState == 1) ? !r.enabled : true;
                }
            }
        }

        latestDamageImmunityBlinkTimerValue = modBlinkTimer;
    }

    public void Die()
    {
        HasDied = true;

        PlayAudio(Constants.CharacterAudioList.DieVoice);
        PlayAudio(Constants.CharacterAudioList.DieSfx);
    }
    #endregion

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
            rb.velocity -= GlobalScript.Instance.NullYAxis(rb.velocity);

            // if we haven't jumped yet
            if (timesJumped == 0)
            {
                // if variousTimers runs out, act as if first jump has been made
                if (variousTimers[(int)Constants.Timers.CoyoteTimer] <= 0 && IsCoyoteTimeActive)
                {
                    timesJumped++;
                    IsCoyoteTimeActive = false;
                }
            }

            if (!NearGround)
            {
                SetAnimValue(Constants.AnimatorBooleans.IsFalling, true);
            }
        }

        if (NearGround)
        {
            SetAnimValue(Constants.AnimatorBooleans.IsFalling, false);
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
            IsCoyoteTimeActive = true;
            variousTimers[(int)Constants.Timers.CoyoteTimer] = sValues.CoyoteTimeLeniency;
        }
    }

#if UNITY_EDITOR
    private Color gizmoColor = new Color32(255, 0, 0, 100);

    private void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;

        // ground check
        Gizmos.DrawWireSphere(new Vector3(transform.position.x, transform.position.y - (transform.localScale.y / 2) * sValues.MaxDistanceCharacterGrounded, transform.position.z), 0.45f);

        // pickup radius
        Gizmos.DrawWireSphere(transform.position, cValues.PickupRadius);

        Gizmos.DrawRay(Camera.main.transform.position, Camera.main.transform.forward * cValues.PickupThrowMaxDistance);
    }
#endif
}
