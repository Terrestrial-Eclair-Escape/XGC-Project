//using Palmmedia.ReportGenerator.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/// <summary>
/// Script for player characters. Uses BaseCharacterMovement's functions for movement in conjunction with player inputs.
/// </summary>
public class CharacterScript : BaseCharacterMovement, CharacterInterface
{
    private PlayerInputActions playerActions;

    private InputAction inputMove;
    private InputAction inputJump;
    private InputAction inputFire;
    private InputAction inputInteract;

    public GameObject lastFramePosObject;
    private Queue<Vector3> lastFramePos = new Queue<Vector3>();
    public int QueueCount = 5;

    void UIUpdate()
    {
        Omni?.UIHealthUpdate(cValues.HealthMax, healthCurrent);
        Omni?.DisplayReticle(pickedUpObject != null);
    }

    private void Awake()
    {
        playerActions = new PlayerInputActions();
    }

    // Start is called before the first frame update
    private void Start()
    {
        CharacterStart();
        Omni?.UIHealthUpdate(cValues.HealthMax, healthCurrent);
    }

    private void FixedUpdate()
    {
        if (!HasDied)
        {
            CharacterFixedUpdate();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!HasDied)
        {
            if(lastFramePos.Count > QueueCount)
            {
                lastFramePosObject.transform.position = lastFramePos.Dequeue();
            }

            CharacterUpdate(
                InputMove,
                Camera.main.ViewportPointToRay(Constants.HalfVector),
                true
            );

            lastFramePos.Enqueue(transform.position);
        }
        else if (!IsDead)
        {
            OnDead();
        }

        UIUpdate();
    }

    private void LateUpdate()
    {
        CharacterLateUpdate();
    }

    private void OnEnable()
    {
        inputMove = playerActions.Player.Move;
        inputMove.Enable();

        inputJump = playerActions.Player.Jump;
        inputJump.Enable();
        inputJump.performed += InputJump;

        inputFire = playerActions.Player.Fire;
        inputFire.Enable();
        inputFire.performed += InputFire;

        inputInteract = playerActions.Player.Interact;
        inputInteract.Enable();
        inputInteract.performed += InputInteract;
    }

    private void OnDisable()
    {
        inputMove.Disable();
        inputJump.Disable();
        inputFire.Disable();
        inputInteract.Disable();
    }

    private void InputJump(InputAction.CallbackContext context)
    {
        bufferTimers[(int)Constants.Inputs.Jump] = sValues.BufferLeniency;
    }

    private void InputFire(InputAction.CallbackContext context)
    {
        bufferTimers[(int)Constants.Inputs.Fire] = sValues.BufferLeniency;
    }

    private void InputInteract(InputAction.CallbackContext context)
    {
        bufferTimers[(int)Constants.Inputs.Interact] = sValues.BufferLeniency;
    }

    /// <summary>
    /// Move input relative to camera rotation
    /// </summary>
    public Vector3 InputMove
    {
        get
        {
            Vector2 mInput = inputMove.ReadValue<Vector2>();
            Vector3 cFwd = Camera.main.transform.forward;
            Vector3 cRight = Camera.main.transform.right;
            cFwd.y = 0; cRight.y = 0;
            cFwd.Normalize(); cRight.Normalize();
            return cFwd * mInput.y + cRight * mInput.x;
        }
    }

    public void OnDead()
    {
        StartCoroutine(Dying());
    }

    public IEnumerator Dying()
    {
        IsDead = true;
        Debug.Log($"{transform.name} DEAD");
        yield return null;
    }
}
