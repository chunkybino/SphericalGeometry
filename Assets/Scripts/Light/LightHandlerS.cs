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

    public bool setBuffer;

    void OnValidate()
    {
        if (setBuffer)
        {
            setBuffer = false;

            UpdateBufferSendData();
            SetLightBuffer();
        }
    }

    void Awake()
    {
        CheckSingleton();
    }

    void OnEnable()
    {
        CheckSingleton();

        Dispose();

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

    void Dispose()
    {
        lightBuffer?.Dispose();
        lightCountBuffer?.Dispose();
    }

    public void AddLight(LightS light)
    {
        if (lights.Contains(light)) return;

        light.lightIndex = lights.Count;
        lights.Add(light);

        UpdateFullBuffer();
    }
    public void RemoveLight(LightS light)
    {
        if (!lights.Contains(light)) return;

        lights.Remove(light);

        for (int i = light.lightIndex; i < lights.Count; i++)
        {
            lights[i].lightIndex--;
        }

        UpdateFullBuffer();
    }
}
