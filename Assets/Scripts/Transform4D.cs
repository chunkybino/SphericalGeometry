using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.Events;

[ExecuteAlways]
public class Transform4D : MonoBehaviour
{
    public static float radius = 1;

    public Transform4D()
    {
        matrix = Matrix4x4.identity;
    }

    public Matrix4x4 matrix;
    public Matrix4x4 GetMatrix() {return matrix;}

    public Vector4 position {
        get {
            return matrix.GetColumn(3) * radius;
        }
        set {
            matrix.SetColumn(3,value);
        }
    }

    public Vector4 xBasis {get{return matrix.GetColumn(0);}}
    public Vector4 yBasis {get{return matrix.GetColumn(1);}}
    public Vector4 zBasis {get{return matrix.GetColumn(2);}}
    public Vector4 wBasis {get{return matrix.GetColumn(3);}}

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

    public UnityEvent onMove;

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

        matrix = moveMatrix * matrix;

        onMove?.Invoke();
    }

    public void RotateRelativeXY(float angle) 
    {
        matrix = matrix * UFunc.MatXYRot(angle);
    }
    public void RotateRelativeXZ(float angle) 
    {
        matrix = matrix * UFunc.MatXZRot(angle);
    }
    public void RotateRelativeYZ(float angle) 
    {
        matrix = matrix * UFunc.MatYZRot(angle);
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
