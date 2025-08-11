using UnityEngine;

[ExecuteAlways]
public class LightS : MonoBehaviour
{
    LightHandlerS lightHandler;

    public Transform4D transform4;

    public LightData data;

    int m_lightIndex;
    public int lightIndex {get{return m_lightIndex;}set{m_lightIndex = value;}}

    public Color color;
    public float intensity;

    public bool doFalloff;
    public float falloffStart;
    public float falloffRange;

    public float ambience;

    public bool dirty;

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
        if (
            data.position != transform4.positionNorm ||
            data.color != new Vector3(color.r,color.g,color.b) ||
            data.intensity != intensity ||
            data.doFalloff != (doFalloff ? 1 : 0) ||
            data.falloffStart != falloffStart ||
            data.falloffRange != falloffRange ||
            data.ambience != ambience 
            ) {
            dirty = true;
        }

        data.position = transform4.positionNorm;
        data.color = new Vector3(color.r,color.g,color.b);
        data.intensity = intensity;

        data.doFalloff = doFalloff ? 1 : 0;
        data.falloffStart = falloffStart;
        if (falloffRange == 0) {
            data.falloffRange = 9999;
        } else {
            data.falloffRange = 1/falloffRange;
        }

        data.ambience = ambience;
    }
}
