using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.Events;

[ExecuteAlways]
public class TransformH : MonoBehaviour
{
    [HideInInspector] public TransformH parent;
    [HideInInspector] public List<TransformH> children;

    void OnTransformParentChanged()
    {
        parent?.children.Remove(this);
        FindParent();
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

            if (t.parent.TryGetComponent<TransformH>(out TransformH par)) 
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
}
