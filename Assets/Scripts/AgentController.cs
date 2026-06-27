using UnityEngine;
using UnityEngine.InputSystem;

public class AgentController : MonoBehaviour
{
    [SerializeField] private InputActionAsset InputActions;

    public float moveSpeed = 5.0f;
    public bool isHeuristic = false;

    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction jumpAction;

    private Vector2 moveVektor;
    private Vector2 lookVektor;
    private bool jumpCall = false;

    private Rigidbody rb;
    private bool isGrounded = true;

    void OnEnable()
    {
        InputActions.FindActionMap("Player").Enable();
    }

    void OnDisable()
    {
        InputActions.FindActionMap("Player").Disable();
    }

    void Awake()
    {
        moveAction = InputSystem.actions.FindAction("Move");
        lookAction = InputSystem.actions.FindAction("Look");
        jumpAction = InputSystem.actions.FindAction("Jump");

        jumpCall = false;

        rb = gameObject.GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if (isHeuristic) SetInput(moveAction.ReadValue<Vector2>(), lookAction.ReadValue<Vector2>(), jumpAction.ReadValue<float>() > 0.5f);
        setRotation(lookVektor.x);
        moveBody();
        jump();
    }

    private void moveBody()
    {
        if (!isGrounded) return;
        Vector3 movement = new Vector3(moveVektor.x, 0.0f, moveVektor.y);
        Quaternion rotation = Quaternion.Euler(0, transform.eulerAngles.y, 0);
        movement.Normalize();
        movement.z = movement.z > 0 ? movement.z * 1.2f : movement.z;
        movement = rotation * movement;
        movement *= moveSpeed;
        movement.y = rb.linearVelocity.y;
        rb.linearVelocity = movement;
        //rb.AddForce(movement);
    }

    private void setRotation(float rotationAmount)
    {
        Quaternion rotation = Quaternion.Euler(0, rotationAmount, 0);
        transform.rotation *= rotation;
    }

    private void jump()
    {
        if (!jumpCall) return;
        if (!isGrounded) return;
        //Vector3 velocity = rb.linearVelocity;
        //velocity.y = Mathf.Sqrt(2f * 5.0f * Mathf.Abs(Physics.gravity.y));
        //rb.linearVelocity = velocity;
        //isGrounded = false;
        rb.AddForce(Vector3.up * 3.0f, ForceMode.Impulse);
    }

    public void SetInput(Vector2 move, Vector2 look, bool jump)
    {
        moveVektor = move;
        lookVektor = look;
        jumpCall = jump;
    }

    public bool GetIsGrounded()
    {
        return isGrounded;
    }

    private void OnTriggerStay(Collider other)
    {
        isGrounded = true;
    }

    private void OnTriggerExit(Collider other)
    {
        isGrounded = false;
    }
}
