using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/CameraValues", order = 1)]
public class CameraValues : ScriptableObject
{
    [Tooltip("Sensitivity on the horizontal axis.")]
    public float SensitivityHorizontal = 6;
    [Tooltip("Sensitivity on the vertical axis.")]
    public float SensitivityVertical = 4;

    [Tooltip("The height at which the camera is placed.")]
    public float CameraHeight = 1.5f;

    [Tooltip("Camera field of view")]
    public int CameraFOV = 60;

    [Tooltip("Position slerp."), Range(0, 1)]
    public float SlerpCameraPosition = 0.4f;
    [Tooltip("Position slerp."), Range(0, 1)]
    public float SlerpParentPosition = 0.4f;
    [Tooltip("Jump slerp."), Range(0, 1)]
    public float SlerpParentJump = 0.1f;

    [Tooltip("Max angle. The limit at how high the camera can go.")]
    public int AngleMax = 60;
    [Tooltip("Min angle. The limit at how low the camera can go.")]
    public int AngleMin = -40;

    [Tooltip("Distance farthest away from camera.")]
    public float DistMax = -15;
    [Tooltip("Distance closest to camera.")]
    public float DistMin = -7;
}
