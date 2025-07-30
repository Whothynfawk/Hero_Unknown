using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerLook : MonoBehaviour
{
    [Header("referances")]
    [SerializeField] private StateHandler stateHandler;
    [SerializeField] private PlayerWallRun wallrun;
    [SerializeField] private Transform player;
    [SerializeField] private CinemachineCamera cam;

    //Input
    private Vector2 mouseInput;
    private Vector3 playerRotations;
    private float xRot;

    [Header("mouseSense")]
    [SerializeField] private float mouseSense;

    [Header("clamps")]
    private float minClamp = -80;
    private float maxClamp = 80;

    [Header("Wallrun")]
    private float normalZoom = 90;
    private float wallrunZoom = 110f;
    private float wallrunZoomSpeed = 20f;
    private float wallrunTiltAmount = 20f;
    private float wallrunTiltSpeed = 20f;

    public float tilt { get; private set; }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void FixedUpdate()
    {
        PlayerRotation();
        WallRunTilt();
    }

    private void PlayerRotation()
    {
        //setUp and Calculations
        playerRotations = new Vector3(mouseInput.y, mouseInput.x, 0) * mouseSense * Time.deltaTime;
        xRot -= playerRotations.x;
        xRot = Mathf.Clamp(xRot, minClamp, maxClamp);

        //playerRotation
        transform.localRotation = Quaternion.Euler(xRot, 0, tilt);
        player.Rotate(Vector3.up, playerRotations.y);
    }

    private void WallLookRotation()
    {

    }

    private void WallRunTilt()
    {
        if (stateHandler.state != MovementStates.wallRun)
        {
            cam.Lens.FieldOfView = Mathf.Lerp(cam.Lens.FieldOfView, normalZoom, wallrunZoomSpeed * Time.deltaTime);
            tilt = Mathf.Lerp(tilt, 0, wallrunTiltSpeed * Time.deltaTime);
        }
        else
        {
            cam.Lens.FieldOfView = Mathf.Lerp(cam.Lens.FieldOfView, wallrunZoom, wallrunZoomSpeed * Time.deltaTime);
            if (wallrun.wallLeft)
            {
                tilt = Mathf.Lerp(tilt, -wallrunTiltAmount, wallrunTiltSpeed * Time.deltaTime);
                Debug.Log("left");
            }
            else if (wallrun.wallRight)
            {
                tilt = Mathf.Lerp(tilt, wallrunTiltAmount, wallrunTiltSpeed * Time.deltaTime);
                Debug.Log("Right");
            }
        }
    }

    public void RotationInput(InputAction.CallbackContext context)
    {
        mouseInput = context.ReadValue<Vector2>();
    }
}
