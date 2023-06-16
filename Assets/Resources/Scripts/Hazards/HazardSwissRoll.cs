using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HazardSwissRoll : MonoBehaviour
{
    public Transform startPosObj;
    public Transform endPosObj;
    public Rigidbody rb;
    public float timeFromStartToEnd;

    private Vector3 startPos;
    private Vector3 endPos;
    private bool towardsEnd = true;
    float dist;
    private float timer;

    // Start is called before the first frame update
    void Start()
    {
        startPos = startPosObj.position;
        endPos = endPosObj.position;

        RaycastHit hit;
        if(Physics.Raycast(startPos, Vector3.down, out hit))
        {
            startPos = new Vector3(startPos.x, hit.point.y + transform.localScale.x * GetComponent<CapsuleCollider>().radius, startPos.z);
        }
        if (Physics.Raycast(endPos, Vector3.down, out hit))
        {
            endPos = new Vector3(endPos.x, hit.point.y + transform.localScale.x * GetComponent<CapsuleCollider>().radius, endPos.z);
        }

        dist = Vector3.Distance(startPos, endPos);

        transform.position = startPos;
        transform.up = (endPos - startPos);
        transform.eulerAngles += new Vector3(0, 90, 0);

        Destroy(startPosObj.gameObject);
        Destroy(endPosObj.gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        timer += (towardsEnd) ? GetSpeed() : -GetSpeed();

        if (timer >= 180)
        {
            timer = 180;
            towardsEnd = false;
        }
        else if(timer <= 0)
        {
            timer = 0;
            towardsEnd = true;
        }

        rb.MovePosition(Vector3.Lerp(startPos, endPos, (Mathf.Cos(timer) + 1f) / 2f));
    }

    float GetSpeed()
    {
        if(timeFromStartToEnd == 0)
        {
            if(timer == 180)
            {
                return 0;
            }

            return 180;
        }

        return (dist / timeFromStartToEnd) * Time.deltaTime;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.CompareTag(Constants.Tags.Player.ToString()))
        {
            collision.gameObject.SendMessage("TakeDamage", 1);
        }
    }
}
