using UnityEngine;

[ExecuteAlways]
public class ShadowCaster : MonoBehaviour, I_ShadowCaster
{
    public Transform4D transform4;

    public bool castShadows;
    bool m_castShadows;
    public int castShadowLevel;

    /*
    public bool doSphereProfile;
    bool m_doSphereProfile;
    */
    public float sphereProfileRad;

    public int[] GetTri() {return null;}
    public Vector3[] GetVertex3() {return null;}
    public Vector4[] GetVertex4() {return null;}

    public virtual Vector3 GetScale() {return new Vector3(1,1,1)*transform4.scale;}

    public Vector4 GetPos() {return transform4.positionNorm;}
    public Matrix4x4 GetMatrix() {return transform4.matrix;}

    public bool shadow_doVertex4 {get{return false;}}
    public int shadow_castShadowLevel {get{return castShadowLevel;}}

    //public bool shadow_doSphereProfile {get{return doSphereProfile;}}
    public LightHandlerS.ShadowProfileType shadow_profileType {get{return LightHandlerS.ShadowProfileType.Sphere;}}
    public float shadow_sphereRadius {get{return sphereProfileRad*transform4.scale;}}

    void OnEnable()
    {
        if (!transform4) transform4 = GetComponent<Transform4D>();

        if (castShadows && LightHandlerS.singleton != null) {
            LightHandlerS.singleton.AddStaticShadow(this);
        }
    }
    void OnDisable()
    {
        if (castShadows && LightHandlerS.singleton != null) {
            LightHandlerS.singleton.RemoveStaticShadow(this);
        }
    }

    void OnValidate()
    {
        if (!gameObject.activeInHierarchy) return;

        if (LightHandlerS.singleton != null)
        {

            if (castShadows != m_castShadows)
            {
                if (castShadows) {
                    LightHandlerS.singleton.AddStaticShadow(this);
                } else {
                    LightHandlerS.singleton.RemoveStaticShadow(this);
                }
            }
            m_castShadows = castShadows;

            /*
            if (doSphereProfile != m_doSphereProfile && castShadows)
            {
                LightHandlerS.singleton.AddStaticShadow(this);
            }
            m_doSphereProfile = doSphereProfile;
            */
        }
    }
}
