using Palmmedia.ReportGenerator.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Script for player characters. Uses BaseCharacterMovement's functions for movement in conjunction with player inputs.
/// </summary>
public class CharacterScript : BaseCharacterMovement
{
    private PlayerInputActions playerActions;

    private InputAction inputMove;
    private InputAction inputJump;

    private void Awake()
    {
        playerActions = new PlayerInputActions();
        coyoteTimer = GlobalScript.Instance.GenerateInputList();
        bufferTimer = GlobalScript.Instance.GenerateInputList();
    }

    // Start is called before the first frame update
    private void Start()
    {
    }

    private void FixedUpdate()
    {
        BufferUpdate();
    }

    // Update is called once per frame
    void Update()
    {
        CharacterMove(InputMove);
        CharacterJump();
    }

    private void LateUpdate()
    {
        CharacterOnAirborne();
    }

    private void OnEnable()
    {
        inputMove = playerActions.Player.Move;
        inputMove.Enable();

        inputJump = playerActions.Player.Jump;
        inputJump.Enable();
        inputJump.performed += InputJump;
    }

    private void OnDisable()
    {
        inputMove.Disable();
        inputJump.Disable();
    }

    private void InputJump(InputAction.CallbackContext context)
    {
        bufferTimer[(int)Constants.Inputs.Jump] = sValues.BufferLeniency;
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
}
