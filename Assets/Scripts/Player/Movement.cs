using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class Movement : MonoBehaviour
{
    private CharacterController character;

    [Header("moveVariables")]
    private Vector2 playerMoveInput;
    [SerializeField] private float moveSpeed;

    [Header("jump variables")]
    private Vector3 velcity;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float jumpheight;

    [Header("checks")]
    public Transform feet;
    public LayerMask groundMask;
    private bool isGrounded;

    private void Start()
    {
        character = GetComponent<CharacterController>();
    }

    
    private void Update()
    {
        //function variables
        Vector3 playerMove = new Vector3(playerMoveInput.x * moveSpeed, 0, playerMoveInput.y * moveSpeed);
        Vector3 moveDir = transform.right * playerMove.x + transform.forward * playerMove.z;
        //checks if the player is standing on ground or jumpables
        isGrounded = Physics.CheckSphere(feet.position, 0.4f, groundMask);
        //movement for the player
        character.Move(moveDir * Time.deltaTime);

        // gravity so the player falls
        velcity.y += gravity * Time.deltaTime; 
        character.Move(velcity * Time.deltaTime);

        //makes sure the player his y velocity stopps
        if (isGrounded && velcity.y < 0)
        {
            velcity.y = -2;
        }
    }

    public void PlayerMovementInput(InputAction.CallbackContext conetext)
    {
        playerMoveInput = conetext.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext conetext)
    {
        if (isGrounded)
        {
            velcity.y = Mathf.Sqrt(jumpheight * -2 * gravity);
        }
    }
}
