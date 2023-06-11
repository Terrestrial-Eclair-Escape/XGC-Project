using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiningPlatform : MonoBehaviour
{
    [SerializeField]
    float _spinningSpeed = 15;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        transform.Rotate(0f, _spinningSpeed * Time.deltaTime, 0f, Space.Self);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(Constants.Tags.Player.ToString()) || other.CompareTag(Constants.Tags.Enemy.ToString()))
        {
            other.transform.SetParent(transform);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(Constants.Tags.Player.ToString()) || other.CompareTag(Constants.Tags.Enemy.ToString()))
        {
            other.transform.SetParent(null);
        }
    }
}
