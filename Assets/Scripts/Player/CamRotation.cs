using UnityEngine;
using UnityEngine.InputSystem;

public class CamRotation : MonoBehaviour
{

    [Header("references")]
    public Movement movement;

    public Transform player;

    private Vector2 playerRotationInput;
    private float Xrotation;

    [SerializeField] private float rotateSpeed;

    [SerializeField] 
    private float minClamp, maxClamp;

    [Header("headbob")]
    [SerializeField] 
    float walkbob, sprintbob, crouchbob;

    [SerializeField]
    float walkbobAmaount, SprintBobAmaount, crouchBobAmount;

    private float defaultY,defaultX;
    private float timer;

    private void Awake()
    {
        defaultY = this.transform.localPosition.y;
        defaultX  = this.transform.localPosition.x;
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
        if (!movement.character.isGrounded) return;
            
        if (Mathf.Abs(movement.moveDir.x) > 0.1f || Mathf.Abs(movement.moveDir.z) > 0.1f)
        {
            timer += Time.deltaTime * (movement.isCrouching ? crouchbob : movement.isSprinting ? sprintbob : walkbob);
            this.transform.localPosition = new Vector3(
                defaultY + Mathf.Cos(timer) *
                (movement.isCrouching ? crouchBobAmount
                : movement.isSprinting ? SprintBobAmaount
                : walkbobAmaount),
                defaultY + Mathf.Sin(timer) * 
                (movement.isCrouching ? crouchBobAmount
                : movement.isSprinting ? SprintBobAmaount 
                : walkbobAmaount), this.transform.localPosition.z);
        }
    }
}
