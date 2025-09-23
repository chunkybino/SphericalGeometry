using UnityEngine;
using UnityEditor;

public class SceneCameraWindow : EditorWindow
{
    public static bool disableSphericalSceneCam;

    public static Matrix4x4 sceneCamMatrix = Matrix4x4.identity;

    public static float camSpeed = 0.5f;

    [MenuItem("Spherical/SceneCamera")]
    public static void ShowWindow()
    {
        GetWindow<SceneCameraWindow>("SceneCamera");
    }

    void OnGUI()
    {
        bool prevDisable = disableSphericalSceneCam;
        disableSphericalSceneCam = EditorGUILayout.Toggle("Disable Spherical Camera", disableSphericalSceneCam);
        if (!prevDisable && disableSphericalSceneCam)
        {
            sceneCamMatrix = Matrix4x4.identity;
            SceneView.lastActiveSceneView.camera.worldToCameraMatrix = sceneCamMatrix;
        }

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
