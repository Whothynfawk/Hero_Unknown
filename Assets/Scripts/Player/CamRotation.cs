using UnityEngine;
using UnityEngine.InputSystem;

public class CamRotation : MonoBehaviour
{

    [Header("references")]
    public Movement movement;
    public Transform player;
    //input
    private Vector2 playerRotationInput;
    private float Xrotation;

    [Header("mouse sense")]
    [SerializeField] private float rotateSpeed;

    [Header("mouse lock")]
    [SerializeField] private float minClamp, maxClamp;

    [Header("headbob")]
    [SerializeField] private float walkbob;
    [SerializeField] private float sprintbob;
    [SerializeField] private float crouchbob;
    [SerializeField] private float wallRunbob;

    [Header("headbob amount")]
    [SerializeField] private float walkbobAmaount;
    [SerializeField] private float sprintBobAmaount;
    [SerializeField] private float crouchBobAmount;
    [SerializeField] private float wallRunBobAmount;


    private float defaultY, defaultX;
    private float timer;

    private void Awake()
    {
        defaultY = this.transform.localPosition.y;
        defaultX = this.transform.localPosition.x;
    }
    void FixedUpdate()
    {
        Vector3 playerRot = new Vector3(playerRotationInput.y * rotateSpeed, playerRotationInput.x * rotateSpeed, 0);
        Xrotation -= playerRot.x;

        Xrotation = Mathf.Clamp(Xrotation, minClamp, maxClamp);
        transform.localRotation = Quaternion.Euler(Xrotation, 0, 0);
        player.Rotate(Vector3.up * playerRot.y);

        HeadBob();
    }

    public void PlayerRotation(InputAction.CallbackContext conetext)
    {
        playerRotationInput = conetext.ReadValue<Vector2>();
    }

    private void HeadBob()
    {
        if (!movement.character.isGrounded && !movement.isWallrunning) return;

        bool crouch = movement.isCrouching;
        bool sprint = movement.isSprinting;
        bool wall = movement.isWallrunning;

        float bobSpeed = crouch ? crouchbob : sprint ? sprintbob : wall ? wallRunbob : walkbob;
        float bobAmount = crouch ? crouchBobAmount : sprint ? sprintBobAmaount : wall ? wallRunBobAmount : walkbobAmaount;

        timer += Time.deltaTime * bobSpeed;

        this.transform.localPosition = new Vector3(
            defaultX + Mathf.Cos(timer) * bobAmount,
            defaultY + Mathf.Sin(timer) * bobAmount,
            this.transform.localPosition.z);
    }
}
