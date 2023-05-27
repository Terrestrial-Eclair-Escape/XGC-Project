using UnityEngine;
using UnityEngine.InputSystem;

public class BaseCamera : MonoBehaviour
{
    public CameraValues cValues;
    public Transform cam;
    public Transform camActual;
    public GameObject player;

    private PlayerInputActions playerActions;

    private InputAction inputLook;

    private Vector3 camStartPos;
    [SerializeField] bool useRay;

    private void Awake()
    {
        playerActions = new PlayerInputActions();
    }

    private void OnEnable()
    {
        inputLook = playerActions.Player.Look;
        inputLook.Enable();
    }

    private void OnDisable()
    {
        inputLook.Disable();
    }

    // Use this for initialization
    void Start()
    {
        camActual.SetParent(cam);
        camActual.localPosition = transform.forward;

        transform.position = player.transform.position;

        camStartPos = new Vector3(0, cValues.CameraHeight, 0);
        cam.localPosition = camStartPos;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        ParentPosition();
        CameraPosition();
    }

    private void LateUpdate()
    {
        ParentRotation();
        CameraFOV();
    }

    /// <summary>
    /// Set rotation of parent object (around player object).
    /// </summary>
    void ParentRotation()
    {
        Vector2 input = inputLook.ReadValue<Vector2>();

        // clamp camera rotation
        transform.eulerAngles += new Vector3(-input.y * cValues.SensitivityVertical, input.x * cValues.SensitivityHorizontal, 0);
        transform.localRotation = Quaternion.Slerp(transform.localRotation, Quaternion.Euler(ClampAngle(transform.eulerAngles.x, cValues.AngleMin, cValues.AngleMax), transform.eulerAngles.y, 0), 0.5f);
    }

    // Set position of parent object (at player position).
    void ParentPosition()
    {
        Vector3 newCamPos = player.transform.position;
        newCamPos = Vector3.Lerp(transform.position, player.transform.position, cValues.SlerpParentPosition);
        newCamPos.y = Vector3.Slerp(transform.position, player.transform.position, cValues.SlerpParentJump).y;

        transform.position = newCamPos;
    }

    /// <summary>
    /// Set camera position (distance from player).
    /// </summary>
    void CameraPosition()
    {
        // makes camera zoom in when lower and zoom out when above the player
        float theX = (transform.eulerAngles.x >= 180) ? transform.eulerAngles.x - 360 : transform.eulerAngles.x;
        Vector3 slerpVector = Vector3.Slerp(new Vector3(0, cValues.CameraHeight, cValues.DistMin), new Vector3(0, cValues.CameraHeight, cValues.DistMax), (theX + Mathf.Abs(cValues.AngleMin)) / (cValues.AngleMax + Mathf.Abs(cValues.AngleMin)));
        float dist = Vector3.Distance(player.transform.position, transform.TransformPoint(slerpVector));
        Debug.DrawRay(player.transform.position, -(player.transform.position - transform.TransformPoint(slerpVector)), Color.green);

        // keep camera from going into walls
        if (useRay)
        {
            RaycastHit hit;
            if (Physics.Raycast(player.transform.position, -(player.transform.position - transform.TransformPoint(slerpVector)), out hit, dist, 0 << (int)(Constants.Layers.IgnoreRaycast)))
            {
                // smooth
                //cam.position = Vector3.Slerp(cam.position, hit.point, cValues.SlerpParentPosition);

                // instant
                cam.position = hit.point;
            }
            else
            {
                cam.localPosition = slerpVector;
            }
        }
        else
        {
            cam.localPosition = slerpVector;
        }
    }

    void CameraFOV()
    {
        camActual.GetComponent<Camera>().fieldOfView = cValues.CameraFOV;
    }

    float ClampAngle(float angle, float min, float max)
    {
        if (angle > 180) { angle -= 360; }
        else if (angle < -180) { angle += 360; }
        return Mathf.Clamp(angle, min, max);
    }
}
