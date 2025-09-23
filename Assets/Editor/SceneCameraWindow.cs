using UnityEngine;
using UnityEditor;

public class SceneCameraWindow : EditorWindow
{
    public static Matrix4x4 sceneCamMatrix = Matrix4x4.identity;

    public static float camSpeed = 0.5f;

    [MenuItem("Spherical/SceneCamera")]
    public static void ShowWindow()
    {
        GetWindow<SceneCameraWindow>("SceneCamera");
    }

    void OnGUI()
    {
        camSpeed = EditorGUILayout.FloatField("Camera Speed", camSpeed);
        DisplayMatrix();
    }

    void DisplayMatrix()
    {
        EditorGUILayout.BeginVertical();

        EditorGUILayout.LabelField("Camera Matrix");
        DoMatrix(sceneCamMatrix);

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
}
