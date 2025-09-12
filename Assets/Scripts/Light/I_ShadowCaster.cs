using UnityEngine;

public interface I_ShadowCaster
{
    public int[] GetTri();
    public Vector3[] GetVertex3();
    public Vector4[] GetVertex4();

    public virtual Vector3 GetScale() {return new Vector3(1,1,1);}

    public Vector4 GetPos();
    public Matrix4x4 GetMatrix();

    public bool shadow_doVertex4 {get{return false;}}
    public int shadow_castShadowLevel {get{return 0;}}

    public bool shadow_doSphereProfile {get{return false;}}
    public float shadow_sphereRadius {get{return 1;}}
}
