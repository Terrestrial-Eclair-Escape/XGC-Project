using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Deform;

public class JellyWobbler : MonoBehaviour
{
    [SerializeField]
    WaveDeformer waveDeformer;
    [SerializeField]
    float waveLength = 2.5f;
    [SerializeField]
    float steepness = 0.3f;
    [SerializeField]
    float speed = 0.5f;
    [SerializeField]
    float offset = 0;
    // Start is called before the first frame update
    void Start()
    {
        waveDeformer = GetComponent<WaveDeformer>();
        if (waveDeformer != null)
        {
            waveDeformer.WaveLength = waveLength;
            waveDeformer.Steepness = steepness;
            waveDeformer.Speed = speed;
            waveDeformer.Offset = offset;
        }
    }
    // Update is called once per frame
    void Update()
    {
        transform.Rotate(0f,1f,0f, Space.Self); //constant wobbling.
    }

   //todo wobble on event.
   //eg. set steepness to 1 and lerp it back to default.
}
