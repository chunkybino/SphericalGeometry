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
    public virtual CapsuleColliderS capsule {get{return null;}}

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

        float bounce = 1 - (1 - c1.bounce)*(1 - c2.bounce);

        Vector4 contact = UFunc.Slerp4(contact1, contact2, push1, overlap); 

        contactNormal = UFunc.ProjectToVectorNormal(contactNormal, contact).normalized;

        c1.transform4.LeftMult(UFunc.RotateTowardsMatrix(contact1, contact2, -overlap*push1/Transform4D.radius));
        c2.transform4.LeftMult(UFunc.RotateTowardsMatrix(contact2, contact1, -overlap*push2/Transform4D.radius));

        Vector4 vel1 = c1.rigidbody4 != null ? c1.rigidbody4.GetVelocityAtAnchor(contact) : Vector4.zero;
        Vector4 vel2 = c2.rigidbody4 != null ? c2.rigidbody4.GetVelocityAtAnchor(contact) : Vector4.zero;

        float velDot1 = UFunc.Dot(vel1, contactNormal);
        float velDot2 = UFunc.Dot(vel2, contactNormal);

        if (velDot1-velDot2 > 0) //if the velDifference is negative, then the objects arnt moving towards eachotjher, so doint do velocity calucations
        {
            c1.rigidbody4?.AddVelocityAtAnchor(contactNormal * (velDot2-velDot1)*push1*(1+bounce), contact);
            c2.rigidbody4?.AddVelocityAtAnchor(contactNormal * (velDot1-velDot2)*push2*(1+bounce), contact);
        }

        return true;

        float FindCollisionType()
        {
            int type1 = c1.colliderType;
            int type2 = c2.colliderType;

            //return SphereOnSphere(c1.sphere, c2.sphere, ref contact1, ref contact2, ref contactNormal);

            switch (type1) {
                default: //sphere
                    switch (type2) {
                        default: //sphere-sphere
                            return SphereOnSphere(c1.sphere, c2.sphere, ref contact1, ref contact2, ref contactNormal);
                        case 1: //sphere-capsule
                            return SphereOnCapsule(c1.sphere, c2.capsule, ref contact1, ref contact2, ref contactNormal);
                    }
                case 1: //capsule
                    switch (type2) {
                        default: //capsule-sphere
                            float overlap = SphereOnCapsule(c2.sphere, c1.capsule, ref contact2, ref contact1, ref contactNormal);
                            contactNormal *= -1;
                            return overlap;
                        //case 1: //capsule-capsule
                        //    return OnCapsule(c2.capsule, c2.capsule, ref contact1, ref contact2, ref contactNormal);
                    }
            }
        }
    }

    public static float SphereOnSphere(SphereColliderS c1, SphereColliderS c2, ref Vector4 contact1, ref Vector4 contact2, ref Vector4 contactNorm)
    {
        Vector4 p1 = c1.center;
        Vector4 p2 = c2.center;

        //find angle between positions of both
        float dot = UFunc.Dot(p1, p2);
        float dis = Mathf.Acos(Mathf.Clamp(dot,-1,1)) * Transform4D.radius;

        float overlap = c1.radius + c2.radius - dis;

        if (overlap < 0) return overlap;

        contact1 = UFunc.Slerp4(p1, p2, c1.radius / dis);
        contact2 = UFunc.Slerp4(p2, p1, c2.radius / dis);

        contactNorm = (p2 - p1).normalized;

        return overlap;
    }

    public static float SphereOnCapsule(SphereColliderS c1, CapsuleColliderS c2, ref Vector4 contact1, ref Vector4 contact2, ref Vector4 contactNorm)
    {
        Matrix4x4 capsuleTransformMat = c2.transform4.matrix;
        Vector4 sphereRelativePos = capsuleTransformMat.transpose * c1.center; //sphere pos relative to capsule orientation

        float closestAngle = Mathf.Atan2(sphereRelativePos[c2.capsuleAxis],sphereRelativePos.w); //flatten sphere pos to XY plane, then take arctan

        closestAngle = Mathf.Clamp(closestAngle, -c2.length/(2*Transform4D.radius), c2.length/2*Transform4D.radius); //clamp to the edges of our capsule

        Vector4 capsuleClosest = new Vector4(0,0,0, Mathf.Cos(closestAngle));
        capsuleClosest[c2.capsuleAxis] = Mathf.Sin(closestAngle);
        capsuleClosest = capsuleTransformMat * capsuleClosest;

        //same stuff as the sphere from here
        //find angle between positions of both
        float dot = UFunc.Dot(c1.center, capsuleClosest);
        float dis = Mathf.Acos(Mathf.Clamp(dot,-1,1)) * Transform4D.radius;

        float overlap = c1.radius + c2.radius - dis;

        if (overlap < 0) return overlap;

        contact1 = UFunc.Slerp4(c1.center, capsuleClosest, c1.radius / dis);
        contact2 = UFunc.Slerp4(capsuleClosest, c1.center, c2.radius / dis);

        contactNorm = (capsuleClosest - c1.center).normalized;

        return overlap;
    }
}
