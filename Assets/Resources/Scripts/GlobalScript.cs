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

    public List<float> GenerateInputList()
    {
        List<float> toReturn = new List<float>();
        foreach (var x in Enum.GetNames(typeof(Constants.Inputs)))
        {
            toReturn.Add(0);
        }
        return toReturn;
    }
}
