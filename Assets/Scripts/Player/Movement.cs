using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;


[RequireComponent(typeof(CharacterController))]
public class Movement : MonoBehaviour
{
    [Header("refrences")]
    public CharacterController character;

    [Header("moveVariables")]
    [SerializeField]
    private float walkSpeed, sprintSpeed, crouchSpeed;
    public Vector3 playerMove;
    private Vector2 playerMoveInput;
    public Vector3 moveDir;
    private float moveSpeed;

    [Header("sprinting")]
    public bool isSprinting;

    [Header("crouching")]
    public bool isCrouching = false;
    private bool isBusyCrouching;
    private float crouchHeigt = 0.5f;
    private float standHeigt = 2;
    private float crouchtime = 0.15f;
    private Vector3 crouchCenter = new Vector3(0, 0.5f, 0);
    private Vector3 standCenter = new Vector3(0, 0, 0);


    [Header("jump variables")]
    private Vector3 velcity;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float jumpheight;

    private void Start()
    {
        Cursor.visible = false;
    }


    private void Update()
    {
        //function variables
        playerMove = new Vector3(playerMoveInput.x * moveSpeed, 0, playerMoveInput.y * moveSpeed);
        moveDir = transform.right * playerMove.x + transform.forward * playerMove.z;

        //Checks
        if (isSprinting)
            moveSpeed = sprintSpeed;

        if (isCrouching)
            moveSpeed = crouchSpeed;

        if (!isCrouching && !isSprinting)
        {
            moveSpeed = walkSpeed;
        }


        //movement for the player
        character.Move(moveDir * Time.deltaTime);


        // gravity so the player falls
        velcity.y += gravity * Time.deltaTime;
        character.Move(velcity * Time.deltaTime);

        //makes sure the player his y velocity stopps
        if (character.isGrounded && velcity.y < 0)
        {
            velcity.y = -2;
        }
    }

    public void Walking(InputAction.CallbackContext context)
    {
        playerMoveInput = context.ReadValue<Vector2>();

    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (character.isGrounded && context.started)
        {
            velcity.y = Mathf.Sqrt(jumpheight * -2 * gravity);
        }
    }

    public void Sprint(InputAction.CallbackContext context)
    {
        isSprinting = context.ReadValueAsButton();
    }

    public void Crouch(InputAction.CallbackContext context)
    {
        isCrouching = context.ReadValueAsButton();

        StartCoroutine(CrouchMode());
    }

    IEnumerator CrouchMode()
    {
        isBusyCrouching = true;
        float timeElapsed = 0;

        float targetHeight = isCrouching ? crouchHeigt : standHeigt;
        float currentHeigt = character.height;

        Vector3 targetCenter = isCrouching ? crouchCenter : standCenter;
        Vector3 currnetCenter = character.center;

        while (timeElapsed < crouchtime)
        {
            character.height = Mathf.Lerp(currentHeigt, targetHeight, timeElapsed / crouchtime);
            character.center = Vector3.Lerp(currnetCenter, targetCenter, timeElapsed / crouchtime);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        character.height = targetHeight;
        character.center = targetCenter;

        isBusyCrouching = false;
    }
}
