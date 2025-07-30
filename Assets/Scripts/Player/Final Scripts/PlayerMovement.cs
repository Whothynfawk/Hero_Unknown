using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;


[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("refrences")]
    public CharacterController controller;
    [SerializeField] private StateHandler stateHandler;
    [SerializeField] private PlayerWallRun wallRun;

    [Header("MoveSpeed")]
    private float currentSpeed;
    [SerializeField] private float groundWalkSpeed;
    [SerializeField] private float groundSprintSpeed;
    [SerializeField] private float crouchSpeed;

    [Header("Unility and calculations")]
    public float gravity;
    private float normalGravity = -9.81f;
    public bool isGrounded;
    [HideInInspector] public Vector3 velocity;
    [HideInInspector] public Vector3 move;
    [HideInInspector] public Vector2 input;

    [Header("checks and states")]
    [HideInInspector] public bool isSprinting;
    [HideInInspector] public bool isCrouching;

    //crouch variables
    private float crouchHeigt = 0.5f;
    private float standHeigt = 2;
    private float crouchtime = 0.15f;
    private Vector3 crouchCenter = new Vector3(0, 0.5f, 0);
    private Vector3 standCenter = new Vector3(0, 0, 0);

    [Header("jump")]
    public float jumpPower;

    private void Update()
    {
        BasicMovement();
        StateChecks();
    }

    private void BasicMovement()
    {
        //Checks for crouch sprint ect
        if (isGrounded)
        {
            if (!isSprinting && !isCrouching)
                currentSpeed = groundWalkSpeed;
            else if (isSprinting && !isCrouching)
                currentSpeed = groundSprintSpeed;
            else if (!isSprinting && isCrouching)
                currentSpeed = crouchSpeed;
        }

        //grvaity
        if (isGrounded && velocity.y < 0)
            velocity.y = -2f;

        //for moving
        move = new Vector3(input.x, 0, input.y);
        move = transform.right * input.x + transform.forward * input.y;
        controller.Move(move * currentSpeed * Time.deltaTime);

        //Gravity
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    private void StateChecks()
    {
        isGrounded = controller.isGrounded;

        if (stateHandler.state == MovementStates.groundMovement)
        {
            gravity = normalGravity;
        }
        else if (stateHandler.state == MovementStates.wallRun)
        {
            gravity = wallRun.wallgravity;
            velocity.y = 0f;
        }
    }

    public void OnWalkInput(InputAction.CallbackContext context)
    {
        input = context.ReadValue<Vector2>();
    }

    public void OnSprintInput(InputAction.CallbackContext context)
    {
        isSprinting = context.ReadValueAsButton();
    }

    public void OnJumpInput(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            if (isGrounded)
                velocity.y = jumpPower;
        }
    }

    public void OnCrouchInput(InputAction.CallbackContext context)
    {
        isCrouching = context.ReadValueAsButton();
        CrouchMode();
    }

    //Crouch Methods
    IEnumerator CrouchMode()
    {
        float timeElapsed = 0;

        float targetHeight = isCrouching ? crouchHeigt : standHeigt;
        float currentHeigt = controller.height;

        Vector3 targetCenter = isCrouching ? crouchCenter : standCenter;
        Vector3 currnetCenter = controller.center;

        while (timeElapsed < crouchtime)
        {
            controller.height = Mathf.Lerp(currentHeigt, targetHeight, timeElapsed / crouchtime);
            controller.center = Vector3.Lerp(currnetCenter, targetCenter, timeElapsed / crouchtime);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        controller.height = targetHeight;
        controller.center = targetCenter;
    }
}