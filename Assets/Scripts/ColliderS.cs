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

    public float bounce {get{
        if (rigidbody4 == null) return 0;
        return rigidbody4.bounce;
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
        Vector4 contact1 = new Vector4(); //the point on c1 that will get pushed
        Vector4 contact2 = new Vector4(); //the point on c2 that will get pushed
        Vector4 contactNormal = new Vector4(); //the point on c2 that will get pushed
        //both of these points shall meet when collision is resolved

        float overlap = FindCollisionType();

        if (overlap < 0) return false; //return false cause no collision 

        float totalMass = c1.mass + c2.mass;

        float push1 = 0; 
        float push2 = 0; //the percent of push for c1 and c2

        if (c1.isStatic && c2.isStatic) return true; //both static, so no push

        if (c1.isStatic) {
            push2 = 1;
        } 
        else if (c2.isStatic) {
            push1 = 1;
        }
        else {
            push1 = c2.mass/totalMass;
            push2 = c1.mass/totalMass;
        }

        float bounce = 1 - (1 - UFunc.Clamp01(c1.bounce))*(1 - UFunc.Clamp01(c2.bounce));

        Vector4 contact = UFunc.Slerp4(contact1, contact2, push1); 
        contactNormal = UFunc.ProjectToVectorNormal(contact, contactNormal).normalized;

        c1.rigidbody4?.MoveFromAnchor(contactNormal*overlap * push1, contact);
        c2.rigidbody4?.MoveFromAnchor(-contactNormal*overlap * push2, contact);

        Vector4 vel1 = c1.rigidbody4 != null ? c1.rigidbody4.GetVelocityAtAnchor(contact) : Vector4.zero;
        Vector4 vel2 = c2.rigidbody4 != null ? c2.rigidbody4.GetVelocityAtAnchor(contact) : Vector4.zero;

        Vector4 relativeVel = vel1 - vel2;

        float relativeVelDot = UFunc.Dot(relativeVel, contactNormal);

        //print(relativeVelDot);
        //print(contactNormal * -relativeVelDot * push1);

        c1.rigidbody4?.AddVelocityAtAnchor(-contactNormal * relativeVelDot * push1, contact);
        c2.rigidbody4?.AddVelocityAtAnchor(contactNormal * relativeVelDot * push2, contact);

        return true;

        float FindCollisionType()
        {
            return SphereOnSphere(c1.sphere, c2.sphere, ref contact1, ref contact2, ref contactNormal);
        }
    }

    public static float SphereOnSphere(SphereColliderS s1, SphereColliderS s2)
    {
        Vector4 v1 = new Vector4();
        Vector4 v2 = new Vector4();
        Vector4 contact = new Vector4();
        return SphereOnSphere(s1, s2, ref v1, ref v2, ref contact);
    }
    public static float SphereOnSphere(SphereColliderS s1, SphereColliderS s2, ref Vector4 contact1, ref Vector4 contact2, ref Vector4 contactNorm)
    {
        //find angle between positions of both
        float dot = UFunc.Dot(s1.transform4.positionNorm, s2.transform4.positionNorm);
        float dis = Mathf.Acos(Mathf.Clamp(dot,-1,1)) * Transform4D.radius;

        float overlap = s1.radius + s2.radius - dis;

        if (overlap < 0) return overlap;

        contact1 = UFunc.Slerp4(s1.transform4.positionNorm, s2.transform4.positionNorm, s1.radius / dis);
        contact2 = UFunc.Slerp4(s2.transform4.positionNorm, s1.transform4.positionNorm, s2.radius / dis);

        contactNorm = (s2.transform4.positionNorm - s1.transform4.positionNorm).normalized;

        return overlap;
    }
}
