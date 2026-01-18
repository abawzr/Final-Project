using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Move")]
    [SerializeField] private float speed = 6.0f;

    [Header("Jump")]
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float SinglejumpHeight = 4f;

    [Header("Look")]
    [SerializeField] private float mouseSensitivity = 1f;
    [SerializeField] private float lookLimit = 90f;
    [SerializeField] private Transform cameraHolder;

    [Header("Controls")]
    [SerializeField] private bool controlsEnabled = true;

    private CharacterController cc;
    private float velocity;
    private float xRotation = 0f;

    private void Awake()
    {
        cc = GetComponent<CharacterController>();
    }

    private void Update()
    {
        if (!controlsEnabled) return;

        Move();
        Jump();
        ApplyGravity();
    }

    private void LateUpdate()
    {
        if (!controlsEnabled) return;

        Look();
    }

    private void Look()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -lookLimit, lookLimit);
        cameraHolder.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        transform.Rotate(Vector3.up * mouseX);
    }

    private void Move()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");
        Vector3 move = transform.right * moveX + transform.forward * moveZ;
        move = Vector3.ClampMagnitude(move, 1f) * speed;
        move.y = velocity;
        cc.Move(move * Time.deltaTime);
    }

    private void Jump()
    {
        //Single Jump
        if (cc.isGrounded && Input.GetKeyDown(KeyCode.Space))
        {
            velocity = SinglejumpHeight;
        }
    }

    private void ApplyGravity()
    {
        if (cc.isGrounded && velocity < 0f)
            velocity = -2f;

        else
        {
            velocity += gravity * Time.deltaTime;
        }
    }

}
