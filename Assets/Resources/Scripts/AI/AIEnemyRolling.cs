using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using Unity.VisualScripting;
using UnityEngine;

public class AIEnemyRolling : BaseAI, CharacterInterface, AIInterface
{
    Vector3 startRot;
    Vector3 lastPlacedPos;

    // Start is called before the first frame update
    void Start()
    {
        AIStart();
        if (!IsTargetWithinRange)
        {
            startRot = GlobalScript.Instance.GetStartRot(transform, characterModelPosition);
            characterModelPosition.transform.localEulerAngles = startRot;
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
                RotateCookie();

                if (IsTargetWithinRange && !IsAggro && !IsAwake)
                {
                    StartCoroutine(WakeUp());
                }

                if (IsAggro && IsAwake && (IsTargetWithinRange || variousTimers[(int)Constants.Timers.Searching] > 0))
                {
                    Vector3 targetPos = new Vector3(targetLastKnownLocation.x, transform.position.y, targetLastKnownLocation.z);
                    Vector3 targetRot = (targetPos - transform.position).normalized;
                    Quaternion look = Quaternion.LookRotation(targetRot);
                    look.x -= look.x; look.z -= look.z;
                    float angleDifference = Quaternion.Angle(look, rb.rotation);

                    if (angleDifference < cValues.ExtraValueList[0].Value)
                    {
                        if (variousTimers[(int)Constants.Timers.AIUniqueAttack] <= 0)
                        {
                            CharacterMove(targetRot);
                        }
                        else
                        {
                            CharacterRotateTowards(targetRot);
                        }
                    }
                    else
                    {
                        variousTimers[(int)Constants.Timers.AIUniqueAttack] = cValues.ExtraValueList[1].Value;
                        CharacterMove(Vector3.zero);
                        CharacterRotateTowards(targetRot);
                    }
                }

                if (IsStartWithinRange && IsAwake && !IsAggro)
                {
                    StartCoroutine(Sleep());
                }
            }
        }
        else if (!IsDead)
        {
            OnDead();
        }
    }

    private void RotateCookie()
    {
        float dist = Vector3.Distance(transform.position, lastPlacedPos);
        if (IsAwake)
        {

            characterModelPosition.transform.GetChild(0).localEulerAngles += new Vector3(Mathf.Abs(cValues.ExtraValueList[2].Value * dist), 0, 0);
            characterModelPosition.transform.GetChild(0).localEulerAngles = new Vector3(characterModelPosition.transform.GetChild(0).localEulerAngles.x, 0, 90);

        }
        lastPlacedPos = transform.position;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.CompareTag(Constants.Tags.Player.ToString()))
        {
            collision.gameObject.SendMessage("TakeDamage", cValues.HealthAttack);
        }

        if(collision.transform.GetComponent<Rigidbody>() != null && collision.transform.GetComponent<Rigidbody>().velocity.magnitude > 2)
        {
            if (collision.transform.CompareTag(Constants.Tags.Player.ToString()))
            {

            }
            else
            {
                TakeDamage(1);
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
        float startY = transform.position.y;
        float endY = 2;
        rb.isKinematic = true;

        while (transform.position.y < startY + endY)
        {
            transform.position += new Vector3(0, (endY) * Time.deltaTime, 0);
            endY /= 2;
            yield return null;
        }

        GameObject.Instantiate(Resources.Load<ParticleSystem>("Particles/DeathRollingEnemy"), transform.position, Quaternion.identity);

        Destroy(this.gameObject);
    }

    public IEnumerator WakeUp()
    {
        IsAggro = true;

        float timer = 0;
        while(timer <= 1)
        {
            characterModelPosition.transform.localEulerAngles = Vector3.Lerp(startRot, Vector3.zero, timer);
            timer += (1 / aValues.TimeToWakeUp) * Time.deltaTime;
            variousTimers[(int)Constants.Timers.AIUniqueAttack] = cValues.ExtraValueList[1].Value;

            yield return new WaitForEndOfFrame();
        }
        IsAwake = true;
    }

    public IEnumerator Sleep()
    {
        startRot = GlobalScript.Instance.GetStartRot(transform, characterModelPosition);
        ResetMaxMoveValue();

        float timer = 0;
        while (timer <= 1)
        {
            characterModelPosition.transform.localEulerAngles = Vector3.Lerp(Vector3.zero, startRot, timer);
            timer += (1 / aValues.TimeToSleep) * Time.deltaTime;

            yield return new WaitForEndOfFrame();
        }

        IsAwake = false;
    }
}
