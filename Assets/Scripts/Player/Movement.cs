using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;


[RequireComponent(typeof(CharacterController))]
public class Movement : MonoBehaviour
{
    [Header("refrences")]
    public CharacterController character;

    [Header("moveVariables")]
    [SerializeField] private float walkSpeed;
    [HideInInspector] public Vector3 moveDir;
    private Vector2 playerMoveInput;
    private float moveSpeed;

    [Header("sprinting")]
    [HideInInspector] public bool isSprinting;
    [SerializeField] private float sprintSpeed;

   [Header("crouching")]
    [HideInInspector] public bool isCrouching = false;
    [SerializeField] private float crouchSpeed;
    private bool isBusyCrouching;
    private float crouchHeigt = 0.5f;
    private float standHeigt = 2;
    private float crouchtime = 0.15f;
    private Vector3 crouchCenter = new Vector3(0, 0.5f, 0);
    private Vector3 standCenter = new Vector3(0, 0, 0);

    [Header("jump variables")]
    [SerializeField] private float gravity;
    [SerializeField] private float jumpheight;
    private Vector3 velcity;

    [Header("sliding")]
    [SerializeField] private float  slideSlopeSpeed;
    [SerializeField] float slopSlowMulitplier;
    private Vector3 hitpointnormal;
    private float slopeAngle;

    private void Start()
    {
        Cursor.visible = false;
    }


    private void Update()
    {
        Vector2 currentInput = new Vector2(playerMoveInput.x * moveSpeed, playerMoveInput.y * moveSpeed);

        float movedirY = moveDir.y;
        moveDir = (transform.TransformDirection(Vector3.forward) * currentInput.y) + (transform.TransformDirection(Vector3.right) * currentInput.x);
        moveDir.y = movedirY;

        if (character.isGrounded && Physics.Raycast(transform.position, Vector3.down, out RaycastHit slopeHit, 2f))
        {
            hitpointnormal = slopeHit.normal;
            slopeAngle = Vector3.Angle(hitpointnormal, Vector3.up);


            if (slopeAngle <= character.slopeLimit)
            {
                float slowdownMultiCalculator = (character.slopeLimit - slopeAngle) / character.slopeLimit;
                float slowfactor = Mathf.Lerp(slopSlowMulitplier, 1,slowdownMultiCalculator);
                moveDir.x *= slowfactor;
                moveDir.z *= slowfactor;
            }
            else
            {
                moveDir = Vector3.zero;
                moveDir += new Vector3(hitpointnormal.x, -hitpointnormal.y, hitpointnormal.z) * slideSlopeSpeed;
            }
        }
        //Checks
        if (!character.isGrounded)
            moveDir.y -= gravity * Time.deltaTime;

        if (isSprinting)
            moveSpeed = sprintSpeed;

        if (isCrouching)
            moveSpeed = crouchSpeed;

        if (!isCrouching && !isSprinting)
            moveSpeed = walkSpeed;

        //movement for the player
        character.Move(moveDir * Time.deltaTime);
    }

    public void Walking(InputAction.CallbackContext context)
    {
        playerMoveInput = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (character.isGrounded && context.started)
        {
            moveDir.y = jumpheight;
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
