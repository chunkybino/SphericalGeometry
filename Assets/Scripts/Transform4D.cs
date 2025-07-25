using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.Events;

[ExecuteAlways]
public class Transform4D : MonoBehaviour
{
    [HideInInspector] public Transform4D parent;
    [HideInInspector] public List<Transform4D> children = new List<Transform4D>();

    public static float radius = 1;

    Transform4D()
    {
        m_matrix = Matrix4x4.identity;
        m_localMatrix = Matrix4x4.identity;
    }

    void OnEnable()
    {
        FindParent();
    }
    void OnDestroy()
    {
        parent?.children.Remove(this);
    }

    void FindParent()
    {
        Check(transform);
        if (parent == this) parent = null;
        m_matrix = parentMatrix * m_localMatrix;

        void Check(Transform t)
        {
            if (t.parent == null) {
                parent = null;
                return;
            }

            if (t.parent.TryGetComponent<Transform4D>(out Transform4D par)) 
            {
                parent?.children.Remove(this);
                parent = par;
                if (!parent.children.Contains(this)) parent.children.Add(this);
                return;
            }
            else
            {
                Check(t.parent);
            }
        }
    }

    [SerializeField] Matrix4x4 m_matrix;
    [HideInInspector] [SerializeField] Matrix4x4 m_localMatrix;

    Matrix4x4 parentMatrix {get{
        if (parent) return parent.matrix;
        return Matrix4x4.identity;
    }}

    public Matrix4x4 matrix {
        get {
            return m_matrix;
        }
        set {
            if (m_matrix == value) return;
            m_matrix = value;
            m_localMatrix = parentMatrix.transpose * m_matrix;
            MatrixUpdate();
        }
    }
    public Matrix4x4 localMatrix {
        get {
            return m_localMatrix;
        }
        set {
            if (m_localMatrix == value) return;
            m_localMatrix = value;
            m_matrix = parentMatrix * m_localMatrix;
            MatrixUpdate();
        }
    }

    [HideInInspector] public UnityEvent<Matrix4x4> onMatrixUpdate;
    void MatrixUpdate()
    {
        for (int i = children.Count-1; i >= 0; i--)
        {
            if (children[i] == null) {
                children.RemoveAt(i);
                continue;
            }
            if (children[i] == this) continue;
            children[i].RecompMatrix();
        }

        onMatrixUpdate?.Invoke(matrix);
    }
    public void RecompMatrix()
    {
        m_matrix = parentMatrix * m_localMatrix;
        MatrixUpdate();
    }

    public Vector4 position { get {
        return matrix.GetColumn(3) * radius;
    }}
    public Vector4 positionNorm { get {
        return matrix.GetColumn(3);
    }}
    public Vector4 localPosition { get {
        return localMatrix.GetColumn(3) * radius;
    }}
    public Vector4 localPositionNorm { get {
        return localMatrix.GetColumn(3);
    }}

    public Vector4 xBasis {get{return matrix.GetColumn(0);}}
    public Vector4 yBasis {get{return matrix.GetColumn(1);}}
    public Vector4 zBasis {get{return matrix.GetColumn(2);}}
    public Vector4 wBasis {get{return matrix.GetColumn(3);}}
    public Vector4 GetBasis(int i) {
        return matrix.GetColumn(i);
    }

    public Matrix4x4 lookMatrix {
        get {
            Matrix4x4 mat = matrix;
            mat.SetColumn(3, new Vector4(0,0,0,1));
            return mat;
        }
    }

    public Vector3 scale = new Vector3(1,1,1);

    //stereographically project the points into 3d space
    public Vector3 Sterographic()
    {
        return UFunc.SterographicProjection(position, radius);
    }

    [SerializeField] Vector3 sterographicPos;

    public bool lockSterographicPos = true;

    [HideInInspector] public UnityEvent<Matrix4x4> onLeftMult;
    [HideInInspector] public UnityEvent<Matrix4x4> onRightMult;

    void OnValidate()
    {
        UpdateSterographicPos();
    }

    public virtual void Update()
    {
        UpdateSterographicPos();
    }

    public void UpdateSterographicPos()
    {
        sterographicPos = Sterographic();
        if (lockSterographicPos) transform.position = sterographicPos;
    }

    //move relative to our orientation
    public void MoveRelative(Vector3 move)
    {
        Vector3 moveNormal = -move.normalized;
        Vector4 moveTarget = lookMatrix * new Vector4(moveNormal.x,moveNormal.y,moveNormal.z, 0);

        Matrix4x4 moveMatrix = UFunc.RotateTowardsMatrix(position, moveTarget, move.magnitude / radius);

        LeftMult(moveMatrix);
    }

    public void RotateRelativeXY(float angle) 
    {
        RightMult(UFunc.MatXYRot(angle));
    }
    public void RotateRelativeXZ(float angle) 
    {
        RightMult(UFunc.MatXZRot(angle));
    }
    public void RotateRelativeYZ(float angle) 
    {
        RightMult(UFunc.MatYZRot(angle));
    }

    public void LeftMult(Matrix4x4 mat)
    {
        matrix = mat * matrix;
        onLeftMult?.Invoke(mat);
    }
    public void RightMult(Matrix4x4 mat)
    {
        matrix = matrix * mat;
        onRightMult?.Invoke(mat);
    }

    void OnDrawGizmosSelected()
    {
        Color[] colors = {
            new Color(1,0,0,1),
            new Color(0,1,0,1),
            new Color(0,0,1,1),
            new Color(0.75f,0,0.75f,1)
        };

        for (int i = 0; i < 4; i++) {
            Gizmos.color = colors[i];
            Gizmos.DrawLine(Vector3.zero, UFunc.SterographicProjection(matrix.GetColumn(i), radius));
        }
    }
}
