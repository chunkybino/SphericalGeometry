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


    void OnEnable()
    {
        trailHandler = BikeTrailHandler.singleton;
    }

    void Update()
    {
        if (!gameActive) return;

        rb.velocity = -transform4.zBasis * speed;

        turnInput = Vector2.zero;
        if (input.left) turnInput.x--;
        if (input.right) turnInput.x++;
        if (input.backward) turnInput.y--;
        if (input.forward) turnInput.y++;

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
