using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

[CustomEditor(typeof(Transform4D))]
public class Transform4D_Editor : Editor
{
    Vector3 moveDirection;
    float rotateAmount = Mathf.PI/4;

    public override void OnInspectorGUI()
    {
        Transform4D transform = (Transform4D)target;

        base.OnInspectorGUI();

        moveDirection = EditorGUILayout.Vector3Field("move direction", moveDirection);
        rotateAmount = EditorGUILayout.FloatField("rotate amount", rotateAmount);

        Transform4D.radius = EditorGUILayout.FloatField("World Radius", Transform4D.radius);

        Matrix4x4 mat = transform.matrix;

        //matrix display
        EditorGUILayout.BeginVertical();
        for (int j = 0; j < 4; j++)
        {
            //EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(
                GetNum(mat[0,j]) + "    " +
                GetNum(mat[1,j]) + "    " +
                GetNum(mat[2,j]) + "    " +
                GetNum(mat[3,j])
            );

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
            //EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndVertical();


        bool moved = false;

        if (GUILayout.Button("Move")) {
            transform.Move(moveDirection);
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
        if (GUILayout.Button("Rot XY 2 ")) {
            transform.matrix = transform.matrix * UFunc.MatXYRot(Mathf.PI/32);
            moved = true;
        }
        if (GUILayout.Button("Rot XZ")) {
            transform.matrix = UFunc.MatXZRot(rotateAmount) * transform.matrix;
            moved = true;
        }
        if (GUILayout.Button("Rot XZ 2 ")) {
            transform.matrix = transform.matrix * UFunc.MatXZRot(Mathf.PI/32);
            moved = true;
        }
        if (GUILayout.Button("Rot YZ")) {
            transform.matrix = UFunc.MatYZRot(rotateAmount) * transform.matrix;
            moved = true;
        }
        if (GUILayout.Button("Rot YZ 2 ")) {
            transform.matrix = transform.matrix * UFunc.MatYZRot(Mathf.PI/32);
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
