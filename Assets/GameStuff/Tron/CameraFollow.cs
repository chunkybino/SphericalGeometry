using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public bool active;
    public Transform4D followTransform;

    [SerializeField] Transform4D transform4;

    [SerializeField] Camera4D camera;
    [SerializeField] Camera camObj;

    void Update()
    {
        if (!active || !followTransform || !transform4) return;

        transform4.matrix = followTransform.matrix;
    }
}
