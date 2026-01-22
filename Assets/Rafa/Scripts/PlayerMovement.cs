using System.Collections;
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

    [Header("Audio")]
    [SerializeField] private Transform footstepTransform;
    [SerializeField] private AudioClip footstepClip;
    [SerializeField] private float footstepInterval;

    private CharacterController cc;
    private Vector3 move;
    private float velocity;
    private float xRotation = 0f;

    public static bool IsControlsEnabled { get; set; }

    private void Awake()
    {
        cc = GetComponent<CharacterController>();

        IsControlsEnabled = true;

        velocity = 0;
        xRotation = 0;

        StartCoroutine(PlayFootstepSound());
    }

    private void Update()
    {
        if (!IsControlsEnabled) return;

        Move();
        Jump();
        ApplyGravity();
    }

    private void LateUpdate()
    {
        if (!IsControlsEnabled) return;

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
        move = transform.right * moveX + transform.forward * moveZ;
        move = Vector3.ClampMagnitude(move, 1f) * speed;
        move.y = velocity;
        cc.Move(move * Time.deltaTime);
    }

    private void Jump()
    {
        //Single Jump
        if (cc.isGrounded && Input.GetButtonDown("Jump"))
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

    private IEnumerator PlayFootstepSound()
    {
        while (true)
        {
            if (cc.isGrounded && move.x != 0 && move.z != 0)
            {
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.Play3DSFX(footstepClip, footstepTransform.position);
                }

                yield return new WaitForSeconds(footstepInterval);
            }
            yield return null;
        }
    }
}
