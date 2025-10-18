using UnityEngine;

public class GuyController4D : MonoBehaviour
{
    [SerializeField] Transform4D transform4;
    [SerializeField] Rigidbody4D rb;
    [SerializeField] new Camera4D camera;
    Transform4D camTransform { get { return camera.transform4; } }

    [SerializeField] PlayerInput input;

    Vector4 prevPosition; //position we were last fixed frame

    //movement
    [SerializeField] float speed = 1;
    [SerializeField] float acceleration = 4f;
    [SerializeField] float opposeAccelMult = 4f; //if target direction in opposite direction of current vel (based on dot product), accelerate faster

    [SerializeField] float airDeccelerationMult = 0.5f; //if we're in the air, lower decceleration at low speeds
    [SerializeField] float airDeccelerationVelocityThreshold = 0.5f; //only activate lower air decceleration if our velocity is lower than this*speed

    [SerializeField] Vector3 moveVector;
    Vector2 moveVector2 { get { return new Vector2(moveVector.x, moveVector.z); } }

    [SerializeField] float gravity = 1;

    [SerializeField] float lookSpeed = 0.02f;
    [SerializeField] bool doCameraPitch;

    //jump
    [SerializeField] float jumpForce;
    [SerializeField] bool isJump;
    [SerializeField] float releaseVelMult = 0.4f;

    [SerializeField] GroundCheck groundCheck;
    bool grounded {get{return groundCheck.grounded;}}

    //dash
    bool isDash { get { return dashTimer > 0; } }
    [SerializeField] float dashTime = 0.4f;
    [SerializeField] float dashTimer;
    float timeSinceDash;
    Vector4 dashDirection;
    float dashProgress { get { return 1 - dashTimer / dashTime; } }
    [SerializeField] float dashSpeed = 3f;
    [SerializeField] AnimationCurve dashSpeedCurve;
    [SerializeField] AnimationCurve dashGravCurve;
    [SerializeField] float afterDashAccelMult = 0.4f;
    [SerializeField] float afterDashAccelTime = 0.2f;
    bool isAfterDashAccel { get { return timeSinceDash < afterDashAccelTime; } }

    //swing
    [SerializeField] bool disableSwing;

    bool isSwingCharge;
    bool swingActive;
    bool swingCharged;
    float timeChargingSwing;
    [SerializeField] float swingChargeTime = 0.4f; //how long we need to charge to get a charged swing
    [SerializeField] float swingActiveTime = 0.2f;
    [SerializeField] ColliderS swingCollider;
    [SerializeField] float swingSpeed = 2;
    [SerializeField] float swingSpeedCharged = 3f;
    [SerializeField] float swingMass = 1;

    //anim
    [SerializeField] Animator animator;
    [SerializeField] int racketAnimLayer = 0;

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

        if (!isDash && input.shiftPress)
        {
            StartDash();
        }

        if (!disableSwing)
        {
            SwingFunc();
        }
        else
        {
            if (isSwingCharge || swingActive)
            {
                isSwingCharge = false;
                EndActiveSwing();
            }
        }
    }

    void SwingFunc()
    {
        if (input.leftClick) 
        {
            StartSwingCharge();
        }
        if (isSwingCharge)
        {
            timeChargingSwing += Time.deltaTime;

            if (timeChargingSwing > swingChargeTime) {
                swingCharged = true;
                animator.Play("ChargeCharged",racketAnimLayer);
            }

            if (!input.leftClickDown)
            {
                isSwingCharge = false;
                ReleaseSwing();
            }
        }
    }

    void FixedUpdate()
    {
        if (isDash && prevPosition != transform4.positionNorm)
        {
            dashDirection = new Rotor(prevPosition, transform4.positionNorm) * dashDirection;
            dashDirection = UFunc.ProjectToVectorNormal(dashDirection, transform4.yBasis);
            dashDirection = dashDirection.normalized;
        }
        UFunc.TickTimerFixed(ref dashTimer);

        if (!isDash) timeSinceDash += Time.fixedDeltaTime;

        rb.gravityScale = gravity;
        if (isDash)
        {
            rb.gravityScale *= dashGravCurve.Evaluate(dashProgress);
        }

        float yVel = rb.GetRelativeVelocity(1);

        if (grounded && yVel < 0.05f)
        {
            isJump = false;
        }

        if (isJump && !input.space && yVel > 0) //if space released
        {
            rb.SetRelativeVelocityY(yVel * releaseVelMult);
            isJump = false;
        }
        VelocityFunc();

        prevPosition = transform4.positionNorm;

        if (swingActive) CheckSwingCollider();
    }

    void VelocityFunc()
    {
        Vector2 relativeVel = rb.GetRelativeVelocityXZ();
        Vector2 newVel = new Vector2();

        Vector2 targetSpeed = moveVector2 * speed;
        if (!grounded) {
            targetSpeed = moveVector2 * Mathf.Max(speed, Vector2.Dot(moveVector2,relativeVel));
        }

        //all the acceleration mults
        float accel = acceleration;

        if (targetSpeed != Vector2.zero)
        {
            float oppositeAccelFactor = 0.5f * (1 - Vector2.Dot(relativeVel.normalized, (targetSpeed - relativeVel).normalized));
            accel *= 1 + opposeAccelMult * oppositeAccelFactor;
        }
        else
        {
            if (!grounded && relativeVel.magnitude < airDeccelerationVelocityThreshold * speed)
            {
                accel *= airDeccelerationMult;
            }
        }

        if (isAfterDashAccel) accel *= afterDashAccelMult;

        ///

        newVel = new Vector2(
            Mathf.MoveTowards(relativeVel.x, targetSpeed.x, accel * Time.fixedDeltaTime),
            Mathf.MoveTowards(relativeVel.y, targetSpeed.y, accel * Time.fixedDeltaTime)
        );
        
        if (isDash)
        {
            newVel = dashDirection;
            Vector3 relativeDashDir = transform4.RelativeDirectionTo(dashDirection);

            newVel = new Vector2(relativeDashDir.x,relativeDashDir.z) * dashSpeedCurve.Evaluate(dashProgress) * dashSpeed;
        }

        rb.SetRelativeVelocityX(newVel.x);
        rb.SetRelativeVelocityZ(newVel.y);
    }

    void StartDash()
    {
        dashTimer = dashTime;
        rb.SetVelocityTowards(new Vector4(0,1,0,0), 0);

        if (moveVector2 == Vector2.zero)
        {
            dashDirection = -transform4.zBasis;
        }
        else
        {
            dashDirection = transform4.xBasis*moveVector.x + transform4.zBasis*moveVector.z;
        }
    }

    void StartSwingCharge()
    {
        isSwingCharge = true;
        animator.Play("Charge",racketAnimLayer);

        timeChargingSwing = 0;
        swingCharged = false;
    }
    void ReleaseSwing()
    {
        isSwingCharge = false;
        animator.Play("Swing",racketAnimLayer);

        swingActive = true;

        swingCollider.gameObject.SetActive(true);

        Invoke("EndActiveSwing", swingActiveTime);
    }
    void EndActiveSwing()
    {
        swingCollider.gameObject.SetActive(false);

        swingActive = false;
        animator.Play("Hold",racketAnimLayer);
    }

    void CheckSwingCollider()
    {
        Vector4 attackDir = -camTransform.matrix.GetColumn(2);
        Vector4 attackPos = camTransform.positionNorm;

        float spd = swingSpeed;
        if (swingCharged) spd = swingSpeedCharged;

        foreach (ColliderS c in swingCollider.overlapColliders)
        {
            if (c.isTrigger) continue;

            Rigidbody4D colliderRB = c.rigidbody4;
            if (!colliderRB) continue;
            if (colliderRB == rb) continue;

            Vector4 rbAttackDir = new Rotor(attackPos, colliderRB.transform4.positionNorm) * attackDir;

            colliderRB.CollidePointMass(transform4.positionNorm, rbAttackDir*spd, swingMass);
            //colliderRB.SetVelocityTowards(rbAttackDir, spd);
        }
    }
}
