using UnityEngine;
using UnityEngine.Rendering;

[ExecuteAlways]
public class LightS : MonoBehaviour
{
    LightHandlerS lightHandler;

    public Transform4D transform4;

    public LightData data;

    //int m_lightIndex;
    //public int lightIndex {get{return m_lightIndex;}set{m_lightIndex = value;}}

    public Color color;
    public float intensity;

    public bool doFalloff;
    //public float falloffStart;
    //public float falloffRange;

    public float falloffDegree = 2;

    public float ambience;

    public Vector3 direction = new Vector3(1,0,0);
    Vector4 direction4;
    [Range(0,180)] public float range = 180;
    [Range(0,180)] public float rangeFalloff = 180;

    public bool castShadows;
    bool m_castShadows;

    public bool dirty;

    public ComputeShader shadowMapShader;
    public RenderTexture shadowMap;

    public bool setShadow;

    public Camera shadowCam;

    public Material shadowCamMaterial;
    public Shader shadowCamShader;
    public Renderer4D[] shadowCamRenders = new Renderer4D[0];

    void OnEnable()
    {
        lightHandler = LightHandlerS.singleton;
        if (!transform4) transform4 = GetComponent<Transform4D>();

        lightHandler.AddLight(this);
    }
    void OnDisable()
    {
        lightHandler.RemoveLight(this);
    }

    void Update()
    {
        DrawShadowCam();

        if (setShadow) {
            //SetShadowMap();
        }

        if (m_castShadows != castShadows) {
            m_castShadows = castShadows;
            lightHandler.UpdateLightCastShadow();
        }

        direction4 = transform4.RelativeToWorld(direction.normalized);
        //rangeCos = Mathf.Cos(Mathf.Deg2Rad * range);
        //rangeFalloffCos = Mathf.Cos(Mathf.Deg2Rad * Mathf.Clamp(range+rangeFalloff,0,180));
        //if (rangeFalloffCos > rangeCos) rangeFalloffCos = rangeCos;

        if (
            data.position != transform4.positionNorm ||
            data.color != new Vector3(color.r,color.g,color.b) ||
            data.intensity != intensity ||
            //data.doFalloff != (doFalloff ? 1 : 0) ||
            //data.falloffStart != falloffStart ||
            //data.falloffRange != falloffRange ||
            data.falloffDegree != falloffDegree ||
            data.ambience != ambience ||
            data.direction != direction4 ||
            data.rangeAngle != range ||
            data.rangeFalloffAngleMult != rangeFalloff
            ) {
            dirty = true;
        }

        data.position = transform4.positionNorm;
        data.color = new Vector3(color.r,color.g,color.b);
        data.intensity = intensity;

        //data.doFalloff = doFalloff ? 1 : 0;
        /*
        data.falloffStart = falloffStart;
        if (falloffRange != 0) {
            data.falloffRange = 1/falloffRange;
        } else {
            data.falloffRange = 9999;
        }
        */

        data.falloffDegree = doFalloff ? falloffDegree : 0;

        data.ambience = ambience;

        data.direction = direction4;
        data.rangeAngle = Mathf.Deg2Rad*range;
        if (rangeFalloff != 0) {
            data.rangeFalloffAngleMult = 1/(Mathf.Deg2Rad*rangeFalloff);
        } else {
            data.rangeFalloffAngleMult = 9999;
        }
    }

    void SetShadowMap()
    {
        shadowMap = new RenderTexture(64,64,6);
        shadowMap.enableRandomWrite = true;
        shadowMap.Create();

        shadowMapShader.SetTexture(0, "Result", shadowMap);
    }

    public void OnPostRender() 
    {
        return;
        for (int i = 0; i < shadowCamRenders.Length; i++)
        {
            Renderer4D r = shadowCamRenders[i];

            MaterialPropertyBlock prop = new MaterialPropertyBlock();
            Matrix4x4 transformMatrix = r.transform4.matrix.inverse;
            prop.SetVector("_MatC0", transformMatrix.GetColumn(0));
            prop.SetVector("_MatC1", transformMatrix.GetColumn(1));
            prop.SetVector("_MatC2", transformMatrix.GetColumn(2));
            prop.SetVector("_MatC3", transformMatrix.GetColumn(3));
            prop.SetVector("_Scale", new Vector4(r.transformScale.x,r.transformScale.y,r.transformScale.z,0));
            prop.SetFloat("_DoV4", r.doVertex4 ? 1f : 0f);

            print(i);
            print(transformMatrix.GetColumn(3));

            RenderParams rparams = new RenderParams();

            rparams.camera = shadowCam;
            rparams.material = shadowCamMaterial;
            rparams.matProps = prop;

            Graphics.RenderMesh(rparams, r.filter.sharedMesh, 0, Matrix4x4.identity);
            //Graphics.DrawMesh(r.filter.sharedMesh, r.transform4.matrix, shadowCamMaterial, 0, shadowCam, 0, prop);
        }
    }

    void DrawShadowCam()
    {

    }
    void LateUpdate()
    {
        if (!shadowCam) return;

        //shadowCam.depthTextureMode = DepthTextureMode.Depth;

        //shadowCam.transform.position = Vector3.zero;
        shadowCam.worldToCameraMatrix = transform4.matrix.inverse;

        shadowCam.Render();

        //shadowCam.Render();
        //shadowCam.SetReplacementShader(shadowCamShader, "");
        //shadowCam.RenderWithShader(shadowCamShader, "");
    }
}
