using UnityEngine;

[ExecuteAlways]
public class TransformConstraint4D : MonoBehaviour
{
    public bool active = true;
    public Transform4D transform4;
    public Transform4D followTransform; //copy from this transform
    bool transformValid;

    void OnEnable()
    {
        if (!transform4) transform4 = GetComponent<Transform4D>();
        if (followTransform) followTransform.onMatrixUpdate.AddListener(MatrixUpdate);
    }
    void OnDisable()
    {
        if (followTransform) followTransform.onMatrixUpdate.RemoveListener(MatrixUpdate);
    }
    void OnValidate()
    {
        if (followTransform != null)
        {
            if (!transformValid) {
                transformValid = true;
                followTransform.onMatrixUpdate.AddListener(MatrixUpdate);
            }
        }
        else
        {
            transformValid = false;
        }
    }

    void MatrixUpdate(Matrix4x4 mat)
    {
        if (!transform4) return;

        transform4.matrix = followTransform.matrix;
    }
}
