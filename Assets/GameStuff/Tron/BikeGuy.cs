using UnityEngine;

public class BikeGuy : MonoBehaviour
{
    BikeTrailHandler trailHandler;

    public Transform4D transform4;
    [SerializeField] Rigidbody4D rb;

    public Transform4D camTransform;

    [SerializeField] PlayerInput input;

    [SerializeField] float speed = 1f;
    [SerializeField] float turnSpeed = 1;


    [SerializeField] Vector3 turnInput;
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

    public float wide = 0.05f;

    public Vector4 widePoint1 {get{return UFunc.Slerp4Angle(transform4.positionNorm, transform4.xBasis, wide);}}
    public Vector4 widePoint2 {get{return UFunc.Slerp4Angle(transform4.positionNorm, transform4.xBasis, -wide);}}


    void OnEnable()
    {
        trailHandler = BikeTrailHandler.singleton;
    }

    void Update()
    {
        if (!gameActive) return;

        rb.velocity = -transform4.zBasis * speed;

        turnInput = Vector2.zero;
        if (left) turnInput.x--;
        if (right) turnInput.x++;
        if (down) turnInput.y--;
        if (up) turnInput.y++;

        Vector4 newPos = UFunc.Slerp4Angle(new Vector4(0, 0, 1, 0), new Vector4(-turnInput.x, -turnInput.y, 0, 0), turnSpeed * Time.deltaTime);
        Matrix4x4 rotMat = UFunc.MatrixBiReflect(new Vector4(0, 0, 1, 0), newPos);

        transform4.matrix = transform4.matrix * rotMat;
    }

    public void BlowUp()
    {
        rb.velocity = Vector4.zero;

        gameObject.SetActive(false);
    }
}
