using UnityEngine;
using System.Collections.Generic;

[ExecuteAlways]
public class LightHandlerS : MonoBehaviour
{
    public static LightHandlerS singleton;

    public List<LightS> lights = new List<LightS>();
    LightData[] lightDataSend;
    public int lightDatasCount;

    [SerializeField] ComputeBuffer lightBuffer;
    [SerializeField] ComputeBuffer lightCountBuffer;

    [SerializeField] Vector4[] shadowSend;

    [SerializeField] ComputeBuffer shadowBuffer;
    [SerializeField] ComputeBuffer shadowCountBuffer;

    public List<Renderer4D> staticShadowRenderers = new List<Renderer4D>();

    public bool disableShadows;

    public bool updateFullBuffer;

    void Awake()
    {
        CheckSingleton();
    }

    void OnEnable()
    {
        CheckSingleton();

        Dispose();

        SetStaticShadowBuffer();

        UpdateBufferSendData();
        SetLightBuffer();
    }

    void CheckSingleton()
    {
        if (!singleton) {
            singleton = this;
        } else if (singleton != this) {
            Destroy(this);
        }
    }

    void OnDestroy()
    {
        Dispose();
    }
    void OnDisable()
    {
        Dispose();
    }

    void OnValidate()
    {
        if (updateFullBuffer) {
            updateFullBuffer = false;
            SetLightBuffer();
            SetStaticShadowBuffer();
        }
    }

    void Update()
    {
        UpdateBufferSendData();

        int startIndex = 0;
        int bufferCount = 0;
        for (int i = 0; i < lights.Count; i++)
        {
            if (lights[i].dirty)
            {
                if (bufferCount == 0) {
                    startIndex = i;
                }
                bufferCount++;
                lights[i].dirty = false;
            }
            else
            {
                if (bufferCount > 0) {
                    lightBuffer.SetData(lightDataSend, startIndex, startIndex, bufferCount);
                    bufferCount = 0;
                }
            }
        }
        if (bufferCount > 0) {
            lightBuffer.SetData(lightDataSend, startIndex, startIndex, bufferCount);
        }

        if (disableShadows)
        {
            if (shadowSend.Length > 0)
            {
                shadowBuffer?.Release();
                shadowCountBuffer?.Release();
                shadowSend = new Vector4[0];
            }
        }
        else
        {
            if (shadowSend.Length == 0 && staticShadowRenderers.Count > 0)
            {
                SetStaticShadowBuffer();
            }
        }
        
    }

    void UpdateFullBuffer()
    {
        UpdateBufferSendData();

        if (lightBuffer == null || lights.Count != lightDatasCount)
        {
            SetLightBuffer();
        }
    }

    void UpdateBufferSendData()
    {
        lightDataSend = new LightData[lights.Count];
        for (int i = 0; i < lights.Count; i++)
        {
            lightDataSend[i] = lights[i].data;
        }
    }   

    void SetLightBuffer()
    {
        lightDatasCount = lights.Count;
        
        if (lightDatasCount > 0)
        {
            lightBuffer = new ComputeBuffer(lightDatasCount, System.Runtime.InteropServices.Marshal.SizeOf(typeof(LightData))); //System.Runtime.InteropServices.Marshal.SizeOf(typeof(LightData)));
            lightBuffer.SetData(lightDataSend);
            Shader.SetGlobalBuffer("_LightData", lightBuffer);
        }

        lightCountBuffer = new ComputeBuffer(1, sizeof(int));
        lightCountBuffer.SetData(new int[] {lightDatasCount});
        Shader.SetGlobalBuffer("_LightCount", lightCountBuffer);
    }

    public void AddLight(LightS light)
    {
        if (lights.Contains(light)) return;

        lights.Add(light);

        UpdateFullBuffer();
    }
    public void RemoveLight(LightS light)
    {
        if (!lights.Contains(light)) return;

        lights.Remove(light);

        UpdateFullBuffer();
    }

    void Dispose()
    {
        print("dispose");

        lightBuffer?.Release();
        lightCountBuffer?.Release();

        shadowBuffer?.Release();
        shadowCountBuffer?.Release();
    }

    void SetStaticShadowBuffer()
    {
        if (disableShadows) return;

        int totalShadowTriLength = 0;
        for (int i = 0; i < staticShadowRenderers.Count; i++)
        {
            totalShadowTriLength += staticShadowRenderers[i].GetTri().Length/3;
        }

        Vector4[] shadowSend = new Vector4[totalShadowTriLength*4];

        int triPlaceIndex = 0;

        for (int j = 0; j < staticShadowRenderers.Count; j++)
        {
            Renderer4D ren = staticShadowRenderers[j];

            int[] shadowTri = ren.GetTri();
            Vector4[] shadowVertex4 = new Vector4[0];
            int shadowTriCount = shadowTri.Length/3;

            Matrix4x4 mat = ren.transform4.matrix;

            if (!ren.doVertex4)
            {
                Vector3[] shadowVertex = ren.GetVertex3();
                shadowVertex4 = new Vector4[shadowVertex.Length];

                for (int i = 0; i < shadowVertex.Length; i++)
                {
                    Vector3 p = Vector3.Scale(ren.transformScale, shadowVertex[i]);
                    shadowVertex4[i] = mat * UFunc.SterographicInverse(p,1);
                }
            }
            else
            {
                shadowVertex4 =  ren.GetVertex4();
            }

            for (int i = 0; i < shadowTriCount; i++)
            {
                AddTri(shadowVertex4[shadowTri[3*i+0]], shadowVertex4[shadowTri[3*i+1]], shadowVertex4[shadowTri[3*i+2]], triPlaceIndex);
                triPlaceIndex++;
            }
        }

        this.shadowSend = shadowSend;

        shadowBuffer = new ComputeBuffer(totalShadowTriLength, sizeof(float) * 16);
        shadowBuffer.SetData(shadowSend);
        Shader.SetGlobalBuffer("_ShadowData", shadowBuffer);

        shadowCountBuffer = new ComputeBuffer(1, sizeof(int));
        shadowCountBuffer.SetData(new int[] {totalShadowTriLength});
        Shader.SetGlobalBuffer("_ShadowCount", shadowCountBuffer);

        void AddTri(Vector4 v1, Vector4 v2, Vector4 v3, int i)
        {
            shadowSend[4*i + 0] = UFunc.HyperCross(v1,v2,v3).normalized;
            shadowSend[4*i + 1] = UFunc.HyperCross(v1,v2,shadowSend[4*i]).normalized;
            shadowSend[4*i + 2] = UFunc.HyperCross(v2,v3,shadowSend[4*i]).normalized;
            shadowSend[4*i + 3] = UFunc.HyperCross(v3,v1,shadowSend[4*i]).normalized;
        }
    }

    public void AddStaticShadow(Renderer4D ren)
    {
        if (staticShadowRenderers.Contains(ren)) return;

        staticShadowRenderers.Add(ren);

        SetStaticShadowBuffer();
    }
    public void RemoveStaticShadow(Renderer4D ren)
    {
        if (!staticShadowRenderers.Contains(ren)) return;

        staticShadowRenderers.Remove(ren);

        SetStaticShadowBuffer();
    }
}
