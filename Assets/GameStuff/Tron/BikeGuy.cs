using UnityEngine;

public class BikeGuy : MonoBehaviour
{
    [SerializeField] Transform4D transform4;
    [SerializeField] Rigidbody4D rb;
    [SerializeField] new Camera4D camera;
    Transform4D camTransform { get { return camera.transform4; } }

    [SerializeField] PlayerInput input;

    [SerializeField] float speed = 1f;
    [SerializeField] float turnSpeed = 1;


    [SerializeField] Vector3 turnInput;

    [SerializeField] LineRendererS lineRen;
    [SerializeField] float lineFrameInterval = 0.1f;
    float lineFrameTimer;

    [SerializeField] float lineTime = 10;

    void Update()
    {
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

    void FixedUpdate()
    {
        lineFrameTimer -= Time.fixedDeltaTime;
        if (lineFrameTimer <= 0)
        {
            lineFrameTimer = lineFrameInterval;

            if (lineRen)
            {
                lineRen.AddPos(transform4.positionNorm, transform4.yBasis);

                if (lineRen.positions.Count > lineTime / lineFrameInterval) lineRen.RemovePos();
            }
        }
    }
}
