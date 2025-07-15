using Unity.Cinemachine;
using UnityEngine;

public class WallRun : MonoBehaviour
{
    [Header("references")]
    [SerializeField] private Movement movement;
    [SerializeField] private CliffAndLedgeMovement cliffAndLedgeMovement;

    [Header("layers")]
    [SerializeField] private LayerMask whatIsWall;
    [SerializeField] private LayerMask whatIsGround;

    [Header("wall running")]
    [SerializeField] private float WallrunSpeed;
    [SerializeField] private float WallJumpHeight;
    private float wallRunForce = 24f;
    private float wallSlideForce = 15f;
    private float minJumpHeight = 1.5f;
    private float gravityForce;

    // Detection
    private RaycastHit wallLeftHit;
    private RaycastHit wallRightHit;
    private bool wallLeft;
    private bool wallRight;
    private float wallDist = 1.5f;

    // Exit time
    private bool isExitingWall;
    private float exitTime = 0.2f;
    private float exitTimer;

    [Header("Effects")]
    [SerializeField] private CinemachineCamera playerCam;
    [SerializeField] private float Fov;
    [SerializeField] float wallrunFov;
    [SerializeField] float wallrunFovSpeed;
    [SerializeField] private float camTilt;
    [SerializeField] private float camTiltSpeed;

     public float tilt { get; private set; }

    private void Start()
    {
        gravityForce = movement.gravity;
    }

    private void Update()
    {
        WallChecks();
        States();
        CamEffects();
        if (isExitingWall)
        {
            if (movement.moveStates == MoveStates.wallrun)
                StopWallRun();

            if (exitTimer > 0)
                exitTimer -= Time.deltaTime;
            else
                isExitingWall = false;
        }
    }

    private void FixedUpdate()
    {
        if (movement.moveStates == MoveStates.wallrun)
            WallRunning();
    }

    private void CamEffects()
    {
        if (movement.moveStates != MoveStates.wallrun)
        {
            playerCam.Lens.FieldOfView = Mathf.Lerp(playerCam.Lens.FieldOfView, Fov, wallrunFovSpeed * Time.deltaTime);
            tilt = Mathf.Lerp(tilt, 0, camTiltSpeed * Time.deltaTime);
        }
        else
        {
            playerCam.Lens.FieldOfView = Mathf.Lerp(playerCam.Lens.FieldOfView, wallrunFov, wallrunFovSpeed * Time.deltaTime);

            if (wallLeft)
                tilt = Mathf.Lerp(tilt, -camTilt, camTiltSpeed * Time.deltaTime);
            else if (wallRight)
                tilt = Mathf.Lerp(tilt, camTilt, camTiltSpeed * Time.deltaTime);
        }
    }

    private void WallChecks()
    {
        wallRight = Physics.Raycast(transform.position, transform.right, out wallRightHit, wallDist, whatIsWall);
        wallLeft = Physics.Raycast(transform.position, -transform.right, out wallLeftHit, wallDist, whatIsWall);
    }

    private void States()
    {
        if (cliffAndLedgeMovement.isOnLedge)
        {
            StopWallRun();
        }

        if (cliffAndLedgeMovement.isOnLedge) return;

        if (AboveGround() && !isExitingWall)
        {
            if (wallLeft && movement.playerMoveInput.x < 0)
            {
                if (movement.moveStates != MoveStates.wallrun)
                {
                    StartWallRun();
                }
            }
            else if (wallRight && movement.playerMoveInput.x > 0)
            {
                if (movement.moveStates != MoveStates.wallrun)
                {
                    StartWallRun();
                }
            }
            else
            {
                if (movement.moveStates == MoveStates.wallrun)
                {
                    StopWallRun();
                }
            }
        }
        else
        {
            if (movement.moveStates == MoveStates.wallrun)
            {
                StopWallRun();
            }
        }
    }

    private bool AboveGround()
    {
        return !Physics.Raycast(transform.position, Vector3.down, minJumpHeight, whatIsGround);
    }

    private void StartWallRun()
    {
        movement.moveStates = MoveStates.wallrun;
        movement.gravity = 0;

    }

    private void WallRunning()
    {
        Vector3 wallNormal = wallRight ? wallRightHit.normal : wallLeftHit.normal;
        Vector3 wallForward = Vector3.ProjectOnPlane(transform.forward, wallNormal).normalized;

        if (Vector3.Dot(transform.forward, wallForward) < 0)
            wallForward = -wallForward;

        Vector3 runDir = wallForward * wallRunForce;

        if (movement.playerMoveInput.y != 0)
        {
            movement.moveDir += -wallNormal * wallSlideForce * Time.fixedDeltaTime;
            movement.isRunningOnWall = true;
        }
        else
        {
            movement.isRunningOnWall = false;
        }


        movement.moveDir.y = -0.1f;

        if (movement.moveStates == MoveStates.wallrun)
        {
            movement.moveDir.z = wallForward.z * WallrunSpeed * movement.playerMoveInput.y;
        }


    }

    public void StopWallRun()
    {
        movement.moveStates = MoveStates.ground;

        movement.gravity = gravityForce;
    }

    public void WallJump()
    {
        if (cliffAndLedgeMovement.isOnLedge || cliffAndLedgeMovement.exitingLedge) return;

        Vector3 wallNormal = wallRight ? wallRightHit.normal : wallLeftHit.normal;

        movement.moveDir.y = WallJumpHeight * 0.5f;

        movement.moveDir += wallNormal * WallJumpHeight;

        isExitingWall = true;
        exitTimer = exitTime;
    }
}
