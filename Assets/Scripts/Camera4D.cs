using UnityEngine;

[ExecuteAlways]
public class Camera4D : MonoBehaviour
{
    new public Camera camera;
    public Transform4D transform4;

    public Matrix4x4 viewMatrix;

    public float pitchAngle;

    void Awake()
    {
        if (!camera) camera = GetComponent<Camera>();
        if (!transform4) transform4 = GetComponent<Transform4D>();
    }

    void Update()
    {
        if (transform4 == null) return;

        transform4.localMatrix = UFunc.MatZYRot(pitchAngle);

        viewMatrix = transform4.matrix; //* UFunc.MatZYRot(pitchAngle);
        //viewMatrix.SetRow(2, viewMatrix.GetRow(2)*-1);
        camera.worldToCameraMatrix = viewMatrix.inverse;
    }
}
