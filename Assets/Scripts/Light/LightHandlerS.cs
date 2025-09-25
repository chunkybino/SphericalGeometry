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

    //[SerializeField] ComputeBuffer shadowBuffer;
    [SerializeField] ComputeBuffer shadowCountBuffer;

    [SerializeField] int totalShadowTriLength;

    public enum ShadowProfileType {Mesh,Sphere};

    [SerializeField] Vector4[] shadowSend;
    [SerializeField] List<float> sphereShadowSend = new List<float>();

    [SerializeField] ShadowProfile meshProfiles;
    [SerializeField] ShadowProfile sphereProfiles;

    [SerializeField] List<I_ShadowCaster> allShadowCasters = new List<I_ShadowCaster>();
    [SerializeField] Dictionary<I_ShadowCaster,int> shadowCasterDict = new Dictionary<I_ShadowCaster,int>();

    [System.Serializable]
    public class ShadowProfile
    {
        public ShadowProfile(string name, int elementSize, LightHandlerS handler)
        {
            bufferElementSize = sizeof(float)*elementSize;
            bufferName = name;
            casterIndicies = new List<int>();

            theHandler = handler;
        }

        public LightHandlerS theHandler;

        public int shadowCount;
        public int prevShadowCount;
        public List<int> casterIndicies;
        public ComputeBuffer profileBuffer;

        public int bufferElementSize = 4; //size of each element * sizeof(float)
        public string bufferName;

        public void Add(I_ShadowCaster cast)
        {
            if (!theHandler.shadowCasterDict.ContainsKey(cast)) return;

            int index = theHandler.shadowCasterDict[cast];
            if (!casterIndicies.Contains(index)) 
            {
                casterIndicies.Add(index);
                shadowCount++;
            }
        }
        public void Remove(I_ShadowCaster cast)
        {
            if (!theHandler.shadowCasterDict.ContainsKey(cast)) return;

            int index = theHandler.shadowCasterDict[cast];
            if (casterIndicies.Contains(index)) 
            {
                casterIndicies.Remove(index);
                shadowCount--;
            }
        }
    }

    bool lightBufferDirty;
    bool lightBufferReady;

    bool shadowBufferDirty;
    bool shadowBufferReady;

    public bool disableShadows;
    public int castShadowLevel;
    int prevCastShadowLevel;

    public bool updateFullBuffer;

    void Awake()
    {
        CheckSingleton();

        shadowCasterDict = new Dictionary<I_ShadowCaster,int>();

        meshProfiles = new ShadowProfile("_ShadowData", 16, this);
        sphereProfiles = new ShadowProfile("_ShadowSphereData", 5, this);
    }

    void OnEnable()
    {
        CheckSingleton();

        if (allShadowCasters == null) allShadowCasters = new List<I_ShadowCaster>();
        if (shadowCasterDict == null) shadowCasterDict = new Dictionary<I_ShadowCaster,int>();

        if (meshProfiles == null) {
            meshProfiles = new ShadowProfile("_ShadowData", 16, this);
        }
        if (sphereProfiles == null) {
            sphereProfiles = new ShadowProfile("_ShadowSphereData", 5, this);
        }

        Dispose();

        //SetLightBuffer();
        //SetStaticShadowBuffer();

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
            UpdateShadowBuffer();
        }
    }

    void Update()
    {
        if (lightBufferDirty) {
            if (shadowBufferReady) {
                lightBufferDirty = false;
                shadowBufferReady = false;
                UpdateBufferSendData();
            }
            else {
                shadowBufferReady = true;
            }
        }

        if (disableShadows)
        {
            DisposeShadow();

            shadowSend = new Vector4[0];
            sphereShadowSend.Clear();

            meshProfiles.prevShadowCount = 0;
            sphereProfiles.prevShadowCount = 0;
        }
        else
        {
            shadowBufferDirty = true;
            //SetStaticShadowBuffer();
        }

        if (shadowBufferDirty) {
            if (shadowBufferReady) {
                shadowBufferDirty = false;
                shadowBufferReady = false;
                SetStaticShadowBuffer();
            }
            else {
                shadowBufferReady = true;
            }
        }
        
        UpdateShadowBuffer();
    }

    void LateUpdate()
    {
        lightBufferDirty = true;
    }

    void UpdateBufferSendData()
    {
        SetLightBuffer();

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
        int prevNoShadowLightCount = noShadowlightDatasCount;
        int prevShadowLightCount = shadowLightDatasCount;

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

        bool noShadowChange = noShadowLights.Count != prevNoShadowLightCount;
        bool shadowChange = shadowLights.Count != prevShadowLightCount;

        noShadowlightDatasCount = noShadowLights.Count;
        shadowLightDatasCount = shadowLights.Count;

        if (noShadowlightDatasCount > 0)
        {
            if (noShadowChange || lightBuffer == null)
            {
                lightBuffer = new ComputeBuffer(noShadowlightDatasCount, System.Runtime.InteropServices.Marshal.SizeOf(typeof(LightData)));
                Shader.SetGlobalBuffer("_LightDataNoShadow", lightBuffer);
            }
        }

        if (shadowLightDatasCount > 0)
        {
            if (shadowChange || shadowLightBuffer == null)
            {
                shadowLightBuffer = new ComputeBuffer(shadowLightDatasCount, System.Runtime.InteropServices.Marshal.SizeOf(typeof(LightData)));
                Shader.SetGlobalBuffer("_LightDataShadow", shadowLightBuffer);
            }
        }
        

        if (lightCountBuffer == null)
        {
            lightCountBuffer = new ComputeBuffer(2, sizeof(int));
            Shader.SetGlobalBuffer("_LightCount", lightCountBuffer);
        }

        if (lightCountBuffer != null) {
            lightCountBuffer?.SetData(new int[] {lightDatasCount,shadowLightDatasCount});
        }
    }

    public void AddLight(LightS light)
    {
        if (lights.Contains(light)) return;

        lights.Add(light);

        lightBufferDirty = true;
    }
    public void RemoveLight(LightS light)
    {
        if (!lights.Contains(light)) return;

        lights.Remove(light);

        lightBufferDirty = true;
    }
    public void UpdateLightCastShadow()
    {
        lightBufferDirty = true;
    }

    void Dispose()
    {
        print("dispose");

        lightBuffer?.Release();
        shadowLightBuffer?.Release();
        lightCountBuffer?.Release();

        DisposeShadow();
    }
    void DisposeShadow()
    {
        shadowCountBuffer?.Release();
        meshProfiles.profileBuffer?.Release();
        sphereProfiles.profileBuffer?.Release();
    }

    void UpdateShadowBuffer()
    {
        if (disableShadows) return;

        if (castShadowLevel != prevCastShadowLevel) {
            SetStaticShadowBuffer();
        }
        prevCastShadowLevel = castShadowLevel;

        //mesh casters
        if (meshProfiles.casterIndicies != null)
        {
            totalShadowTriLength = 0;
            List<int> meshCasters = meshProfiles.casterIndicies;
            for (int i = 0; i < meshCasters.Count; i++)
            {
                I_ShadowCaster ren = allShadowCasters[meshCasters[i]];
                if (ren.shadow_castShadowLevel < castShadowLevel) continue;
                totalShadowTriLength += ren.GetTri().Length/3;
            }
            Vector4[] shadowSend = new Vector4[totalShadowTriLength*4];

            int triPlaceIndex = 0;

            meshProfiles.shadowCount = totalShadowTriLength;
            for (int j = 0; j < meshCasters.Count; j++)
            {
                I_ShadowCaster ren = allShadowCasters[meshCasters[j]];
                if (ren.shadow_castShadowLevel < castShadowLevel) continue;

                //meshProfiles.shadowCount++;

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
            
            void AddTri(Vector4 v1, Vector4 v2, Vector4 v3, int i)
            {
                shadowSend[4*i + 0] = UFunc.HyperCross(v1,v2,v3).normalized;
                shadowSend[4*i + 1] = UFunc.HyperCross(v1,v2,shadowSend[4*i]).normalized;
                shadowSend[4*i + 2] = UFunc.HyperCross(v2,v3,shadowSend[4*i]).normalized;
                shadowSend[4*i + 3] = UFunc.HyperCross(v3,v1,shadowSend[4*i]).normalized;
            }
        }

        //sphere profile time
        if (sphereProfiles.casterIndicies != null)
        {
            sphereProfiles.shadowCount = 0;
            sphereShadowSend.Clear();
            List<int> sphereCasters = sphereProfiles.casterIndicies;

            if (sphereCasters.Count > 0)
            {
                for (int i = 0; i < sphereCasters.Count; i++)
                {
                    I_ShadowCaster ren = allShadowCasters[sphereCasters[i]];
                    if (ren.shadow_castShadowLevel < castShadowLevel) continue;

                    sphereProfiles.shadowCount++;

                    Vector4 pos = ren.GetPos();

                    for (int j = 0; j < 4; j++) {
                        sphereShadowSend.Add(pos[j]);
                    }
                    
                    sphereShadowSend.Add(Mathf.Cos(ren.shadow_sphereRadius/Transform4D.radius));
                }
            }
        }
        
        if (meshProfiles.shadowCount != meshProfiles.prevShadowCount || sphereProfiles.shadowCount != sphereProfiles.prevShadowCount) 
        {
            SetStaticShadowBuffer();
        }

        //ssetting the bufferas
        if (meshProfiles.profileBuffer != null && meshProfiles.profileBuffer.IsValid()) {
            meshProfiles.profileBuffer.SetData(shadowSend);
        }
        if (sphereProfiles.profileBuffer != null && sphereProfiles.profileBuffer.IsValid()) {
            sphereProfiles.profileBuffer.SetData(sphereShadowSend);
        }
    }

    void SetStaticShadowBuffer()
    {
        if (disableShadows) return;

        DoProfile(meshProfiles);
        DoProfile(sphereProfiles);

        void DoProfile(ShadowProfile prof)
        {
            bool change = prof.shadowCount != prof.prevShadowCount;
            prof.prevShadowCount = prof.shadowCount;

            if (prof.shadowCount > 0 && (change || prof.profileBuffer == null))
            {
                if (prof.profileBuffer != null) prof.profileBuffer.Release();
                prof.profileBuffer = new ComputeBuffer(prof.shadowCount, prof.bufferElementSize);
                Shader.SetGlobalBuffer(prof.bufferName, prof.profileBuffer);
            }
        }

        //mesh
        if (meshProfiles.profileBuffer != null && meshProfiles.profileBuffer.IsValid()) {
            meshProfiles.profileBuffer.SetData(shadowSend);
        }
        //sphere
        if (sphereProfiles.profileBuffer != null && sphereProfiles.profileBuffer.IsValid()) {
            sphereProfiles.profileBuffer.SetData(sphereShadowSend);
        }

        //shadow count time
        if (shadowCountBuffer == null || !shadowCountBuffer.IsValid())
        {
            shadowCountBuffer = new ComputeBuffer(2, sizeof(int));
            Shader.SetGlobalBuffer("_ShadowCount", shadowCountBuffer);
        }
        if (shadowCountBuffer.IsValid()) {
            shadowCountBuffer.SetData(new int[] {meshProfiles.shadowCount, sphereProfiles.shadowCount});
        }
    }

    public void AddStaticShadow(I_ShadowCaster ren)
    {
        LightHandlerS.ShadowProfileType profType = ren.shadow_profileType;

        if (!shadowCasterDict.ContainsKey(ren))
        {
            shadowCasterDict.Add(ren,allShadowCasters.Count);
            allShadowCasters.Add(ren);
        }

        if (profType == LightHandlerS.ShadowProfileType.Mesh) {
            meshProfiles.Add(ren);
        } else {
            meshProfiles.Remove(ren);
        }

        if (profType == LightHandlerS.ShadowProfileType.Sphere) {
            sphereProfiles.Add(ren);
        } else {
            sphereProfiles.Remove(ren);
        }

        /*
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
        */

        shadowBufferDirty = true;
        //SetStaticShadowBuffer();
    }
    public void RemoveStaticShadow(I_ShadowCaster ren)
    {     
        LightHandlerS.ShadowProfileType profType = ren.shadow_profileType;

        if (profType == LightHandlerS.ShadowProfileType.Mesh) meshProfiles.Remove(ren);

        if (profType == LightHandlerS.ShadowProfileType.Sphere) sphereProfiles.Remove(ren);

        shadowBufferDirty = true;
        //SetStaticShadowBuffer();
    }
}
