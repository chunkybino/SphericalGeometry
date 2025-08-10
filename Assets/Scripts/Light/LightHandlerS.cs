using UnityEngine;

[ExecuteAlways]
public class LightHandlerS : MonoBehaviour
{
    public LightData[] lightDatas;
    LightData[] lightDataSend;
    public int lightDatasCount;

    [SerializeField] ComputeBuffer lightBuffer;
    [SerializeField] ComputeBuffer lightIntensityBuffer;
    [SerializeField] ComputeBuffer lightPositionBuffer;
    [SerializeField] ComputeBuffer lightCountBuffer;

    public bool setBuffer;

    void OnValidate()
    {
        if (setBuffer)
        {
            setBuffer = false;

            SetLightBuffer();
        }
    }

    void OnEnable()
    {
        Dispose();
        SetLightBuffer();
    }

    void OnDestroy()
    {
        Dispose();
    }
    void OnDisable()
    {
        Dispose();
    }

    void SetLightBuffer()
    {
        lightDataSend = new LightData[lightDatas.Length];
        for (int i = 0; i < lightDatas.Length; i++)
        {
            lightDatas[i].position = lightDatas[i].position.normalized;

            lightDataSend[i] = lightDatas[i];
            lightDataSend[i].doFalloff = lightDatas[i].doFalloff;
            lightDataSend[i].falloffRange = lightDatas[i].falloffRange == 0 ? 999 : 1/(lightDatas[i].falloffRange); //will become falloffRange on shader side
        }

        if (lightBuffer == null || lightDatasCount != lightDatas.Length)
        {
            lightDatasCount = lightDatas.Length;

            lightBuffer = new ComputeBuffer(lightDatasCount, System.Runtime.InteropServices.Marshal.SizeOf(typeof(LightData))); //System.Runtime.InteropServices.Marshal.SizeOf(typeof(LightData)));
            lightBuffer.SetData(lightDataSend);
            Shader.SetGlobalBuffer("_LightData", lightBuffer);

            lightCountBuffer = new ComputeBuffer(1, sizeof(int));
            lightCountBuffer.SetData(new int[] {lightDatasCount});
            Shader.SetGlobalBuffer("_LightCount", lightCountBuffer);
        }
        else
        {
            lightBuffer.SetData(lightDataSend);
            Shader.SetGlobalBuffer("_LightData", lightBuffer);

            lightCountBuffer.SetData(new int[] {lightDatasCount});
            Shader.SetGlobalBuffer("_LightCount", lightCountBuffer);
        }
    }

    void Dispose()
    {
        lightBuffer?.Dispose();
        lightIntensityBuffer?.Dispose();
        lightPositionBuffer?.Dispose();
        lightCountBuffer?.Dispose();
    }
}
