using UnityEngine;

public abstract class ColliderS : MonoBehaviour
{
    public Transform4D transform4;
    public Rigidbody4D rigidbody4;

    public float mass {get{
        if (rigidbody4 == null) return 1;
        return rigidbody4.mass;
    }}

    public bool isStatic {get{
        if (rigidbody4 == null) return true;
        return rigidbody4.isStatic;
    }}

    public bool isTrigger;

    public abstract int colliderType {get;}
    public virtual SphereColliderS sphere {get{return null;}}

    void Awake()
    {
        if (!transform4) transform4 = GetComponent<Transform4D>();
        if (!rigidbody4) rigidbody4 = GetComponent<Rigidbody4D>();
    }

    public static bool CollisionPhysic(ColliderS c1, ColliderS c2)
    {
        Vector4 dir1 = new Vector4(); //direction s1 gets pushed
        Vector4 dir2 = new Vector4(); //direciton s2 gets pushed

        float overlap = FindCollisionType();

        if (overlap < 0) return false; //return false cause no collision

        float totalMass = c1.mass + c2.mass;

        if (c1.isStatic && c2.isStatic) return true; //both static, so no push

        if (c1.isStatic) {
            c2.rigidbody4.MoveTangent(dir2 * overlap);
        } 
        else if (c2.isStatic)  {
            c1.rigidbody4.MoveTangent(dir1 * overlap);
        }
        else {
            c1.rigidbody4.MoveTangent(dir1 * overlap * c2.mass/totalMass);
            c2.rigidbody4.MoveTangent(dir2 * overlap * c1.mass/totalMass);
        }

        return true;

        float FindCollisionType()
        {
            return SphereOnSphere(c1.sphere, c2.sphere, ref dir1, ref dir2);
        }
    }

    public static float SphereOnSphere(SphereColliderS s1, SphereColliderS s2)
    {
        Vector4 v1 = Vector4.zero;
        Vector4 v2 = Vector4.zero;
        return SphereOnSphere(s1, s2, ref v1, ref v2);
    }
    public static float SphereOnSphere(SphereColliderS s1, SphereColliderS s2, ref Vector4 obj1PushDir, ref Vector4 obj2PushDir)
    {
        //find angle between positions of both
        float dot = UFunc.Dot(s1.transform4.positionNorm, s2.transform4.positionNorm);
        float dis = Mathf.Acos(Mathf.Clamp(dot,-1,1)) * Transform4D.radius;

        float overlap = s1.radius + s2.radius - dis;

        if (overlap < 0) return overlap;

        Vector4 posDelta = s2.transform4.position - s1.transform4.position;

        obj1PushDir = posDelta - s1.transform4.position*UFunc.Dot(s1.transform4.position, posDelta);
        obj1PushDir = -obj1PushDir.normalized; //negative this one

        obj2PushDir = obj2PushDir - s2.transform4.position*UFunc.Dot(s2.transform4.position, posDelta);
        obj2PushDir = obj2PushDir.normalized; //positive this one

        return overlap;
    }
}
