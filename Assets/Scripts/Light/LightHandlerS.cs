using UnityEngine;
using System.Collections.Generic;

[ExecuteAlways]
public class LightHandlerS : MonoBehaviour
{
    public static LightHandlerS singleton;

    public List<LightS> lights = new List<LightS>();
    public List<LightS> noShadowLights = new List<LightS>();
    public List<LightS> shadowLights = new List<LightS>();

    LightData[] lightDataSend;
    LightData[] lightDataSendNoShadow;
    LightData[] lightDataSendShadow;

    [SerializeField] int lightDatasCount;
    [SerializeField] int noShadowlightDatasCount;
    [SerializeField] int shadowLightDatasCount;

    [SerializeField] ComputeBuffer lightBuffer;
    [SerializeField] ComputeBuffer shadowLightBuffer;
    [SerializeField] ComputeBuffer lightCountBuffer;

    [SerializeField] Vector4[] shadowSend;

    [SerializeField] ComputeBuffer shadowBuffer;
    [SerializeField] ComputeBuffer shadowCountBuffer;

    [SerializeField] ComputeBuffer sphereShadowBuffer;

    public List<Renderer4D> staticShadowRenderers = new List<Renderer4D>();
    public List<Renderer4D> sphereShadowRenderers = new List<Renderer4D>();

    public bool disableShadows;
    public int castShadowLevel;

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

        //UpdateBufferSendData();
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

        UpdateBuf(noShadowLights, lightBuffer, lightDataSendNoShadow);
        UpdateBuf(shadowLights, shadowLightBuffer, lightDataSendShadow);

        //SetLightBuffer();

        void UpdateBuf(List<LightS> light, ComputeBuffer buff, LightData[] send)
        {
            int startIndex = 0;
            int bufferCount = 0;
            for (int i = 0; i < light.Count; i++)
            {
                if (light[i].dirty)
                {
                    if (bufferCount == 0) {
                        startIndex = i;
                    }
                    bufferCount++;
                    light[i].dirty = false;
                }
                else
                {
                    if (bufferCount > 0) {
                        buff.SetData(send, startIndex, startIndex, bufferCount);
                        bufferCount = 0;
                    }
                }
            }
            if (bufferCount > 0) {
                buff.SetData(send, startIndex, startIndex, bufferCount);
            }
        }

        if (disableShadows)
        {
            if (shadowSend.Length > 0)
            {
                shadowBuffer?.Release();
                shadowCountBuffer?.Release();
                sphereShadowBuffer?.Release();
                shadowSend = new Vector4[0];
            }
        }
        else
        {
            SetStaticShadowBuffer();
            /*
            if (shadowSend.Length == 0 && staticShadowRenderers.Count > 0)
            {
                SetStaticShadowBuffer();
            }
            */
        }
        
    }

    void UpdateFullBuffer()
    {
        UpdateBufferSendData();

        if (lightBuffer == null || lights.Count != lightDatasCount || noShadowLights.Count != noShadowlightDatasCount || shadowLights.Count != shadowLightDatasCount)
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

        lightDataSendNoShadow = new LightData[noShadowLights.Count];
        for (int i = 0; i < noShadowLights.Count; i++)
        {
            lightDataSendNoShadow[i] = noShadowLights[i].data;
        }

        lightDataSendShadow = new LightData[shadowLights.Count];
        for (int i = 0; i < shadowLights.Count; i++)
        {
            lightDataSendShadow[i] = shadowLights[i].data;
        }
    }   

    void SetLightBuffer()
    {
        noShadowLights.Clear();
        shadowLights.Clear();
        for (int i = 0; i < lights.Count; i++)
        {
            if (!lights[i].castShadows) {
                noShadowLights.Add(lights[i]);
            } else {
                shadowLights.Add(lights[i]);
            }
        }

        lightDatasCount = lights.Count;
        noShadowlightDatasCount = noShadowLights.Count;
        shadowLightDatasCount = shadowLights.Count;

        UpdateBufferSendData();
        
        if (noShadowlightDatasCount > 0)
        {
            lightBuffer = new ComputeBuffer(noShadowlightDatasCount, System.Runtime.InteropServices.Marshal.SizeOf(typeof(LightData))); //System.Runtime.InteropServices.Marshal.SizeOf(typeof(LightData)));
            lightBuffer.SetData(lightDataSendNoShadow);
            Shader.SetGlobalBuffer("_LightDataNoShadow", lightBuffer);
        }
        if (shadowLightDatasCount > 0)
        {
            shadowLightBuffer = new ComputeBuffer(shadowLightDatasCount, System.Runtime.InteropServices.Marshal.SizeOf(typeof(LightData))); //System.Runtime.InteropServices.Marshal.SizeOf(typeof(LightData)));
            shadowLightBuffer.SetData(lightDataSendShadow);
            Shader.SetGlobalBuffer("_LightDataShadow", shadowLightBuffer);
        }

        lightCountBuffer = new ComputeBuffer(2, sizeof(int));
        lightCountBuffer.SetData(new int[] {lightDatasCount,shadowLightDatasCount});
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
    public void UpdateLightCastShadow()
    {
        SetLightBuffer();
    }

    void Dispose()
    {
        print("dispose");

        lightBuffer?.Release();
        shadowLightBuffer?.Release();
        lightCountBuffer?.Release();

        shadowBuffer?.Release();
        shadowCountBuffer?.Release();
        sphereShadowBuffer?.Release();
    }

    void SetStaticShadowBuffer()
    {
        if (disableShadows) return;

        int totalShadowTriLength = 0;
        for (int i = 0; i < staticShadowRenderers.Count; i++)
        {
            if (staticShadowRenderers[i].castShadowLevel < castShadowLevel) continue;
            totalShadowTriLength += staticShadowRenderers[i].GetTri().Length/3;
        }

        Vector4[] shadowSend = new Vector4[totalShadowTriLength*4];

        int triPlaceIndex = 0;

        for (int j = 0; j < staticShadowRenderers.Count; j++)
        {
            Renderer4D ren = staticShadowRenderers[j];
            if (ren.castShadowLevel < castShadowLevel) continue;

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

        if (totalShadowTriLength > 0) {
            shadowBuffer = new ComputeBuffer(totalShadowTriLength, sizeof(float) * 16);
            shadowBuffer.SetData(shadowSend);
            Shader.SetGlobalBuffer("_ShadowData", shadowBuffer);
        }

        shadowCountBuffer = new ComputeBuffer(2, sizeof(int));
        shadowCountBuffer.SetData(new int[] {totalShadowTriLength, sphereShadowRenderers.Count});
        Shader.SetGlobalBuffer("_ShadowCount", shadowCountBuffer);

        void AddTri(Vector4 v1, Vector4 v2, Vector4 v3, int i)
        {
            shadowSend[4*i + 0] = UFunc.HyperCross(v1,v2,v3).normalized;
            shadowSend[4*i + 1] = UFunc.HyperCross(v1,v2,shadowSend[4*i]).normalized;
            shadowSend[4*i + 2] = UFunc.HyperCross(v2,v3,shadowSend[4*i]).normalized;
            shadowSend[4*i + 3] = UFunc.HyperCross(v3,v1,shadowSend[4*i]).normalized;
        }

        //sphere profile time
        if (sphereShadowRenderers.Count > 0)
        {
            List<float> sphereShadowSend = new List<float>();

            for (int i = 0; i < sphereShadowRenderers.Count; i++)
            {
                Renderer4D ren = sphereShadowRenderers[i];
                if (ren.castShadowLevel < castShadowLevel) continue;

                Vector4 pos = ren.transform4.positionNorm;

                for (int j = 0; j < 4; j++) {
                    //sphereShadowSend[5*i + j] = pos[j];
                    sphereShadowSend.Add(pos[j]);
                }
                //sphereShadowSend[5*i + 4] = Mathf.Cos(ren.sphereShadowRadius);
                sphereShadowSend.Add(Mathf.Cos(ren.sphereShadowRadius));
            }

            sphereShadowBuffer = new ComputeBuffer(sphereShadowRenderers.Count, sizeof(float)*5);
            sphereShadowBuffer.SetData(sphereShadowSend);
            Shader.SetGlobalBuffer("_ShadowSphereData", sphereShadowBuffer);
        }
    }

    public void AddStaticShadow(Renderer4D ren)
    {
        if (!ren.doSphereShadowProfile)
        {
            if (sphereShadowRenderers.Contains(ren)) sphereShadowRenderers.Remove(ren);
            if (staticShadowRenderers.Contains(ren)) return;
            staticShadowRenderers.Add(ren);
        }
        else
        {
            if (staticShadowRenderers.Contains(ren)) staticShadowRenderers.Remove(ren);
            if (sphereShadowRenderers.Contains(ren)) return;
            sphereShadowRenderers.Add(ren);
        }

        SetStaticShadowBuffer();
    }
    public void RemoveStaticShadow(Renderer4D ren)
    {
        if (!staticShadowRenderers.Contains(ren)) return;
        
        if (!ren.doSphereShadowProfile)
        {
            if (staticShadowRenderers.Contains(ren)) return;
            staticShadowRenderers.Remove(ren);
        }
        else
        {
            if (sphereShadowRenderers.Contains(ren)) return;
            sphereShadowRenderers.Remove(ren);
        }

        SetStaticShadowBuffer();
    }
}
