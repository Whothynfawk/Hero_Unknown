using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class CliffAndLedgeMovement : MonoBehaviour
{
    [Header("reffrences")]
    public Movement movement;
    public Transform orientation;
    public CinemachineCamera cam;
    public WallRun wallRun;

    //grab
    public float moveTolegdeSpeed;
    private float ledgeGrabDist;
    private float timeOnLedge;
    public bool isOnLedge;

    [Header("ledge")]
    public LayerMask ledgeMask;
    private float Length = 1f;
    private float radius = 0.05f;
    private RaycastHit ledgeHit;

    private Transform currentLedge;
    private Transform lastLedge;

    [Header("gravity")]
    private float normalGravity;

    [Header("Legde Jump")]
    [SerializeField] private float ledgeJumpForward;
    [SerializeField] private float ledgeJumpUpwardsward;

    [Header("exit")]
    private float exitLedgeTime = 0.5f;
    [HideInInspector] public bool exitingLedge;
    private float exitLedgeTimer;

    [Header("shimmie")]
    [SerializeField] private float cliffShimmieSpeed;
    //ledgediretion
    private Vector3 ledgeNormal;
    //detection
    private bool ledgeDetecion;
    //adjustment
    private bool ledgeHeighAdjustment;

    private void Start()
    {
        normalGravity = movement.gravity;
    }

    private void Update()
    {
        StateMachine();
        LedgeDetection();
        LedgeShimmie();
    }

    private void StateMachine()
    {
        if (isOnLedge)
        {
            HoldingOnToLegde();

            timeOnLedge += Time.deltaTime;

            if (movement.playerMoveInput.y > 0.5f && timeOnLedge > 0.5f)
                ClimbLedge();

            if (movement.playerMoveInput.y < -0.5f && timeOnLedge > 0.5f)
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
        if (currentLedge != null || exitingLedge) return;

        Vector3 ledgeCheckOrigin = orientation.position + Vector3.up * -0.5f;
        bool ledgeDetected = Physics.SphereCast(ledgeCheckOrigin, radius, transform.forward, out ledgeHit, Length, ledgeMask);

        if (!ledgeDetected && movement.character.isGrounded) return;
        if (ledgeHit.transform == null) return;
        if (ledgeHit.transform == lastLedge) return;

        float distance = Vector3.Distance(orientation.position, ledgeHit.point);
        float adaptiveGrabDist = GetAdaptiveLedgeGrabDistance();

        if (distance < adaptiveGrabDist && !isOnLedge && wallRun.AboveGround())
        {
            EnterLegdeHiold();
        }
    }



    private void EnterLegdeHiold()
    {
        isOnLedge = true;
        movement.isRunningOnWall = false;

        if (movement.wallRun)
            wallRun.StopWallRun();

        movement.moveStates = MoveStates.unlimited;

        movement.gravity = 0;
        movement.moveDir = Vector3.zero;

        movement.isRestriced = true; 

        ledgeNormal = ledgeHit.normal;
        currentLedge = ledgeHit.transform;
        lastLedge = ledgeHit.transform;

        HeighAdjustment();
    }


    private void HeighAdjustment()
    {
        if (movement.isRestriced && !ledgeHeighAdjustment && !exitingLedge && isOnLedge && currentLedge != null)
        {
            StartCoroutine(Adjuster());
        }
    }

    IEnumerator Adjuster()
    {
        ledgeHeighAdjustment = true;
        float pullUpSpeed = 0.05f;

        while (true)
        {
            Vector3 rayDir = transform.forward;
            Vector3 rayOrigin = orientation.position + orientation.forward * 0.3f + Vector3.up * -0.3f;

            Debug.DrawRay(rayOrigin, rayDir * Length, Color.green, 0.1f);

            if (!Physics.Raycast(rayOrigin, rayDir, out RaycastHit hit, Length, ledgeMask))
                break;

            transform.position += Vector3.up * pullUpSpeed;

            yield return null;
        }

        movement.isRestriced = false;
        ledgeHeighAdjustment = false;
    }


    private float GetAdaptiveLedgeGrabDistance()
    {
        float baseValue = 5f;

        if (currentLedge == null) return baseValue;

        Collider ledgeCol = currentLedge.GetComponent<Collider>();
        if (ledgeCol == null) return baseValue;

        Vector3 cliffNormal = Vector3.Cross(ledgeNormal, Vector3.up);
        Vector3 ledgeRight = new Vector3(cliffNormal.x, 0f, cliffNormal.z).normalized;

        float halfExtent = Vector3.Project(ledgeCol.bounds.extents, ledgeRight).magnitude;
        float edgeBuffer = 0.3f;
        float adaptiveDistance = halfExtent + edgeBuffer;

        return Mathf.Max(baseValue, adaptiveDistance);
    }


    private void HoldingOnToLegde()
    {
        movement.gravity = 0;

        Vector3 ledgeDir = currentLedge.position - transform.position;
        float distanceToLedge = Vector3.Distance(transform.position, currentLedge.position);
        float adaptiveGrabDist = GetAdaptiveLedgeGrabDistance();

        if (distanceToLedge > 1f)
        {
            movement.moveDir = ledgeDir.normalized * distanceToLedge * 1000f * Time.deltaTime;
        }
        else
        {
            if (movement.moveStates != MoveStates.freeze)
                movement.moveStates = MoveStates.freeze;
        }

        if (distanceToLedge > adaptiveGrabDist)
            ExitLedgeHold();
    }


    private void ExitLedgeHold()
    {
        isOnLedge = false;
        timeOnLedge = 0;
        currentLedge = null;

        exitingLedge = true;
        exitLedgeTimer = exitLedgeTime;

        movement.moveStates = MoveStates.ground;
        movement.moveDir = Vector3.zero;

        movement.gravity = normalGravity;

        StopAllCoroutines();
        Invoke(nameof(ResetLegde), 1f);
    }

    private void ClimbLedge()
    {
        Vector3 ledgePos = currentLedge.position;

        ExitLedgeHold();

        Vector3 toLedge = ledgePos - transform.position;
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

    private void LedgeShimmie()
    {
        if (currentLedge == null || !isOnLedge) return;

        Collider ledgeCol = currentLedge.GetComponent<Collider>();
        if (ledgeCol == null) return;

        Vector3 cliffNormal = Vector3.Cross(ledgeNormal, Vector3.up);
        Vector3 ledgeRight = new Vector3(cliffNormal.x, 0f, cliffNormal.z).normalized;

        Vector3 localPos = ledgeCol.transform.InverseTransformPoint(transform.position);

        float halfExtent = Vector3.Project(ledgeCol.bounds.extents, ledgeRight).magnitude;

        Vector3 toPlayer = transform.position - ledgeCol.bounds.center;
        float positionOnLedge = Vector3.Dot(toPlayer, ledgeRight);

        float edgeBuffer = 0.3f;

        bool canMoveRight = positionOnLedge < halfExtent - edgeBuffer;
        bool canMoveLeft = positionOnLedge > -halfExtent + edgeBuffer;

        if (movement.playerMoveInput.x > 0 && canMoveRight)
            movement.moveDir = ledgeRight * cliffShimmieSpeed;
        else if (movement.playerMoveInput.x < 0 && canMoveLeft)
            movement.moveDir = -ledgeRight * cliffShimmieSpeed;
        else
            movement.moveDir = Vector3.zero;
    }
}
