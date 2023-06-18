using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using Unity.VisualScripting;
using UnityEngine;

public class AIEnemyMountable : BaseAI, CharacterInterface, AIInterface
{
    Vector3 startRot;
    Vector3 lastPlacedPos;

    public Transform mountedPosition; 
    public GameObject mountedObject;
    Vector3 thisLastFramePos;
    Vector3 targetLastFramePos;

    // Start is called before the first frame update
    void Start()
    {
        AIStart();
        if(mountedPosition != null && mountedObject != null)
        {
            childObject = GameObject.Instantiate(mountedObject, mountedPosition.position, mountedPosition.rotation);
            childObject.GetComponent<BaseAI>().SetMount(mountedPosition);
        }
    }

    void FixedUpdate()
    {
        CharacterFixedUpdate();
    }

    // Update is called once per frame
    void Update()
    {
        if (!HasDied)
        {
            AIUpdate();

            if (target != null)
            {
                if (!IsMounted)
                {
                    if (IsTargetWithinRange && !IsAggro && !IsAwake)
                    {
                        StartCoroutine(WakeUp());
                    }

                    if (IsAggro && IsAwake && (IsTargetWithinRange || variousTimers[(int)Constants.Timers.Searching] > 0) && variousTimers[(int)Constants.Timers.Invincibility] <= 0)
                    {
                        Vector3 targetPos = new Vector3(targetLastKnownLocation.x, transform.position.y, targetLastKnownLocation.z);
                        Vector3 targetRot = (targetPos - transform.position).normalized;
                        Quaternion look = Quaternion.LookRotation(targetRot);
                        look.x -= look.x; look.z -= look.z;
                        float angleDifference = Quaternion.Angle(look, rb.rotation);

                        UpdateMoveDirGlobal(targetRot);
                        //CharacterMove(targetRot);
                        //CharacterJump();
                        //CharacterRotateTowards(targetRot);


                        if (IsGrounded && variousTimers[(int)Constants.Timers.AICooldown] > 0)
                        {
                            if (variousTimers[(int)Constants.Timers.AIUniqueAttack] > 0)
                            {
                                moveSpeedModifierPublic = 0.2f;
                            }
                            else if(DistanceToTarget > cValues.ExtraValueList[2].Value)
                            {
                                moveSpeedModifierPublic = 1;
                            }
                            else
                            {
                                moveSpeedModifierPublic = 0.5f;
                            }
                        }

                        if (variousTimers[(int)Constants.Timers.AIUniqueAttack] <= 0 && variousTimers[(int)Constants.Timers.AICooldown] <= 0)
                        {
                            if (DistanceToTarget < cValues.ExtraValueList[2].Value)
                            {
                                bufferTimers[(int)Constants.Inputs.Jump] = sValues.BufferLeniency;
                                moveSpeedModifierPublic = cValues.ExtraValueList[0].Value;
                                variousTimers[(int)Constants.Timers.AIUniqueAttack] = cValues.ExtraValueList[1].Value;
                                variousTimers[(int)Constants.Timers.AICooldown] = cValues.ExtraValueList[1].Value + cValues.ExtraValueList[3].Value;
                                if(target.transform.position.y > transform.position.y + 1)
                                {
                                    variousTimers[(int)Constants.Timers.AIUniqueAttack] /= 2;
                                    variousTimers[(int)Constants.Timers.AICooldown] /= 2;
                                }
                                IsAttacking = true;
                            }
                        }
                    }

                    if (variousTimers[(int)Constants.Timers.AIUniqueAttack] <= 0)
                    {
                        IsAttacking = false;
                    }

                    if (IsStartWithinRange && IsAwake && !IsAggro)
                    {
                        StartCoroutine(Sleep());
                    }
                }

                thisLastFramePos = transform.position;
                targetLastFramePos = target.transform.position;
            }
        }
        else if (!IsDead)
        {
            OnDead();
        }
    }

    void LateUpdate()
    {
        if (IsMounted)
        {
            if (parentObject != null)
            {
                transform.position = new Vector3(parentObject.transform.position.x, parentObject.transform.position.y + transform.localScale.y / 2, parentObject.transform.position.z); ;// Vector3.Slerp(transform.position, parentObject.transform.position, 0.9f);
                transform.rotation = parentObject.transform.rotation;
            }
            else
            {
                AIKnockBack(Vector3.up, 2);
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.CompareTag(Constants.Tags.Player.ToString()))
        {
            collision.gameObject.SendMessage("OnHit", (cValues.HealthAttack, -collision.GetContact(0).normal, collision.relativeVelocity.magnitude));
        }

        if (collision.transform.GetComponent<Rigidbody>() != null && collision.transform.GetComponent<Rigidbody>().velocity.magnitude > 2)
        {
            if (!IsMounted)
            {
                if (childObject != null && collision.transform.gameObject == childObject || collision.transform.CompareTag(Constants.Tags.Player.ToString()))
                {

                }
                else
                {
                    AIKnockBack(collision.GetContact(0).normal, collision.relativeVelocity.magnitude);
                    TakeDamage(1);
                }
            }
            else
            {
                if (parentObject != null)
                {
                    AIKnockBack(collision.GetContact(0).normal, collision.relativeVelocity.magnitude);
                    TakeDamage(2);
                }
            }
        }
    }

    public void OnDead()
    {
        StartCoroutine(Dying());
    }

    public IEnumerator Dying()
    {
        IsDead = true;
        float endY = 2;
        /*float startY = transform.position.y;
        rb.isKinematic = true;

        while (audioSource.isPlaying)
        {
            transform.position += new Vector3(0, (endY) * Time.deltaTime, 0);
            endY *= 0.95f;
            yield return null;
        }

        ParticleSystem particles = GameObject.Instantiate(Resources.Load<ParticleSystem>("Particles/DeathRollingEnemy"), transform.position, Quaternion.identity);
        particles.GetComponent<AudioSource>().PlayOneShot(GetAudio(Constants.CharacterAudioList.DieSfx));*/

        while (audioSource.isPlaying)
        {
            characterModelPosition.eulerAngles += new Vector3((endY) * Time.deltaTime, 0, 0);
            endY *= 0.95f;
            yield return null;
        }
        Destroy(this.gameObject);
    }

    public IEnumerator WakeUp()
    {
        IsAggro = true;

        float timer = 0;
        while(timer <= 1)
        {
            //transform.LookAt(Vector3.Slerp(transform.localEulerAngles, targetLastKnownLocation, 0.4f));
            timer += (1 / aValues.TimeToWakeUp) * Time.deltaTime;
            CharacterRotateTowards(targetLastKnownLocation);
            //variousTimers[(int)Constants.Timers.AIUniqueAttack] = cValues.ExtraValueList[1].Value;

            yield return new WaitForEndOfFrame();
        }
        IsAwake = true;
    }

    public IEnumerator Sleep()
    {
        startRot = GlobalScript.Instance.GetStartRot(transform, characterModelPosition);
        ResetMaxMoveValue();

        bufferTimers[(int)Constants.Inputs.Jump] = sValues.BufferLeniency;
        CharacterJump();

        float timer = 0;
        while (timer <= 1)
        {
            //characterModelPosition.transform.localEulerAngles = Vector3.Lerp(Vector3.zero, startRot, timer);
            timer += (1 / aValues.TimeToSleep) * Time.deltaTime;

            yield return new WaitForEndOfFrame();
        }

        IsAwake = false;
    }
}
