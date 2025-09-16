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

    [SerializeField] float lookSpeed = 0.02f;

    [SerializeField] bool doCameraPitch;

    [SerializeField] float jumpForce;
    [SerializeField] bool isJump;
    [SerializeField] float releaseVelMult = 0.4f;

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

        if (moveVector.x != 0 && moveVector.z != 0) moveVector *= 0.707f;

        rb.SetRelativeVelocityX(moveVector.x * speed);
        rb.SetRelativeVelocityZ(moveVector.z * speed);

        //jump
        if (input.spacePress && groundCheck.grounded) {
            rb.SetVelocityTowards(new Vector4(0,1,0,0), jumpForce);
            isJump = true;
        }

        //look
        Vector2 angleVector = input.mouseDelta * lookSpeed;

        if (camera && doCameraPitch) 
        {
            camera.pitchAngle = Mathf.Clamp(camera.pitchAngle + (angleVector.y), -Mathf.PI/2, Mathf.PI/2);
        }

        transform4.RotateRelativeXZ(-angleVector.x);
        //rb.angularVelocity = new Vector3(0, -angleVector.x, 0);
    }

    void FixedUpdate()
    {
        float yVel = rb.GetRelativeVelocity(1);

        if (groundCheck.grounded && yVel < 0.05f) {
            isJump = false;
        }

        if (isJump && !input.space && yVel > 0) //if space released
        {
            rb.SetRelativeVelocityY(yVel*releaseVelMult);
            isJump = false;
        }
    }
}
