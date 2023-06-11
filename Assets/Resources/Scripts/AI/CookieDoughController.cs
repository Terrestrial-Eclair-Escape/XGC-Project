using UnityEngine;

public class CookieDoughController: MonoBehaviour
{
    public float knockbackForce = 10f; // the force applied to the player when knocked back
    public float jumpForce = 10f; // the force applied to the enemy when jumping
    public float jumpInterval = 3f; // the time between each jump
    public float jumpRadius = 5f; // the radius around the player within which the enemy will jump towards them
    public float bounceForce = 10f;
    public Vector3 jumpModifier = new Vector3(0, 1, 0); // modifies jump vector

    private Rigidbody rb;
    private GameObject player;
    private float timeSinceLastJump;

    void Start()
    {
        rb = GetComponent<Rigidbody>(); 
        player = GameObject.FindGameObjectWithTag("Player");
    }

    void Update()
    {
        timeSinceLastJump += Time.deltaTime;
        if (timeSinceLastJump >= jumpInterval && Vector3.Distance(transform.position, player.transform.position) <= jumpRadius)
        {
            if (!IsCollidingWithPlayer())
            {
                JumpTowardsPlayer();
                timeSinceLastJump = 0f;
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag(Constants.Tags.Player.ToString()))
        {
            Vector3 contactNormal = collision.contacts[0].normal;
            float angle = Vector3.Angle(Vector3.up, contactNormal);

            //Debug.Log("angle is: " + angle);
            if (angle >= 180f)
            {
                BouncePlayer(collision.gameObject);
                //Debug.Log("Bounce");
            }
            else
            {
                KnockbackPlayer(collision.gameObject);
            }

            timeSinceLastJump = 0f;
        }
    }

    Vector3 PlayerDirection(){
        Vector3 direction = player.transform.position - transform.position;
        direction.y = 0f;
        direction = direction.normalized;
        return direction;
    }
    void JumpTowardsPlayer()
    {
        rb.AddForce(PlayerDirection() * jumpForce + jumpModifier, ForceMode.Impulse);
    }

    void KnockbackPlayer(GameObject player)
    {
        player.GetComponent<Rigidbody>().AddForce(PlayerDirection() * knockbackForce, ForceMode.Impulse);
    }

    void BouncePlayer(GameObject player)
    {
        Vector3 bounceDirection = Vector3.up;
        player.GetComponent<Rigidbody>().AddForce(bounceDirection * bounceForce, ForceMode.Impulse);
    }

    bool IsCollidingWithPlayer()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, 1f); // adjust the sphere radius as needed
        foreach (Collider collider in colliders)
        {
            if (collider.gameObject.CompareTag("Player"))
            {
                return true;
            }
        }
        return false;
    }
}
