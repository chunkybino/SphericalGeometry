using UnityEngine;
using System.Collections.Generic;
using System.Collections;

[System.Serializable]
public struct LightData
{
    public Vector4 position;
    public Vector3 color;
    public float intensity;

    public float doFalloff;
    public float falloffStart;
    public float falloffRange;

    public float ambience;

    public Vector4 direction;
    public float rangeAngle;
    public float rangeFalloffAngleMult;
}
