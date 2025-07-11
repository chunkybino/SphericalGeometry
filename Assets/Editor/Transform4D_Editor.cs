using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

[CustomEditor(typeof(Transform4D))]
public class Transform4D_Editor : Editor
{
    Transform4D transform;
    Matrix4x4 mat;
    Matrix4x4 localMat;

    Vector3 moveDirection;
    float rotateAmount = Mathf.PI/4;

    bool showButtons;

    public override void OnInspectorGUI()
    {
        transform = (Transform4D)target;

        mat = transform.matrix;
        localMat = transform.localMatrix;

        base.OnInspectorGUI();

        EditorGUILayout.ObjectField("Parent", transform.parent, typeof(Transform4D), false);

        DisplayMatrix();

        Transform4D.radius = Mathf.Max(EditorGUILayout.FloatField("World Radius", Transform4D.radius), 0.01f);

        showButtons = EditorGUILayout.Toggle("Show Movement Buttons", showButtons);

        if (showButtons) DisplayButtons();
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
        moveDirection = EditorGUILayout.Vector3Field("move direction", moveDirection);
        rotateAmount = EditorGUILayout.FloatField("rotate amount", rotateAmount);

        bool moved = false;

        if (GUILayout.Button("Move")) {
            transform.MoveRelative(moveDirection);
            moved = true;
        }

        if (GUILayout.Button("Reset")) {
            transform.matrix = Matrix4x4.identity;
            moved = true;
        }

        if (GUILayout.Button("Transpose")) {
            transform.matrix = transform.matrix.transpose;
            moved = true;
        }

        if (GUILayout.Button("Rot XY")) {
            transform.matrix = UFunc.MatXYRot(rotateAmount) * transform.matrix;
            moved = true;
        }
        if (GUILayout.Button("Rot XZ")) {
            transform.matrix = UFunc.MatXZRot(rotateAmount) * transform.matrix;
            moved = true;
        }
        if (GUILayout.Button("Rot YZ")) {
            transform.matrix = UFunc.MatYZRot(rotateAmount) * transform.matrix;
            moved = true;
        }
        if (GUILayout.Button("Rot XW")) {
            transform.matrix = UFunc.MatXWRot(rotateAmount) * transform.matrix;
            moved = true;
        }
        if (GUILayout.Button("Rot YW")) {
            transform.matrix = UFunc.MatYWRot(rotateAmount) * transform.matrix;
            moved = true;
        }
        if (GUILayout.Button("Rot ZW")) {
            transform.matrix = UFunc.MatZWRot(rotateAmount) * transform.matrix;
            moved = true;
        }

        if (moved) {
            transform.UpdateSterographicPos();
            UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
        }
    }
}
