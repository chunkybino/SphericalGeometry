using UnityEngine;
using System.Collections.Generic;

[ExecuteAlways]
public class LightHandlerS : MonoBehaviour
{
    public static LightHandlerS singleton;

    public List<LightS> lights = new List<LightS>();
    public List<LightS> noShadowLights = new List<LightS>();
    public List<LightS> shadowLights = new List<LightS>();

    //LightData[] lightDataSend;
    LightData[] lightDataSendNoShadow;
    LightData[] lightDataSendShadow;

    [SerializeField] int lightDatasCount;
    [SerializeField] int noShadowlightDatasCount;
    [SerializeField] int shadowLightDatasCount;

    [SerializeField] ComputeBuffer lightBuffer;
    [SerializeField] ComputeBuffer shadowLightBuffer;
    [SerializeField] ComputeBuffer lightCountBuffer;

    [SerializeField] Vector4[] shadowSend;
    [SerializeField] List<float> sphereShadowSend = new List<float>();

    [SerializeField] ComputeBuffer shadowBuffer;
    [SerializeField] ComputeBuffer shadowCountBuffer;

    [SerializeField] ComputeBuffer sphereShadowBuffer;

    public List<I_ShadowCaster> staticShadowRenderers = new List<I_ShadowCaster>();
    public List<I_ShadowCaster> sphereShadowRenderers = new List<I_ShadowCaster>();

    [SerializeField] int totalShadowTriLength;

    [SerializeField] int staticShadowCount;
    int prevStaticShadowCount;
    [SerializeField] int sphereShadowCount;
    int prevSphereShadowCount;

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

        //SetLightBuffer();
        SetStaticShadowBuffer();

        //UpdateBufferSendData();
        //SetLightBuffer();
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
        //UpdateFullBuffer();

        //print(lightDataSendNoShadow + " " + lightDataSendNoShadow.Length);

        UpdateBufferSendData();

        /*
        print("nope " +(lightBuffer==null));
        if (lightBuffer != null) print(lightBuffer.IsValid());

        UpdateBuf(noShadowLights, lightBuffer, lightDataSendNoShadow);
        UpdateBuf(shadowLights, shadowLightBuffer, lightDataSendShadow);
        */

        //SetLightBuffer();

        void UpdateBuf(List<LightS> light, ComputeBuffer buff, LightData[] send)
        {
            //print(buff == null);
            //print(buff.IsValid());
            if (buff == null || !buff.IsValid()) return;

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
            shadowBuffer?.Release();
            shadowCountBuffer?.Release();
            sphereShadowBuffer?.Release();
            shadowSend = new Vector4[0];
            sphereShadowSend.Clear();
        }
        else
        {
            SetStaticShadowBuffer();
        }
        
        UpdateShadowBuffer();
    }

    void UpdateFullBuffer()
    {
        UpdateBufferSendData();
    }

    void UpdateBufferSendData()
    {
        //SetLightBuffer();

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

        if (lightBuffer != null && lightBuffer.IsValid()) {
            lightBuffer.SetData(lightDataSendNoShadow);
        }
        if (shadowLightBuffer != null && shadowLightBuffer.IsValid()) {
            shadowLightBuffer.SetData(lightDataSendShadow);
        }
    }   

    void SetLightBuffer() 
    {
        int prevNoShadowLightCount = noShadowLights.Count;
        int prevShadowLightCount = shadowLights.Count;

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

        bool showShadowLightChange = noShadowLights.Count != prevNoShadowLightCount;
        bool sphereShadowCasterChange = shadowLights.Count != prevShadowLightCount;

        if (noShadowlightDatasCount > 0)
        {
            if (showShadowLightChange || lightBuffer == null)
            {
                //print(lightCountBuffer != null);
                //if (lightCountBuffer != null) lightCountBuffer.Release();

                lightBuffer = new ComputeBuffer(noShadowlightDatasCount, System.Runtime.InteropServices.Marshal.SizeOf(typeof(LightData)));
                Shader.SetGlobalBuffer("_LightDataNoShadow", lightBuffer);
            }

            /*
            if (lightBuffer != null && lightBuffer.IsValid()) {
                lightBuffer.SetData(lightDataSendNoShadow);
            }
            */
        }

        //print();
        if (shadowLightDatasCount > 0)
        {
            if (sphereShadowCasterChange || shadowLightBuffer == null)
            {
                //print(lightCountBuffer != null);
                //if (lightCountBuffer != null) lightCountBuffer.Release();

                shadowLightBuffer = new ComputeBuffer(shadowLightDatasCount, System.Runtime.InteropServices.Marshal.SizeOf(typeof(LightData)));
                Shader.SetGlobalBuffer("_LightDataShadow", shadowLightBuffer);
            }

            /*
            if (shadowLightBuffer != null && shadowLightBuffer.IsValid()) {
                shadowLightBuffer.SetData(lightDataSendShadow);
            }
            */
        }
        

        if (lightCountBuffer == null || !lightCountBuffer.IsValid())
        {
            //print(lightCountBuffer != null);
            //if (lightCountBuffer != null) lightCountBuffer.Release();
            lightCountBuffer = new ComputeBuffer(2, sizeof(int));
            Shader.SetGlobalBuffer("_LightCount", lightCountBuffer);
        }

        lightCountBuffer.SetData(new int[] {lightDatasCount,shadowLightDatasCount});
        //if (showShadowLightChange || sphereShadowCasterChange) {
        //    lightCountBuffer.SetData(new int[] {lightDatasCount,shadowLightDatasCount});
        //}
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

    void UpdateShadowBuffer()
    {
        if (disableShadows) return;

        bool staticShadowCasterChange = staticShadowCount != prevStaticShadowCount;
        prevStaticShadowCount = sphereShadowCount;
        bool sphereShadowCasterChange = sphereShadowCount != prevSphereShadowCount;
        prevSphereShadowCount = sphereShadowCount;

        if (totalShadowTriLength > 0 && (staticShadowCasterChange || shadowBuffer == null))
        {
            //if (shadowBuffer != null) shadowBuffer.Release();
            shadowBuffer = new ComputeBuffer(totalShadowTriLength, sizeof(float) * 16);
            Shader.SetGlobalBuffer("_ShadowData", shadowBuffer);
        }
        if (sphereShadowCount > 0 && (sphereShadowCasterChange || sphereShadowBuffer == null))
        {
            //if (sphereShadowBuffer != null) sphereShadowBuffer.Release();
            sphereShadowBuffer = new ComputeBuffer(sphereShadowCount, sizeof(float)*5);
            Shader.SetGlobalBuffer("_ShadowSphereData", sphereShadowBuffer);
        }

        if (shadowBuffer != null && shadowBuffer.IsValid()) {
            shadowBuffer.SetData(shadowSend);
        }
        if (sphereShadowBuffer != null && sphereShadowBuffer.IsValid()) {
            sphereShadowBuffer.SetData(sphereShadowSend);
        }

        if (shadowCountBuffer == null || !shadowCountBuffer.IsValid())
        {
            shadowCountBuffer = new ComputeBuffer(2, sizeof(int));
            Shader.SetGlobalBuffer("_ShadowCount", shadowCountBuffer);
        }
        if (staticShadowCasterChange || sphereShadowCasterChange) {
            shadowCountBuffer.SetData(new int[] {totalShadowTriLength, sphereShadowCount});
        }
    }

    void SetStaticShadowBuffer()
    {
        if (disableShadows) return;

        totalShadowTriLength = 0;
        for (int i = 0; i < staticShadowRenderers.Count; i++)
        {
            if (staticShadowRenderers[i].shadow_castShadowLevel < castShadowLevel) continue;
            totalShadowTriLength += staticShadowRenderers[i].GetTri().Length/3;
        }

        Vector4[] shadowSend = new Vector4[totalShadowTriLength*4];

        int triPlaceIndex = 0;

        staticShadowCount = 0;
        for (int j = 0; j < staticShadowRenderers.Count; j++)
        {
            I_ShadowCaster ren = staticShadowRenderers[j];
            if (ren.shadow_castShadowLevel < castShadowLevel) continue;

            staticShadowCount++;

            int[] shadowTri = ren.GetTri();
            Vector4[] shadowVertex4 = new Vector4[0];
            int shadowTriCount = shadowTri.Length/3;

            Matrix4x4 mat = ren.GetMatrix();

            if (!ren.shadow_doVertex4)
            {
                Vector3[] shadowVertex = ren.GetVertex3();
                shadowVertex4 = new Vector4[shadowVertex.Length];

                for (int i = 0; i < shadowVertex.Length; i++)
                {
                    Vector3 p = Vector3.Scale(ren.GetScale(), shadowVertex[i]);
                    shadowVertex4[i] = mat * UFunc.SterographicInverse(p,1);
                }
            }
            else
            {
                shadowVertex4 = ren.GetVertex4();
            }

            for (int i = 0; i < shadowTriCount; i++)
            {
                AddTri(shadowVertex4[shadowTri[3*i+0]], shadowVertex4[shadowTri[3*i+1]], shadowVertex4[shadowTri[3*i+2]], triPlaceIndex);
                triPlaceIndex++;
            }
        }

        this.shadowSend = shadowSend;

        /*
        if (totalShadowTriLength > 0) {
            if (shadowBuffer != null) shadowBuffer.Release();
            shadowBuffer = new ComputeBuffer(totalShadowTriLength, sizeof(float) * 16);
            shadowBuffer.SetData(shadowSend);
            Shader.SetGlobalBuffer("_ShadowData", shadowBuffer);
        }
        */

        void AddTri(Vector4 v1, Vector4 v2, Vector4 v3, int i)
        {
            shadowSend[4*i + 0] = UFunc.HyperCross(v1,v2,v3).normalized;
            shadowSend[4*i + 1] = UFunc.HyperCross(v1,v2,shadowSend[4*i]).normalized;
            shadowSend[4*i + 2] = UFunc.HyperCross(v2,v3,shadowSend[4*i]).normalized;
            shadowSend[4*i + 3] = UFunc.HyperCross(v3,v1,shadowSend[4*i]).normalized;
        }

        //sphere profile time
        sphereShadowCount = 0;
        sphereShadowSend.Clear();
        if (sphereShadowRenderers.Count > 0)
        {
            //List<float> sphereShadowSend = new List<float>();

            for (int i = 0; i < sphereShadowRenderers.Count; i++)
            {
                I_ShadowCaster ren = sphereShadowRenderers[i];
                if (ren.shadow_castShadowLevel < castShadowLevel) continue;

                sphereShadowCount++;

                Vector4 pos = ren.GetPos();

                for (int j = 0; j < 4; j++) {
                    //sphereShadowSend[5*i + j] = pos[j];
                    sphereShadowSend.Add(pos[j]);
                }
                //sphereShadowSend[5*i + 4] = Mathf.Cos(ren.sphereShadowRadius);
                sphereShadowSend.Add(Mathf.Cos(ren.shadow_sphereRadius));
            }

            /*
            if (sphereShadowCount > 0)
            {
                if (sphereShadowBuffer != null) sphereShadowBuffer.Release();
                sphereShadowBuffer = new ComputeBuffer(sphereShadowCount, sizeof(float)*5);
                sphereShadowBuffer.SetData(sphereShadowSend);
                Shader.SetGlobalBuffer("_ShadowSphereData", sphereShadowBuffer);
            }
            */
        }

        //shadow count time
        /*
        if (shadowCountBuffer != null) shadowCountBuffer.Release();
        shadowCountBuffer = new ComputeBuffer(2, sizeof(int));
        shadowCountBuffer.SetData(new int[] {totalShadowTriLength, sphereShadowCount});
        Shader.SetGlobalBuffer("_ShadowCount", shadowCountBuffer);
        */
    }

    public void AddStaticShadow(I_ShadowCaster ren)
    {
        if (!ren.shadow_doSphereProfile)
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
    public void RemoveStaticShadow(I_ShadowCaster ren)
    {     
        if (staticShadowRenderers.Contains(ren)) staticShadowRenderers.Remove(ren);
        if (sphereShadowRenderers.Contains(ren)) sphereShadowRenderers.Remove(ren);

        SetStaticShadowBuffer();
    }
}
