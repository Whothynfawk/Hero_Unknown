using UnityEngine;
using UnityEngine.InputSystem;


public class GroundMovement : MonoBehaviour
{
    private CharacterController body;
    [SerializeField] private float gravity = 9.81f;
    private Vector3 direction;
    private float verticalVelocity;
    private float targetHeight;
    private float standardHeight = 2f;

    [SerializeField] private float crouchSpeedMultiplier = 0.5f;
    [SerializeField] private float sprintSpeedMultiplier = 1.5f;
    [SerializeField] private float uncrouchSpeed = 4f;
    [SerializeField] private float speed = 5f;
    [SerializeField] private float jumpHeight = 2f;

    private void Awake()
    {
        body = GetComponent<CharacterController>();
        targetHeight = standardHeight;

    }

    public void FixedUpdate()
    {
        if (body.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = 0f;
        }

        verticalVelocity -= gravity * Time.fixedDeltaTime;
        Vector3 movement = direction * speed;
        movement.y = verticalVelocity;
        body.Move(movement * Time.fixedDeltaTime);

        body.height = Mathf.Lerp(body.height, targetHeight, uncrouchSpeed * Time.fixedDeltaTime);
    }

    public void OnMoveInput(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Vector2 input = context.ReadValue<Vector2>();
            direction = new Vector3(input.x, 0, input.y);
        }
        else if (context.canceled)
        {
            direction = Vector3.zero;
        }
    }

    public void OnJumpInput(InputAction.CallbackContext context)
    {
        if (context.started && body.isGrounded)
        {
            verticalVelocity = Mathf.Sqrt(jumpHeight * 2f * gravity);
        }
    }

    public void OnSprintInput(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            speed *= sprintSpeedMultiplier;
        }
        else if (context.canceled)
        {
            speed /= sprintSpeedMultiplier;
        }
    }

    public void OnCrouchInput(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            speed *= crouchSpeedMultiplier;
            targetHeight = standardHeight / 2f;
        }
        else if (context.canceled)
        {
            speed /= crouchSpeedMultiplier;
            targetHeight = standardHeight;
        }
    }
}
