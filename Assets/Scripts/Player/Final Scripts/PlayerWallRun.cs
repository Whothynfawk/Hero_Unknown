using Unity.Mathematics;
using UnityEngine;

public class PlayerWallRun : MonoBehaviour
{
    [Header("References")]
    [SerializeField] PlayerMovement playerMovement;
    [SerializeField] PlayerLook playerLook;
    [SerializeField] StateHandler stateHandler;

    [Header("MovementSpeed")]
    [SerializeField] private float wallRunSpeed;

    [Header("calculations and hitStorages")]
    [SerializeField] private LayerMask wallLayerMask;
    [SerializeField] private LayerMask groundLayerMask;
    private RaycastHit wallHitLeft;
    private RaycastHit wallHitRight;
    private Vector3 wallNormal;
    private float wallDistance = 1f;
    private bool isHighEnough;
    public bool wallLeft;
    public bool wallRight;

    //wallGravity and force
    public float wallgravity = 2f;


    private void Update()
    {
        if (AboveGround())
        {
            Wallcheck();

            if (wallRight || wallLeft)
                IsWallRunning();
            else if (!wallLeft && !wallRight)
                StopWallrunning();
        }
    }

    private void Wallcheck()
    {
        wallLeft = Physics.Raycast(transform.position, -transform.right, out wallHitLeft, wallDistance, wallLayerMask);
        wallRight = Physics.Raycast(transform.position, transform.right, out wallHitRight, wallDistance, wallLayerMask);
    }

    private bool AboveGround()
    {
        return !Physics.Raycast(transform.position, Vector3.down, 2f, groundLayerMask);
    }

    private void IsWallRunning()
    {
        stateHandler.state = MovementStates.wallRun;
        wallNormal = wallLeft ? wallHitLeft.normal : wallHitRight.normal;

        Vector3 pushToWall = -wallNormal * 5f;
        Vector3 wallForward = Vector3.ProjectOnPlane(transform.forward, wallNormal).normalized;

        Quaternion targetRotation = Quaternion.LookRotation(wallForward, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 8f * Time.deltaTime);
    }

    private void StopWallrunning()
    {
        stateHandler.state = MovementStates.groundMovement;
    }
}
