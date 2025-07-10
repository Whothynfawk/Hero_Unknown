using UnityEngine;

public class WallRun : MonoBehaviour
{
    [Header("references")]
    [SerializeField] private Movement movement;

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

    private void Start()
    {
        gravityForce = movement.gravity;
    }

    private void Update()
    {
        WallChecks();
        States();

        if (isExitingWall)
        {
            if (movement.isWallrunning)
                StopWallRun();

            if (exitTimer > 0)
                exitTimer -= Time.deltaTime;
            else
                isExitingWall = false;
        }
    }

    private void FixedUpdate()
    {
        if (movement.isWallrunning)
            WallRunning();
    }

    private void WallChecks()
    {
        wallRight = Physics.Raycast(transform.position, transform.right, out wallRightHit, wallDist, whatIsWall);
        wallLeft = Physics.Raycast(transform.position, -transform.right, out wallLeftHit, wallDist, whatIsWall);
    }

    private void States()
    {
        if (AboveGround() && !isExitingWall)
        {
            if (wallLeft && movement.playerMoveInput.x < 0)
            {
                if (!movement.isWallrunning)
                {
                    StartWallRun();
                    movement.isRunningOnWall = true;
                }
            }
            else if (wallRight && movement.playerMoveInput.x > 0)
            {
                if (!movement.isWallrunning)
                {
                    StartWallRun();
                    movement.isRunningOnWall = true;
                }
            }
            else
            {
                if (movement.isWallrunning)
                {
                    StopWallRun();
                    movement.isRunningOnWall = false;

                }
            }
        }
        else
        {
            if (movement.isWallrunning)
            {
                StopWallRun();
                movement.isRunningOnWall = false;

            }
        }
    }

    private bool AboveGround()
    {
        return !Physics.Raycast(transform.position, Vector3.down, minJumpHeight, whatIsGround);
    }

    private void StartWallRun()
    {
        movement.isWallrunning = true;
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
            movement.moveDir += -wallNormal * wallSlideForce * Time.fixedDeltaTime;

        movement.moveDir.y = -0.1f;

        if (movement.isWallrunning)
            movement.moveDir.z = wallForward.z * WallrunSpeed * movement.playerMoveInput.y;


    }

    private void StopWallRun()
    {
        movement.isWallrunning = false;
        movement.gravity = gravityForce;
    }

    public void WallJump()
    {
        Vector3 wallNormal = wallRight ? wallRightHit.normal : wallLeftHit.normal;

        movement.moveDir.y = WallJumpHeight;

        movement.moveDir += wallNormal * wallSlideForce;

        isExitingWall = true;
        exitTimer = exitTime;
    }
}
