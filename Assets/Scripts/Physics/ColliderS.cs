using UnityEngine;

public abstract class ColliderS : MonoBehaviour
{
    public Transform4D transform4;
    public Rigidbody4D rigidbody4;

    public float mass {get{
        if (rigidbody4 == null) return 1;
        return rigidbody4.mass;
    }}
    public float angularMass {get{
        if (rigidbody4 == null) return 1;
        return rigidbody4.angularMass;
    }}
    public float angularMassMult {get{
        if (rigidbody4 == null) return 1;
        return rigidbody4.angularMassMult;
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

    public virtual float m_radius {get{return 0;}}

    public virtual float boundingRadius {get{return 0;}}

    public abstract int colliderType {get;}
    public virtual SphereColliderS sphere {get{return null;}}
    public virtual CapsuleColliderS capsule {get{return null;}}
    public virtual TriColliderS triangle {get{return null;}}
    public virtual MeshColliderS mesh {get{return null;}}

    public abstract Vector4 PointClose(Vector4 point);

    void Awake()
    {
        if (!transform4) transform4 = GetComponent<Transform4D>();
        if (!rigidbody4) rigidbody4 = GetComponent<Rigidbody4D>();
    }

    struct CollisionObject
    {
        public static CollisionObject GetConfig(ColliderS collider, Vector4 contact, Vector4 contactNormal)
        {
            CollisionObject outObj = new CollisionObject();

            outObj.centerDis = UFunc.DistanceS(collider.transform4.positionNorm, contact);

            outObj.linVel = collider.rigidbody4.GetLinearVelocityAtAnchor(contact);
            outObj.angVel = -collider.rigidbody4.GetAngularVelocityAtAnchor(contact)/outObj.centerDis;

            outObj.linDot = UFunc.Dot(outObj.linVel, contactNormal);
            outObj.angDot = UFunc.Dot(outObj.angVel, contactNormal);

            outObj.vel = outObj.linDot + outObj.angDot*outObj.centerDis;
            outObj.linearMoment = outObj.linDot*collider.mass;
            outObj.angularMoment = outObj.angDot*collider.angularMass/outObj.centerDis;
            outObj.moment = outObj.linearMoment + outObj.angularMoment;

            return outObj;
        }

        public float centerDis;

        public Vector4 linVel;
        public Vector4 angVel;

        public float linDot;
        public float angDot;

        public float vel;
        public float linearMoment;
        public float angularMoment;
        public float moment;

        //public float inertia;
    }

    public static bool CollisionPhysic(ColliderS c1, ColliderS c2)
    {
        Vector4 contact1 = new Vector4(); //the point on c1 that will get pushed
        Vector4 contact2 = new Vector4(); //the point on c2 that will get pushed
        Vector4 contactNormal = new Vector4(); //the point on c2 that will get pushed
        //both of these points shall meet when collision is resolved

        float overlap = FindCollisionType();

        if (overlap < 0) return false; //return false cause no collision 

        if (c1.isStatic && c2.isStatic) return true;

        float mass1 = c1.mass;
        float mass2 = c2.mass;
        if (c1.isStatic) mass2 = 0;
        if (c2.isStatic) mass1 = 0;

        float totalMass = mass1+mass2;

        float push1 = mass2/totalMass; 
        float push2 = mass1/totalMass;  //the percent of push for c1 and c2

        float bounce = 1 - (1 - c1.bounce)*(1 - c2.bounce);

        Vector4 contact = UFunc.Slerp4(contact1, contact2, push1);

        contactNormal = UFunc.ProjectToVectorNormal(contactNormal, contact).normalized;

        c1.transform4.LeftMult(UFunc.RotateTowardsMatrix(contact1, contact2, -overlap*push1/Transform4D.radius));
        c2.transform4.LeftMult(UFunc.RotateTowardsMatrix(contact2, contact1, -overlap*push2/Transform4D.radius));

        //velocity stuff

        CollisionObject obj1 = CollisionObject.GetConfig(c1, contact, contactNormal);
        CollisionObject obj2 = CollisionObject.GetConfig(c2, contact, contactNormal);

        if (c2.gameObject.name == "Cap")
        {
            //print(obj2.vel+" "+obj2.linDot+" "+obj2.angDot+" "+obj2.angDot*obj2.centerDis);
        }

        if (obj1.vel < obj2.vel) return true; //if the velDifference is negative, then the objects arnt moving towards eachotjher, so doint do velocity calucations

        if (c1.isStatic) 
        {
            Vector4 newVel = UFunc.SetVectorDirectionValue(obj2.linVel, contactNormal,-obj2.linDot*bounce);
            //c2.rigidbody4.SetLinearVelocityAtAnchor(newVel, contact);
            print(obj2.linVel);
            print(obj2.linDot);

            c2.rigidbody4.ApplyMomentumAtPoint(contactNormal, -obj2.linDot*(1+bounce), 1, contact);

            print(UFunc.Dot(contactNormal, c2.rigidbody4.GetVelocityAtAnchor(contact)));
            return true;
        } 
        else if (c2.isStatic) 
        {
            Vector4 newVel = UFunc.SetVectorDirectionValue(obj1.linVel, contactNormal,-obj1.linDot*bounce);
            //c1.rigidbody4.SetLinearVelocityAtAnchor(newVel, contact);
            c1.rigidbody4.ApplyMomentumAtPoint(contactNormal, -obj1.linDot*(1+bounce), 1, contact);
            return true;
        }
        else
        {

            c1.rigidbody4.ApplyMomentumAtPoint(contactNormal, obj2.linDot, push1, contact);
            c2.rigidbody4.ApplyMomentumAtPoint(contactNormal, obj1.linDot, push2, contact);
        }

        //if (!c1.isStatic) c1.rigidbody4.SetLinearVelocityInDirection(contactNormal, centroidVel, contact);
        //if (!c2.isStatic) c2.rigidbody4.SetLinearVelocityInDirection(contactNormal, centroidVel, contact);

        /*
        if (obj1.vel > obj2.vel) //if the velDifference is negative, then the objects arnt moving towards eachotjher, so doint do velocity calucations
        {
            if (!c1.isStatic) c1.rigidbody4.ApplyMomentumAtAnchor(contactNormal, applyVel1, applyMass1, contact);
            if (!c2.isStatic) c2.rigidbody4.ApplyMomentumAtAnchor(contactNormal, applyVel2, applyMass2, contact);
        }
        */
        /*
        if (vel2 < vel1) //if the velDifference is negative, then the objects arnt moving towards eachotjher, so doint do velocity calucations
        {
            if (!c1.isStatic) c1.rigidbody4?.SetVelocityAtAnchor(contactNormal, vel1 + (vel2-vel1)*push1*(1+bounce), contact);
            if (!c2.isStatic) c2.rigidbody4?.SetVelocityAtAnchor(contactNormal, vel2 + (vel1-vel2)*push2*(1+bounce), contact);
        }
        */

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
                        return SphereOn(c1.sphere, c2, ref contact1, ref contact2, ref contactNormal);
                        /*switch (type2) {
                            default: //sphere-sphere
                                return SphereOnSphere(c1.sphere, c2.sphere, ref contact1, ref contact2, ref contactNormal);
                            case 1: //sphere-capsule
                                return SphereOnCapsule(c1.sphere, c2.capsule, ref contact1, ref contact2, ref contactNormal);
                            case 2: //sphere-triangle
                                return SphereOnTriangle(c1.sphere, c2.triangle, ref contact1, ref contact2, ref contactNormal);
                            case 3: //sphere-triangle
                                return SphereOnMesh(c1.sphere, c2.mesh, ref contact1, ref contact2, ref contactNormal);
                        }*/
                    case 1: //capsule
                        switch (type2) {
                            default: //capsule-capsule
                                return CapsuleOnCapsule(c1.capsule, c2.capsule, ref contact1, ref contact2, ref contactNormal);
                            case 2:
                                return CapsuleOnTriangle(c1.capsule, c2.triangle, ref contact1, ref contact2, ref contactNormal);
                            case 3: //sphere-triangle
                                return CapsuleOnMesh(c1.capsule, c2.mesh, ref contact1, ref contact2, ref contactNormal);
                        }
                }

                return 0;
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

    public static float SphereOn(SphereColliderS c1, ColliderS c2, ref Vector4 contact1, ref Vector4 contact2, ref Vector4 contactNorm)
    {
        Vector4 point2 = c2.PointClose(c1.sphere.center);
        return PointRadiusContact(c1.center, c1.m_radius, point2, c2.m_radius, ref contact1, ref contact2, ref contactNorm);
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

        return PointRadiusContact(c1.center, c1.radius, triPoint, 0, ref contact1, ref contact2, ref contactNorm);
    }

    public static float SphereOnMesh(SphereColliderS c1, MeshColliderS c2, ref Vector4 contact1, ref Vector4 contact2, ref Vector4 contactNorm)
    {
        Vector4 triPoint = c2.PointClose(c1.center);

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
            int iterations = 6;

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

    public static float CapsuleOnTriangle(CapsuleColliderS c1, TriColliderS c2, ref Vector4 contact1, ref Vector4 contact2, ref Vector4 contactNorm)
    {
        Vector4 linePoint = new Vector4();
        Vector4 triPoint = new Vector4();
        c2.LineClose(c1.point1, c1.point2, ref linePoint, ref triPoint);

        return PointRadiusContact(linePoint, c1.radius, triPoint, 0, ref contact1, ref contact2, ref contactNorm);
    }

    public static float CapsuleOnMesh(CapsuleColliderS c1, MeshColliderS c2, ref Vector4 contact1, ref Vector4 contact2, ref Vector4 contactNorm)
    {
        Vector4 linePoint = new Vector4();
        Vector4 meshPoint = new Vector4();
        c2.LineClose(c1.point1, c1.point2, ref linePoint, ref meshPoint);

        return PointRadiusContact(linePoint, c1.radius, meshPoint, 0, ref contact1, ref contact2, ref contactNorm);
    }
}
