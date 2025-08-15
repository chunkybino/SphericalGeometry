using UnityEngine;

public class GuyController4D : MonoBehaviour
{
    [SerializeField] Transform4D transform4;
    [SerializeField] Rigidbody4D rb;
    [SerializeField] new Camera4D camera;
    Transform4D camTransform {get{return camera.transform4;}}

    [SerializeField] PlayerInput input;

    [SerializeField] float speed = 0.4f;
    [SerializeField] Vector3 moveVector;

    [SerializeField] float lookSpeed = 1;

    [SerializeField] bool doCameraPitch;

    [SerializeField] float jumpForce;

    [SerializeField] GroundCheck groundCheck;

    void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        //move
        moveVector = Vector3.zero;

        if (input.left) moveVector.x--;        
        if (input.right) moveVector.x++;
        if (input.backward) moveVector.z++;        
        if (input.forward) moveVector.z--;  

        if (input.down) moveVector.y--;      
        if (input.up) moveVector.y++;    

        rb.SetRelativeVelocityX(moveVector.x * speed);
        rb.SetRelativeVelocityZ(moveVector.z * speed);

        //jump
        if (input.spacePress && groundCheck.grounded) {
            rb.SetVelocityTowards(new Vector4(0,1,0,0), jumpForce);
        }

        //look
        Vector2 angleVector = input.mouseDelta * lookSpeed;

        if (camera && doCameraPitch) 
        {
            camera.pitchAngle = Mathf.Clamp(camera.pitchAngle + (angleVector.y * Time.deltaTime), -Mathf.PI/2, Mathf.PI/2);
        }

        transform4.RotateRelativeXZ(-angleVector.x  * Time.deltaTime);
        //rb.angularVelocity = new Vector3(0, -angleVector.x, 0);
    }
}
