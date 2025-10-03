using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public bool active;
    public Transform4D followTransform;

    [SerializeField] Transform4D transform4;

    public Camera4D camera;
    public Camera camObj;

    void Update()
    {
        if (!active || !followTransform || !transform4) return;

        transform4.matrix = followTransform.matrix;
    }
}
