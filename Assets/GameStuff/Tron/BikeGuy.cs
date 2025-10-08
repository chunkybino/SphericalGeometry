using UnityEngine;

public class BikeGuy : MonoBehaviour
{
    TronGameManager gameManager;
    BikeTrailHandler trailHandler;

    public Transform4D transform4;
    [SerializeField] Rigidbody4D rb;

    public Transform4D camTransform;

    [SerializeField] PlayerInput input;

    public float speed = 1;
    public float turnSpeed = 1;
    public float spinSpeed = 0.75f;


    [SerializeField] Vector3 turnInput;

    [SerializeField] Vector2 turnMomentum;
    [SerializeField] float spinMomentum;
    [SerializeField] float turnAcceleration = 12;
    [SerializeField] float spinAcceleration = 12;

    public bool gameActive;

    public int bikeIndex;
    int m_lengthIncreaseGet;
    public int lengthIncreaseGet {
        get
        {
            return m_lengthIncreaseGet;
        }
        set
        {
            m_lengthIncreaseGet = value;
            trailHandler.UpdateBike(bikeIndex);
        }
    }

    public int controlIndex;
    bool up { get
        {
            switch (controlIndex)
            {
                case 0:
                    return input.forward;
                case 1:
                    return input.upArrow;
            }
            return false;
        }
    }
    bool down { get
        {
            switch (controlIndex)
            {
                case 0:
                    return input.backward;
                case 1:
                    return input.downArrow;
            }
            return false;
        }
    }
    bool left { get
        {
            switch (controlIndex)
            {
                case 0:
                    return input.left;
                case 1:
                    return input.leftArrow;
            }
            return false;
        }
    }
    bool right { get
        {
            switch (controlIndex)
            {
                case 0:
                    return input.right;
                case 1:
                    return input.rightArrow;
            }
            return false;
        }
    }

    bool twistLeft { get
        {
            return input.leftArrow;
        }
    }
    bool twistRight { get
        {
            return input.rightArrow;
        }
    }

    public float wide = 0.05f;
    public float radius = 0.01f;

    public Vector4 widePoint1 { get { return UFunc.Slerp4Angle(transform4.positionNorm, transform4.xBasis, wide); } }
    public Vector4 widePoint2 {get{return UFunc.Slerp4Angle(transform4.positionNorm, transform4.xBasis, -wide);}}


    void OnEnable()
    {
        gameManager = TronGameManager.singleton;
        trailHandler = BikeTrailHandler.singleton;
    }

    void Update()
    {
        if (!gameActive) return;

        rb.velocity = -transform4.zBasis * speed * gameManager.moveSpeed;

        turnInput = Vector3.zero;
        if (left) turnInput.x--;
        if (right) turnInput.x++;
        if (down) turnInput.y--;
        if (up) turnInput.y++;

        if (twistLeft) turnInput.z--;
        if (twistRight) turnInput.z++;

        turnMomentum += ((Vector2)turnInput - turnMomentum/turnSpeed) * turnAcceleration * Time.deltaTime;
        
        turnMomentum.x = Mathf.Clamp(turnMomentum.x, -turnSpeed, turnSpeed);
        turnMomentum.y = Mathf.Clamp(turnMomentum.y, -turnSpeed, turnSpeed);

        spinMomentum += (turnInput.z - spinMomentum/spinSpeed) * spinAcceleration * Time.deltaTime;

        spinMomentum = Mathf.Clamp(spinMomentum, -spinSpeed, spinSpeed);

        Vector4 newPos = UFunc.Slerp4Angle(new Vector4(0, 0, 1, 0), new Vector4(-turnMomentum.x, -turnMomentum.y, 0, 0).normalized, turnMomentum.magnitude * Time.deltaTime * gameManager.moveSpeed);
        Matrix4x4 rotMat = UFunc.MatrixBiReflect(new Vector4(0, 0, 1, 0), newPos);
        Matrix4x4 spinMat = UFunc.MatXYRot(spinMomentum * Time.deltaTime);

        transform4.matrix = transform4.matrix * rotMat * spinMat;
    }

    public void BlowUp()
    {
        rb.velocity = Vector4.zero;

        gameObject.SetActive(false);
    }
}
