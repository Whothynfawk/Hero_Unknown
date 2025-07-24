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

    [Header("Tslide")]
    private float slideThreshold = 6f;
    private float slideSpeed = 3f;
    private float slideSpeedDamp = 0.99f;
    private float keepSlidingThreshold = 3f;
    private bool isSliding;
    private bool crouchIsHeld;
    private bool wantsToSlide;
    private bool canSlideAfterWallJump;


    private void Start()
    {
        Cursor.visible = false;
        moveStates = MoveStates.ground;
    }

    private void FixedUpdate()
    {
        HandleSliding();
    }
    private void Update()
    {
        MoveInputs();
        States();

        if (character.isGrounded && wantsToSlide)
        {
            StartSliding();
            wantsToSlide = false;
        }
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
        if (isSliding)
        {
            character.Move(moveDir * Time.deltaTime);
            return;
        }

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
            if (character.isGrounded && moveStates != MoveStates.wallrun)
            {
                moveDir.y = jumpheight;
            }

            if (moveStates == MoveStates.wallrun)
            {
                wallRun.WallJump();
                StartCoroutine(AllowSlideAfterWallJump());
            }

            if (cliffAndLedgeMovement.isOnLedge)
            {
                cliffAndLedgeMovement.LedgeJump();
            }
        }
    }

    public void Sprint(InputAction.CallbackContext context)
    {
        isSprinting = context.ReadValueAsButton();
    }

    public void Crouch(InputAction.CallbackContext context)
    {
        crouchIsHeld = context.ReadValueAsButton();

        if (!isSprinting && !isSliding)
        {
            isCrouching = crouchIsHeld;
            StartCoroutine(CrouchMode());
        }

        // Queue slide if crouch is pressed
        if (crouchIsHeld)
        {
            wantsToSlide = true;
        }
        else
        {
            // Stop sliding immediately when crouch is released
            if (isSliding)
            {
                isSliding = false;

                // Stand up if crouch is no longer held
                if (!crouchIsHeld)
                {
                    isCrouching = false;
                    StartCoroutine(CrouchMode());
                }
            }
        }
    }


    IEnumerator AllowSlideAfterWallJump()
    {
        canSlideAfterWallJump = true;
        yield return new WaitForSeconds(0.5f); // Small window to allow slide after wall jump
        canSlideAfterWallJump = false;
    }

    IEnumerator CrouchMode()
    {
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
    }

    private void StartSliding()
    {
        if (isSliding) return; // Already sliding
        if (!character.isGrounded && !canSlideAfterWallJump) return; // Allow queued wall-jump slides

        isSliding = true;

        float currentSpeed = Mathf.Clamp(moveDir.magnitude / 40f, 0f, 1f);
        float boost = slideSpeed * 3f + Mathf.Lerp(0, slideSpeed * 2f, currentSpeed);

        Vector3 direction = new Vector3(moveDir.x, 0, moveDir.z);
        if (direction.magnitude < 0.1f)
            direction = transform.forward;
        direction.Normalize();

        moveDir = direction * boost;

        if (!isCrouching)
        {
            isCrouching = true;
            StartCoroutine(CrouchMode());
        }

        canSlideAfterWallJump = false;
    }

    private void HandleSliding()
    {
        if (!isSliding) return;

        moveDir.x *= slideSpeedDamp;
        moveDir.z *= slideSpeedDamp;
        moveDir.y -= gravity * Time.deltaTime;

        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 2f))
        {
            Vector3 slopeDir = Vector3.ProjectOnPlane(Vector3.down, hit.normal).normalized;
            moveDir += slopeDir * (slideSlopeSpeed * Time.deltaTime);
        }

        float horizontalSpeed = new Vector3(moveDir.x, 0, moveDir.z).magnitude;
        if (horizontalSpeed < keepSlidingThreshold || !isCrouching)
        {
            isSliding = false;

            if (!crouchIsHeld)
            {
                isCrouching = false;
                StartCoroutine(CrouchMode());
            }
        }
    }

}
