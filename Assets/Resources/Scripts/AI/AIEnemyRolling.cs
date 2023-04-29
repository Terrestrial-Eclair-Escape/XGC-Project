using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using Unity.VisualScripting;
using UnityEngine;

public class AIEnemyRolling : BaseAI, CharacterInterface, AIInterface
{
    Vector3 startRot;

    // Start is called before the first frame update
    void Start()
    {
        AIStart();
        if (!IsTargetWithinRange)
        {
            UpdateStartRot();
            characterModel.transform.localEulerAngles = startRot;
        }
    }

    void FixedUpdate()
    {
        CharacterFixedUpdate();
    }

    // Update is called once per frame
    void Update()
    {
        AIUpdate();

        if (target != null)
        {
            RotateCookie();

            if(IsTargetWithinRange && !IsAggro && !IsAwake)
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

        if (IsDead)
        {
            OnDead();
        }
    }

    private void RotateCookie()
    {
        if (IsAwake)
        {
            characterModel.transform.GetChild(0).localEulerAngles += new Vector3(Mathf.Abs(cValues.ExtraValueList[2].Value * rb.velocity.magnitude), 0, 0);
            characterModel.transform.GetChild(0).localEulerAngles = new Vector3(characterModel.transform.GetChild(0).localEulerAngles.x, 0, 90);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.CompareTag(Constants.Tags.Player.ToString()))
        {
            collision.gameObject.SendMessage("TakeDamage", cValues.HealthAttack);
        }

        if(collision.transform.GetComponent<Rigidbody>() != null && collision.transform.GetComponent<Rigidbody>().velocity.magnitude > 2)
        {
            TakeDamage(1);
        }
    }

    void UpdateStartRot()
    {
        Transform tf = characterModel.transform;
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit))
        {
            tf.up = hit.normal;
            tf.eulerAngles = new Vector3(tf.eulerAngles.x, tf.eulerAngles.y, tf.eulerAngles.z + 90);
        }
        startRot = tf.localEulerAngles;
    }

    public void OnDead()
    {
        Destroy(this.gameObject);
    }

    public IEnumerator WakeUp()
    {
        Debug.Log("awake " + startRot);

        IsAggro = true;

        float timer = 0;
        while(timer <= 1)
        {
            characterModel.transform.localEulerAngles = Vector3.Lerp(startRot, Vector3.zero, timer);
            timer += (1 / aValues.TimeToWakeUp) * Time.deltaTime;
            variousTimers[(int)Constants.Timers.AIUniqueAttack] = cValues.ExtraValueList[1].Value;

            yield return new WaitForEndOfFrame();
        }
        IsAwake = true;
    }

    public IEnumerator Sleep()
    {
        Debug.Log("asleep " + startRot);
        UpdateStartRot();
        ResetMaxMoveValue();

        float timer = 0;
        while (timer <= 1)
        {
            characterModel.transform.localEulerAngles = Vector3.Lerp(Vector3.zero, startRot, timer);
            timer += (1 / aValues.TimeToSleep) * Time.deltaTime;

            yield return new WaitForEndOfFrame();
        }

        IsAwake = false;
    }

    public IEnumerator Dying()
    {
        yield return null;
    }
}
