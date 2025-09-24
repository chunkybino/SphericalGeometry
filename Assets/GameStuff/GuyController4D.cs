using UnityEngine;

public class GuyController4D : MonoBehaviour
{
    [SerializeField] Transform4D transform4;
    [SerializeField] Rigidbody4D rb;
    [SerializeField] new Camera4D camera;
    Transform4D camTransform { get { return camera.transform4; } }

    [SerializeField] PlayerInput input;


    //movement
    [SerializeField] float speed = 1;
    [SerializeField] float acceleration = 4f;
    [SerializeField] float opposeAccelMult = 4f; //if target direction in opposite direction of current vel (based on dot product), accelerate faster

    [SerializeField] float airDeccelerationMult = 0.5f; //if we're in the air, lower decceleration at low speeds
    [SerializeField] float airDeccelerationVelocityThreshold = 0.5f; //only activate lower air decceleration if our velocity is lower than this*speed

    [SerializeField] Vector3 moveVector;
    Vector2 moveVector2 { get { return new Vector2(moveVector.x, moveVector.z); } }

    [SerializeField] float lookSpeed = 0.02f;

    [SerializeField] bool doCameraPitch;

    //jump
    [SerializeField] float jumpForce;
    [SerializeField] bool isJump;
    [SerializeField] float releaseVelMult = 0.4f;

    [SerializeField] GroundCheck groundCheck;

    //dash
    bool isDash { get { return dashTimer > 0; } }
    [SerializeField] float dashTime = 0.4f;
    [SerializeField] float dashTimer;
    float dashProgress { get { return 1 - dashTimer / dashTime; } }
    [SerializeField] AnimationCurve dashSpeedCurve;
    [SerializeField] AnimationCurve dashControlCurve;

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

        //if (input.down) moveVector.y--;      
        //if (input.up) moveVector.y++;

        if (moveVector.x != 0 && moveVector.z != 0) moveVector *= 0.707f;

        //jump
        if (input.spacePress && groundCheck.grounded)
        {
            rb.SetVelocityTowards(new Vector4(0, 1, 0, 0), jumpForce);
            isJump = true;
        }

        //look
        Vector2 angleVector = input.mouseDelta * lookSpeed;

        if (camera && doCameraPitch)
        {
            camera.pitchAngle = Mathf.Clamp(camera.pitchAngle + (angleVector.y), -Mathf.PI / 2, Mathf.PI / 2);
        }

        transform4.RotateRelativeXZ(-angleVector.x);
        //rb.angularVelocity = new Vector3(0, -angleVector.x, 0);
    }

    void FixedUpdate()
    {
        float yVel = rb.GetRelativeVelocity(1);

        if (groundCheck.grounded && yVel < 0.05f)
        {
            isJump = false;
        }

        if (isJump && !input.space && yVel > 0) //if space released
        {
            rb.SetRelativeVelocityY(yVel * releaseVelMult);
            isJump = false;
        }

        VelocityFunc();
    }

    void VelocityFunc()
    {
        Vector2 relativeVel = rb.GetRelativeVelocityXZ();

        Vector2 targetSpeed = moveVector2 * speed;

        float accel = acceleration;

        if (targetSpeed != Vector2.zero)
        {
            float oppositeAccelFactor = 0.5f * (1 - Vector2.Dot(relativeVel.normalized, (targetSpeed - relativeVel).normalized));
            accel *= 1 + opposeAccelMult * oppositeAccelFactor;
        }
        else
        {
            if (!groundCheck.grounded && relativeVel.magnitude < airDeccelerationVelocityThreshold * speed)
            {
                accel *= airDeccelerationMult;
            }
        }

        Vector2 newVel = new Vector2(
            Mathf.MoveTowards(relativeVel.x, targetSpeed.x, accel * Time.fixedDeltaTime),
            Mathf.MoveTowards(relativeVel.y, targetSpeed.y, accel * Time.fixedDeltaTime)
        );

        rb.SetRelativeVelocityX(newVel.x);
        rb.SetRelativeVelocityZ(newVel.y);
    }
}
