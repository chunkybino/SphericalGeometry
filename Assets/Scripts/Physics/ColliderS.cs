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
    public virtual TriColliderS triangle {get{return null;}}

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

        Vector4 contact = UFunc.Slerp4(contact1, contact2, push1); 

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

            if (type1 > type2) {
                (c1,c2) = (c2,c1);
                (type1,type2) = (type2,type1);
            }
            
            float over = Find();

            return over;

            float Find()
            {
                switch (type1) {
                    default: //sphere
                        switch (type2) {
                            default: //sphere-sphere
                                return SphereOnSphere(c1.sphere, c2.sphere, ref contact1, ref contact2, ref contactNormal);
                            case 1: //sphere-capsule
                                return SphereOnCapsule(c1.sphere, c2.capsule, ref contact1, ref contact2, ref contactNormal);
                            case 2: //sphere-triangle
                                return SphereOnTriangle(c1.sphere, c2.triangle, ref contact1, ref contact2, ref contactNormal);
                        }
                    case 1: //capsule
                        switch (type2) {
                            default: //capsule-capsule
                                return CapsuleOnCapsule(c1.capsule, c2.capsule, ref contact1, ref contact2, ref contactNormal);
                        }
                }

                return 1;
            }
        }
    }

    public static float PointRadiusContact(Vector4 p1, float r1, Vector4 p2, float r2, ref Vector4 contact1, ref Vector4 contact2, ref Vector4 contactNorm)
    {
        //find angle between positions of both
        float dot = UFunc.Dot(p1, p2);
        float dis = Mathf.Acos(Mathf.Clamp(dot,-1,1)) * Transform4D.radius;

        float overlap = r1 + r2 - dis;

        if (overlap < 0) return overlap;

        contact1 = UFunc.Slerp4(p1, p2, r1 / dis);
        contact2 = UFunc.Slerp4(p2, p1, r2 / dis);

        contactNorm = (p2 - p1).normalized;

        return overlap;
    }

    public static float SphereOnSphere(SphereColliderS c1, SphereColliderS c2, ref Vector4 contact1, ref Vector4 contact2, ref Vector4 contactNorm)
    {
        Vector4 p1 = c1.center;
        Vector4 p2 = c2.center;

        return PointRadiusContact(p1, c1.radius, p2, c2.radius, ref contact1, ref contact2, ref contactNorm);
    }

    public static float SphereOnCapsule(SphereColliderS c1, CapsuleColliderS c2, ref Vector4 contact1, ref Vector4 contact2, ref Vector4 contactNorm)
    {
        Vector4 capPoint = UFunc.SlerpPointClose(c2.point1,c2.point2,c1.center);

        return PointRadiusContact(c1.center, c1.radius, capPoint, c2.radius, ref contact1, ref contact2, ref contactNorm);
    }

    public static float SphereOnTriangle(SphereColliderS c1, TriColliderS c2, ref Vector4 contact1, ref Vector4 contact2, ref Vector4 contactNorm)
    {
        Vector4 triPoint = c2.PointClose(c1.center);

        //print(c1.center.ToString()+triPoint.ToString());
        //print(UFunc.DistanceS(c1.center, triPoint));

        return PointRadiusContact(c1.center, c1.radius, triPoint, 0, ref contact1, ref contact2, ref contactNorm);
    }

    public static float CapsuleOnCapsule(CapsuleColliderS c1, CapsuleColliderS c2, ref Vector4 contact1, ref Vector4 contact2, ref Vector4 contactNorm)
    {
        Vector4 point1_1 = new Vector4();
        Vector4 point2_1 = new Vector4();
        FindMin(0, 0, ref point1_1, ref point2_1);

        Vector4 point1_2 = new Vector4();
        Vector4 point2_2 = new Vector4();
        FindMin(1, 1, ref point1_1, ref point2_1);

        if (UFunc.Dot(point1_1,point2_1) > UFunc.Dot(point1_2,point2_2))
        {
            return PointRadiusContact(point1_1, c1.radius, point2_1, c2.radius, ref contact1, ref contact2, ref contactNorm);
        }
        else
        {
            return PointRadiusContact(point1_2, c1.radius, point2_2, c2.radius, ref contact1, ref contact2, ref contactNorm);
        }

        void FindMin(float t1, float t2, ref Vector4 point1, ref Vector4 point2)
        {
            int iterations = 8;

            point1 = UFunc.Slerp4(c1.point1,c1.point2, t1);
            point2 = UFunc.Slerp4(c2.point1,c2.point2, t2);

            for (int i = 0; i < iterations; i++)
            {
                if (i % 2 == 0)
                {
                    t1 = UFunc.SlerpPointCloseFactor(c1.point1,c1.point2,point2);
                    point1 = UFunc.Slerp4(c1.point1,c1.point2, t1);
                }
                else
                {
                    t2 = UFunc.SlerpPointCloseFactor(c2.point1,c2.point2,point1);
                    point2 = UFunc.Slerp4(c2.point1,c2.point2, t2);
                }

                if (point1 == point2) break;
            }
        }
    }
}
