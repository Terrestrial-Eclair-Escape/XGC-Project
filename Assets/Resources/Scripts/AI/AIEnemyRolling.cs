using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using Unity.VisualScripting;
using UnityEngine;

public class AIEnemyRolling : BaseCharacterMovement, CharacterInterface
{
    GameObject target;

    // Start is called before the first frame update
    void Start()
    {
        CharacterStart();
    }

    // Update is called once per frame
    void LateUpdate()
    {

        CharacterFallOffRespawnDebug();

        if (target == null)
        {
            target = GameObject.FindGameObjectWithTag(Constants.Tags.Player.ToString());
        }
        else
        {
            Vector3 targetPos = new Vector3(target.transform.position.x, transform.position.y, target.transform.position.z);
            Vector3 targetRot = (targetPos - transform.position).normalized;
            Quaternion look = Quaternion.LookRotation(targetRot);
            look.x -= look.x; look.z -= look.z;
            float angleDifference = Quaternion.Angle(look, rb.rotation);

            if (angleDifference < 20)
            {
                CharacterMove(targetRot);
            }
            else
            {
                CharacterMove(Vector3.zero);
                CharacterRotateTowards(targetRot);
                // rb.MoveRotation(Quaternion.Slerp(rb.rotation, look, cValues.MoveTurnValue));
            }
        }

        if (IsDead)
        {
            OnDead();
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

    public void OnDead()
    {
        Destroy(this.gameObject);
    }
}
