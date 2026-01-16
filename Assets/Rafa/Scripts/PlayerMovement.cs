using Unity.VisualScripting;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Move")]
    public CharacterController cc;
    public float speed = 6.0f;

    [Header("Jump")]
    public float gravity = -9.81f;
    public float SinglejumpHeight = 2f;
    //public float DoublejumpHeight = 4f;
    //public int maxJumpCount = 2;
    //int jumpCount = 0;

    [Header("Look")]
    public float mouseSensitivity = 150f;
    public float lookLimit=80f;
    public Transform cameraHolder;

    bool isGrounded;
    Vector3 velocity;
    float xRotation = 0f;
    public bool controlsEnabled=true;
    void Awake() {
        cc = GetComponent<CharacterController>();
        if (!cameraHolder && Camera.main) cameraHolder = Camera.main.transform;
    }
     void Update()
    {
        if (!controlsEnabled) return;

        Look();
        Move();
        Jump();
    }
     void Look()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -80f, 80f);
        cameraHolder.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        transform.Rotate(Vector3.up * mouseX);
    }
    void Move()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");
        Vector3 move = transform.right * moveX + transform.forward * moveZ;
        move=Vector3.ClampMagnitude(move,1f);
        cc.Move(move * speed * Time.deltaTime);
    }
    void Jump()
    {
        if(cc.isGrounded&&velocity.y<0f)
            velocity.y=-2f;

        //Single Jump
        if(cc.isGrounded && Input.GetKeyDown(KeyCode.Space))
        {
            velocity.y = Mathf.Sqrt(SinglejumpHeight * -2f * gravity);
           //jumpCount++;
        }
        //Double Jump
        /*else if(!cc.isGrounded && Input.GetKeyDown(KeyCode.Space) && jumpCount < maxJumpCount)
        {
            velocity.y = Mathf.Sqrt(DoublejumpHeight*-2f*gravity);
            jumpCount++;
            
        }*/
        velocity.y += gravity * Time.deltaTime;
        cc.Move(Vector3.up* velocity.y * Time.deltaTime);
    }
    
}
