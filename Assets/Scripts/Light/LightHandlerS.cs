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

    [SerializeField] List<Vector4> shadowSend;

    public Vector4[] shadowTriVerticies;
    public Vector4[] dynamicShadowTriVerticies;

    int totalShadowTriLength = 0;
    int totalDynamicShadowTriLength = 0;

    [SerializeField] Vector4[] shadowSideNormals;
    [SerializeField] Vector2Int[] shadowTriBufferSpan; //x = buffer start, y = end
    [SerializeField] List<Vector3Int> shadowSubSpan;

    [SerializeField] ComputeBuffer shadowBuffer;
    [SerializeField] ComputeBuffer shadowCountBuffer;

    [SerializeField] ComputeBuffer shadowSpanBuffer;
    [SerializeField] ComputeBuffer shadowSpanSubBuffer;

    public List<Renderer4D> staticShadowRenderers = new List<Renderer4D>();
    public List<Renderer4D> dynamicShadowRenderers = new List<Renderer4D>();

    public bool disableShadows;
    public bool shadowRaycast;

    public bool updateFullBuffer;

    public ComputeShader edgeVertexCompute;

    void Awake()
    {
        CheckSingleton();
    }

    void OnEnable()
    {
        CheckSingleton();

        Dispose();

        SetStaticShadowBuffer(true);

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
            SetStaticShadowBuffer(true);
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
            if (bufferCount > 0 && buff != null) {
                buff.SetData(send, startIndex, startIndex, bufferCount);
            }
        }

        /////////////////////////////
        //the rest of update

        if (disableShadows)
        {
            if (shadowSend.Count > 0)
            {
                shadowBuffer?.Release();
                shadowCountBuffer?.Release();
                shadowSpanBuffer?.Release();
                shadowSpanSubBuffer?.Release();
                shadowSend.Clear();
            }
        }
        else
        {
            if (shadowSend.Count == 0 && staticShadowRenderers.Count+dynamicShadowRenderers.Count > 0)
            {
                SetStaticShadowBuffer(true);
            }
            else
            {
                SetStaticShadowBuffer(false);
            }
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
            //print(System.Runtime.InteropServices.Marshal.SizeOf(typeof(LightData)));
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
        shadowSpanBuffer?.Release();
        shadowSpanSubBuffer?.Release();
    }

    void SetStaticShadowBuffer(bool doStatic = true)
    {
        if (disableShadows) return;

        if (shadowRaycast)
        {
            //static 
            if (doStatic)
            {
                totalShadowTriLength = 0;
                for (int i = 0; i < staticShadowRenderers.Count; i++) {
                    totalShadowTriLength += staticShadowRenderers[i].GetTri().Length/3;
                }

                shadowTriVerticies = new Vector4[totalShadowTriLength*4];
                AddRenderersToTri(staticShadowRenderers, shadowTriVerticies);
            }
            ///////////////

            //dynamic
            totalDynamicShadowTriLength = 0;
            for (int i = 0; i < dynamicShadowRenderers.Count; i++) {
                totalDynamicShadowTriLength += dynamicShadowRenderers[i].GetTri().Length/3;
            }
            ///////////////////////

            dynamicShadowTriVerticies = new Vector4[totalDynamicShadowTriLength*4];
            AddRenderersToTri(dynamicShadowRenderers, dynamicShadowTriVerticies);

            void AddRenderersToTri(List<Renderer4D> renderers, Vector4[] vertexArray)
            {
                int triPlaceIndex = 0;
                for (int j = 0; j < renderers.Count; j++)
                {
                    Renderer4D ren = renderers[j];

                    int[] shadowTri = ren.GetTri();
                    Vector4[] shadowVertex4 = ren.GetVertexWorld();
                    int shadowTriCount = shadowTri.Length/3;

                    Matrix4x4 mat = ren.transform4.matrix;

                    /*
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
                    */

                    for (int i = 0; i < shadowTriCount; i++)
                    {
                        AddTri(shadowVertex4[shadowTri[3*i+0]], shadowVertex4[shadowTri[3*i+1]], shadowVertex4[shadowTri[3*i+2]], triPlaceIndex);

                        triPlaceIndex++;
                    }
                }

                void AddTri(Vector4 v1, Vector4 v2, Vector4 v3, int i)
                {
                    vertexArray[4*i + 0] = UFunc.HyperCross(v1,v2,v3).normalized;
                    vertexArray[4*i + 1] = UFunc.HyperCross(v1,v2,vertexArray[4*i]).normalized;
                    vertexArray[4*i + 2] = UFunc.HyperCross(v2,v3,vertexArray[4*i]).normalized;
                    vertexArray[4*i + 3] = UFunc.HyperCross(v3,v1,vertexArray[4*i]).normalized;
                }
            }
        }

        List<Vector4> sendList = new List<Vector4>();

        shadowTriBufferSpan = new Vector2Int[shadowLights.Count];
        shadowSubSpan.Clear();

        int placeIndex = 0;
        for (int i = 0; i < shadowLights.Count; i++)
        {
            if (!shadowRaycast)
            {
                shadowLights[i].SetShadowNormals(doStatic); //set dynamic shadows only

                List<Vector4> sideNorms = shadowLights[i].allSideNormals;

                foreach (Vector3Int v in shadowLights[i].sideNormalSpan) {
                    shadowSubSpan.Add(new Vector3Int(v.x+sendList.Count, v.y+sendList.Count, v.z+sendList.Count));
                }

                //int span = sideNorms.Count;//norms.Length/5 + normsDynamic.Length/5;
                shadowTriBufferSpan[i] = new Vector2Int(placeIndex,placeIndex + shadowLights[i].sideNormalSpan.Count);
                placeIndex += shadowLights[i].sideNormalSpan.Count;

                sendList.AddRange(sideNorms);
            }
            else
            {
                foreach (Vector4 v in shadowTriVerticies) {
                    sendList.Add(v);
                }
                foreach (Vector4 v in dynamicShadowTriVerticies) {
                    sendList.Add(v);
                }

                /*
                shadowLights[i].SetRaycastShadowNormals(doStatic);

                Vector4[] norms = shadowLights[i].shadowSideNormals;
                Vector4[] normsDynamic = shadowLights[i].dynamicShadowSideNormals;

                for (int j = 0; j < norms.Length; j++) {
                    sendList.Add(norms[j]);
                }
                for (int j = 0; j < normsDynamic.Length; j++) {
                    sendList.Add(normsDynamic[j]);
                }
                */
            }
        }

        shadowSend = sendList;
        if (sendList.Count == 0) return;

        if (sendList.Count > 0) 
        {
            if (!shadowRaycast)
            {
                shadowBuffer = new ComputeBuffer(sendList.Count, sizeof(float) * 4);
                shadowBuffer.SetData(sendList);
                Shader.SetGlobalBuffer("_ShadowData", shadowBuffer);
            }
            else
            {
                shadowBuffer?.Release();
                shadowBuffer = new ComputeBuffer(sendList.Count, sizeof(float) * 4);
                shadowBuffer.SetData(sendList);
                Shader.SetGlobalBuffer("_RaycastShadowData", shadowBuffer);

                shadowCountBuffer = new ComputeBuffer(1, sizeof(int));
                shadowCountBuffer.SetData(new int[] {sendList.Count/4});
                Shader.SetGlobalBuffer("_RaycastShadowCount", shadowCountBuffer);
            }
        }

        if (shadowTriBufferSpan.Length > 0 && shadowSubSpan.Count > 0 && !shadowRaycast) 
        {
            shadowSpanBuffer = new ComputeBuffer(shadowTriBufferSpan.Length, sizeof(int)*2);
            shadowSpanBuffer.SetData(shadowTriBufferSpan);
            Shader.SetGlobalBuffer("_ShadowSpan", shadowSpanBuffer);

            shadowSpanSubBuffer = new ComputeBuffer(shadowSubSpan.Count, sizeof(int)*3);
            shadowSpanSubBuffer.SetData(shadowSubSpan);
            Shader.SetGlobalBuffer("_ShadowSpanSub", shadowSpanSubBuffer);
        }
    }

    public void AddShadow(Renderer4D ren, bool isStatic)
    {
        if (isStatic)
        {
            if (staticShadowRenderers.Contains(ren)) return;
            staticShadowRenderers.Add(ren);
        }
        else
        {
            if (dynamicShadowRenderers.Contains(ren)) return;
            dynamicShadowRenderers.Add(ren);
        }

        SetStaticShadowBuffer(isStatic);
    }
    public void RemoveShadow(Renderer4D ren, bool isStatic)
    {
        if (isStatic)
        {
            if (!staticShadowRenderers.Contains(ren)) return;
            staticShadowRenderers.Remove(ren);
        }
        else
        {
            if (!dynamicShadowRenderers.Contains(ren)) return;
            dynamicShadowRenderers.Remove(ren);
        }

        SetStaticShadowBuffer(isStatic);
    }
}





/*

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

        if (totalShadowTriLength > 0) {
            shadowBuffer = new ComputeBuffer(totalShadowTriLength, sizeof(float) * 16);
            shadowBuffer.SetData(shadowSend);
            Shader.SetGlobalBuffer("_ShadowData", shadowBuffer);
        }

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

*/