using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;


public enum MoveStates
{
    freeze,
    wallrun,
    ground,
    unlimited
};

[RequireComponent(typeof(CharacterController))]

public class Movement : MonoBehaviour
{
    [Header("refrences")]
    public CharacterController character;
    public WallRun wallRun;
    public CliffAndLedgeMovement cliffAndLedgeMovement;

    [Header("moveVariables")]
    [SerializeField] private float walkSpeed;
    [HideInInspector] public Vector3 moveDir;
    [HideInInspector] public Vector2 playerMoveInput;
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
    public float gravity;
    [SerializeField] private float jumpheight;
    private Vector3 velcity;

    [Header("sliding")]
    [SerializeField] private float slideSlopeSpeed;
    [SerializeField] float slopSlowMulitplier;
    private Vector3 hitpointnormal;
    private float slopeAngle;

    //WallrunBool
    [HideInInspector] public bool isRunningOnWall;

    //Restrictions
    [HideInInspector] public bool isRestriced;
    //stateMachine
    public MoveStates moveStates;

    private void Start()
    {
        Cursor.visible = false;
        moveStates = MoveStates.ground;
    }


    private void Update()
    {
        MoveInputs();
        States();
    }

    private void States()
    {
        switch (moveStates)
        {
            case MoveStates.freeze:
                moveDir = Vector3.zero;
                break;
            case MoveStates.unlimited:
                moveSpeed = 999f;
                break;
        }
    }

    private void MoveInputs()
    {
        if (isRestriced) return;

        if (!character.isGrounded)
            moveDir.y -= gravity * Time.deltaTime;

        if (isSprinting)
            moveSpeed = sprintSpeed;

        if (isCrouching)
            moveSpeed = crouchSpeed;

        if (!isCrouching && !isSprinting)
            moveSpeed = walkSpeed;

        Vector2 currentInput = new Vector2(playerMoveInput.x * moveSpeed, playerMoveInput.y * moveSpeed);

        if (moveStates != MoveStates.wallrun && character.isGrounded)
        {
            float movedirY = moveDir.y;
            moveDir = (transform.TransformDirection(Vector3.forward) * currentInput.y) + (transform.TransformDirection(Vector3.right) * currentInput.x);
            moveDir.y = movedirY;
        }

        if (character.isGrounded && Physics.Raycast(transform.position, Vector3.down, out RaycastHit slopeHit, 2f))
        {
            hitpointnormal = slopeHit.normal;
            slopeAngle = Vector3.Angle(hitpointnormal, Vector3.up);


            if (slopeAngle <= character.slopeLimit)
            {
                float slowdownMultiCalculator = (character.slopeLimit - slopeAngle) / character.slopeLimit;
                float slowfactor = Mathf.Lerp(slopSlowMulitplier, 1, slowdownMultiCalculator);
                moveDir.x *= slowfactor;
                moveDir.z *= slowfactor;
            }
            else
            {
                moveDir = Vector3.zero;
                moveDir += new Vector3(hitpointnormal.x, -hitpointnormal.y, hitpointnormal.z) * slideSlopeSpeed;
            }
        }

        //movement for the player
        character.Move(moveDir * Time.deltaTime);
    }

    public void Walking(InputAction.CallbackContext context)
    {
        playerMoveInput = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            if (character.isGrounded && context.started && moveStates != MoveStates.wallrun)
                moveDir.y = jumpheight;

            if (moveStates == MoveStates.wallrun)
                wallRun.WallJump();

            if (cliffAndLedgeMovement.isOnLedge)
                cliffAndLedgeMovement.LedgeJump();
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
