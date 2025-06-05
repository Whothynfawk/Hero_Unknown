using UnityEngine;
using UnityEngine.InputSystem;

public class MOVEBITCH : MonoBehaviour
{
    private Rigidbody body;

    private Vector2 input;

    [SerializeField] private float sped ;
    void Start()
    {
        body = GetComponent<Rigidbody>();

    }

    void FixedUpdate()
    {
        body.linearVelocity = new Vector3(input.x * sped, 0, input.y * sped);

    }

    public void OnMove(InputAction.CallbackContext context)
    {
        input = context.ReadValue<Vector2>();
    }
}
