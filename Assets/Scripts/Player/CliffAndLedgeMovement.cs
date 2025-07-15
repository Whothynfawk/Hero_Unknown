using Unity.Cinemachine;
using UnityEngine;

public class CliffAndLedgeMovement : MonoBehaviour
{
    [Header("reffrences")]
    public Movement movement;
    public Transform orientation;
    public CinemachineCamera cam;
    public WallRun wallRun;

    [Header("Grab")]
    public float moveTolegdeSpeed;
    public float legdeGrabDist;
    public float minTimeOnLedge;
    private float timeOnLegde;
    public bool isOnLedge;

    [Header("ledge")]
    [SerializeField] private float Length;
    [SerializeField] private float radius;
    public LayerMask ledgeMask;
    private RaycastHit ledgeHit;

    private Transform currentLedge;
    private Transform lastLedge;

    [Header("gravity")]
    private float normalGravity;

    [Header("Legde Jump")]
    public float ledgeJumpForward;
    public float ledgeJumpUpwardsward;

    [Header("exit")]
    [SerializeField] private float exitLedgeTime;
    public bool exitingLedge;
    private float exitLedgeTimer;


    private void Start()
    {
        normalGravity = movement.gravity;
    }

    private void Update()
    {
        StateMachine();
        LedgeDetection();
    }

    private void StateMachine()
    {
        if (isOnLedge)
        {
            HoldingOnToLegde();

            timeOnLegde += Time.deltaTime;

            if (movement.playerMoveInput.y > 0.5f && timeOnLegde > 0.5f)
                ClimbLedge();

            if (movement.playerMoveInput.y < -0.5f && timeOnLegde > 0.5f)
                DropDownLegde();

        }
        else if (exitingLedge)
        {
            if (exitLedgeTimer > 0)
                exitLedgeTimer -= Time.deltaTime;
            else
                exitingLedge = false;
        }
    }

    private void LedgeDetection()
    {
        bool ledgeDetecion = Physics.SphereCast(this.transform.position, radius, cam.transform.forward, out ledgeHit, Length, ledgeMask);

        if (!ledgeDetecion) return;

        float distance = Vector3.Distance(transform.position, ledgeHit.transform.position);

        if (ledgeHit.transform == lastLedge) return;

        if (distance < legdeGrabDist && !isOnLedge)
            EnterLegdeHiold();
    }

    private void EnterLegdeHiold()
    {
        isOnLedge = true;
        movement.isRunningOnWall = false;
        if (movement.wallRun)
            wallRun.StopWallRun();

        movement.moveStates = MoveStates.unlimited;
        movement.isRestriced = true;

        currentLedge = ledgeHit.transform;
        lastLedge = ledgeHit.transform;

        movement.gravity = 0;
        movement.moveDir = Vector3.zero;
    }

    private void HoldingOnToLegde()
    {
        movement.gravity = 0;

        Vector3 ledgeDir = currentLedge.position - this.transform.position;
        float distanceToLedge = Vector3.Distance(this.transform.position, currentLedge.position);

        if (distanceToLedge > 1f)
        {
            movement.moveDir = ledgeDir.normalized * distanceToLedge * 1000f * Time.deltaTime;
        }
        else
        {
            if (movement.moveStates != MoveStates.freeze)
                movement.moveStates = MoveStates.freeze;
        }

        if (distanceToLedge > legdeGrabDist)
            ExitLedgeHold();
    }

    private void ExitLedgeHold()
    {
        isOnLedge = false;
        timeOnLegde = 0;
        movement.isRestriced = false;

        exitingLedge = true;
        exitLedgeTimer = exitLedgeTime;

        movement.moveStates = MoveStates.ground;
        movement.gravity = normalGravity;
        movement.moveDir = Vector3.zero;



        StopAllCoroutines();
        Invoke(nameof(ResetLegde), 1);
    }

    private void ClimbLedge()
    {
        ExitLedgeHold();

        Vector3 toLedge = currentLedge.position - transform.position;
        toLedge.y = 0;

        Vector3 climbDir = toLedge.normalized + Vector3.up * 2.5f;
        movement.moveDir = climbDir.normalized * 10f;
    }

    private void DropDownLegde()
    {
        ExitLedgeHold();
        movement.moveDir = Vector3.zero;

    }

    private void ResetLegde()
    {
        lastLedge = null;
    }

    public void LedgeJump()
    {
        ExitLedgeHold();

        Invoke(nameof(LedgeJumpDelay), 0.05f);
    }

    private void LedgeJumpDelay()
    {
        Vector3 jumpPower = cam.transform.forward * ledgeJumpForward + orientation.up * ledgeJumpUpwardsward;
        movement.moveDir = Vector3.zero;
        movement.moveDir = jumpPower;
    }
}
