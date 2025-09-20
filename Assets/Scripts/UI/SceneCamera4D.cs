using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;

[ExecuteAlways]
public class SceneCamera4D : MonoBehaviour
{
    //private static readonly int _isSceneCamID = Shader.PropertyToID("_IsSceneCam");

    [SerializeField] Matrix4x4 sceneCamMatrix;

    [SerializeField] float camSpeed = 0.5f;

    void OnEnable()
    {
        RenderPipelineManager.beginCameraRendering += PreRender;
    }
    void OnDisable()
    {
        RenderPipelineManager.beginCameraRendering -= PreRender;
    }

    void PreRender(ScriptableRenderContext context, Camera cam)
    {
        if (cam != SceneView.lastActiveSceneView.camera) return;

        UpdateSceneViewMatrix();
    }

    void UpdateSceneViewMatrix()
    {
        SceneView sceneView = SceneView.lastActiveSceneView;

        Vector3 scenePivot = sceneView.pivot * camSpeed;
        scenePivot.z *= -1;
        Quaternion sceneRot = sceneView.rotation;

        //reset pos
        sceneView.pivot = Vector3.zero;
        sceneView.rotation = Quaternion.identity;

        Vector4 xBasis = sceneRot * new Vector3(1, 0, 0);
        Vector4 yBasis = sceneRot * new Vector3(0, 1, 0);
        Vector4 zBasis = sceneRot * new Vector3(0, 0, 1);

        Matrix4x4 rotMatrix = new Matrix4x4(xBasis,yBasis,zBasis,new Vector4(0,0,0,1));

        Vector4 camTransform4 = new Vector4(0, 0, 0, 1);
        camTransform4 = UFunc.Slerp4Angle(new Vector4(0, 0, 0, 1), scenePivot, scenePivot.magnitude);

        Matrix4x4 moveMatrix = UFunc.MatrixBiReflect(new Vector4(0, 0, 0, 1), camTransform4);

        sceneCamMatrix = sceneCamMatrix*(moveMatrix*rotMatrix);

        SceneView.lastActiveSceneView.camera.worldToCameraMatrix = sceneCamMatrix.inverse;
    }
}
