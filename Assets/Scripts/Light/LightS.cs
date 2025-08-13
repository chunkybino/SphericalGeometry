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

    public Vector3 direction = new Vector3(1,0,0);
    Vector4 direction4;
    [Range(0,180)] public float range = 180;
    [Range(0,180)] public float rangeFalloff = 180;

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
        direction4 = transform4.RelativeToWorld(direction.normalized);
        //rangeCos = Mathf.Cos(Mathf.Deg2Rad * range);
        //rangeFalloffCos = Mathf.Cos(Mathf.Deg2Rad * Mathf.Clamp(range+rangeFalloff,0,180));
        //if (rangeFalloffCos > rangeCos) rangeFalloffCos = rangeCos;

        if (
            data.position != transform4.positionNorm ||
            data.color != new Vector3(color.r,color.g,color.b) ||
            data.intensity != intensity ||
            data.doFalloff != (doFalloff ? 1 : 0) ||
            data.falloffStart != falloffStart ||
            data.falloffRange != falloffRange ||
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

        data.doFalloff = doFalloff ? 1 : 0;
        data.falloffStart = falloffStart;
        if (falloffRange != 0) {
            data.falloffRange = 1/falloffRange;
        } else {
            data.falloffRange = 9999;
        }

        data.ambience = ambience;

        data.direction = direction4;
        data.rangeAngle = Mathf.Deg2Rad*range;
        if (rangeFalloff != 0) {
            data.rangeFalloffAngleMult = 1/(Mathf.Deg2Rad*rangeFalloff);
        } else {
            data.rangeFalloffAngleMult = 9999;
        }
    }
}
