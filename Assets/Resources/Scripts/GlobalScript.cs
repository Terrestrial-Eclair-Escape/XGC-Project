using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Global script, can be used for globally accessible functions and changing values. 
/// <br/><br/>
/// Access it from any script by writing GlobalScript.Instance
/// </summary>
public class GlobalScript
{
    private static GlobalScript _instance;
    public static GlobalScript Instance => (_instance == null) ? new GlobalScript() : _instance;

    private void Awake()
    {
        _instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

    }

    public float[] GenerateInputList()
    {
        return new float[Enum.GetValues(typeof(Constants.Inputs)).Length];
    }

    public float[] GenerateTimerList()
    {
        return new float[Enum.GetValues(typeof(Constants.Timers)).Length];
    }

    public float AngleDir(Vector3 fwd, Vector3 targetDir, Vector3 up)
    {
        Vector3 perp = Vector3.Cross(fwd, targetDir);
        float dir = Vector3.Dot(perp, up);

        if (dir > 0.0f)
        {
            return 1.0f;
        }
        else if (dir < 0.0f)
        {
            return -1.0f;
        }
        else
        {
            return 0.0f;
        }
    }
}
