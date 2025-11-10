using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

[CustomEditor(typeof(TransformH))]
public class TransformH_Editor : Editor
{
    TransformH transform;
    Matrix4x4 mat;
    Matrix4x4 localMat;

    Vector3 moveDirection;
    Vector3 rotateDirection;

    float rotateAmount = Mathf.PI/4;

    bool showButtons;

    public override void OnInspectorGUI()
    {
        transform = (TransformH)target;

        mat = transform.matrix;
        localMat = transform.localMatrix;

        base.OnInspectorGUI();

        EditorGUILayout.ObjectField("Parent", transform.parent, typeof(TransformH), false);

        DisplayMatrix();
    }

    public void OnEnable()
    {
        transform = (TransformH)target;    

        transform.transform.position = Vector3.zero;
        transform.transform.eulerAngles = Vector3.zero;
    }

    void DisplayMatrix()
    {
        EditorGUILayout.BeginVertical();

        EditorGUILayout.LabelField("World Matrix");
        DoMatrix(mat);

        EditorGUILayout.LabelField("Local Matrix");
        DoMatrix(localMat);

        EditorGUILayout.EndVertical();

        void DoMatrix(Matrix4x4 m) {
            for (int j = 0; j < 4; j++)
            {
                EditorGUILayout.LabelField(
                    GetNum(m[j,0]) + "    " +
                    GetNum(m[j,1]) + "    " +
                    GetNum(m[j,2]) + "    " +
                    GetNum(m[j,3])
                );
            }
        }

        string GetNum(float inNum)
        {
            float new0 = UFunc.RoundDigit(inNum, 0);
            float new1 = UFunc.RoundDigit(inNum, 1);
            float new2 = UFunc.RoundDigit(inNum, 2);

            if (new1 == new2) {
                if (new1 == new0) return new0.ToString() + ".00";
                return new1.ToString() + "0";
            }
            return new2.ToString();
        }
    }

    void DisplayButtons()
    {
        bool moved = false;

        moveDirection = EditorGUILayout.Vector3Field("move direction", moveDirection);
        rotateDirection = EditorGUILayout.Vector3Field("rotate direction", rotateDirection);

        if (moved) {
            UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
            EditorUtility.SetDirty(transform);
        }
    }
}
