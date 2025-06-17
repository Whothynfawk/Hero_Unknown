using UnityEngine;
using UnityEngine.InputSystem;

public class CamRotation : MonoBehaviour
{
    public Transform player;

    private Vector2 playerRotationInput;
    private float Xrotation;

    [SerializeField] private float rotateSpeed;
    [SerializeField] private float minClamp, maxClamp;

    void FixedUpdate()
    {
        Vector3 playerRot = new Vector3(playerRotationInput.y * rotateSpeed, playerRotationInput.x * rotateSpeed, 0);
        Xrotation -= playerRot.x;

        Xrotation = Mathf.Clamp(Xrotation, minClamp, maxClamp);
        transform.localRotation = Quaternion.Euler(Xrotation, 0, 0);
        player.Rotate(Vector3.up * playerRot.y);

    }

    public void PlayerRotation(InputAction.CallbackContext conetext)
    {
        playerRotationInput = conetext.ReadValue<Vector2>();
    }
}
