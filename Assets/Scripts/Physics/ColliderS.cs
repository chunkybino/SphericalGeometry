using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

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
    public float friction {get{
        if (rigidbody4 == null) return 0;
        return rigidbody4.friction;
    }}

    public bool isTrigger;

    public virtual float m_radius {get{return 0;}}

    public virtual float boundingRadius {get{return 0;}}

    public abstract int colliderType {get;}
    public virtual SphereColliderS sphere {get{return null;}}
    public virtual CapsuleColliderS capsule {get{return null;}}
    public virtual TriColliderS triangle {get{return null;}}
    public virtual MeshColliderS mesh {get{return null;}}
    public virtual RingColliderS ring {get{return null;}}

    public abstract Vector4 PointClose(Vector4 point);
    public abstract void LineClose(Vector4 v1, Vector4 v2, ref Vector4 outLine, ref Vector4 outThis);

    public UnityEvent<ColliderS> onTriggerEnter;
    public UnityEvent<ColliderS> onTriggerStay;
    public UnityEvent<ColliderS> onTriggerExit;

    public List<ColliderS> overlapColliders = new List<ColliderS>();
    public List<ColliderS> thisFrameOverlapColliders = new List<ColliderS>();

    void Awake()
    {
        if (!transform4) transform4 = GetComponent<Transform4D>();
        if (!rigidbody4) rigidbody4 = GetComponent<Rigidbody4D>();
    }

    public void PhysicsUpdate()
    {
        thisFrameOverlapColliders.Clear();
    }
    public void PhysicsUpdate2()
    {
        if (!isTrigger) return;

        for (int i = overlapColliders.Count-1; i >= 0; i--)
        {
            ColliderS c = overlapColliders[i];
            if (!thisFrameOverlapColliders.Contains(c))
            {
                overlapColliders.Remove(c);
                onTriggerExit.Invoke(c);
            }
            else
            {
                onTriggerStay.Invoke(c);
            }
        }
    }

    public void CollisionHappen(ColliderS col)
    {
        if (!isTrigger) return;

        if (!overlapColliders.Contains(col))
        {
            overlapColliders.Add(col);
            onTriggerEnter.Invoke(col);
        }

        thisFrameOverlapColliders.Add(col);
    }

    public static bool IsOverlap(ColliderS c1, ColliderS c2)
    {
        float overlap = FindCollisionType(c1,c2).overlap;

        return overlap > 0;
    }

    public static bool CollisionPhysic(ColliderS c1, ColliderS c2)
    {
        ContactData conData = FindCollisionType(c1, c2);

        float overlap = conData.overlap;
        Vector4 contact1 = conData.contact1;
        Vector4 contact2 = conData.contact2;
        Vector4 contactNormal = conData.contactNormal;

        if (overlap < 0) return false; //return false cause no collision 

        if (c1.isStatic && c2.isStatic) return true;

        float mass1 = c1.mass;
        float mass2 = c2.mass;

        float push1 = mass1 / (mass1 + mass2);
        float push2 = mass2 / (mass1 + mass2);  //the percent of push for c1 and c2

        if (c1.isStatic) {
            push1 = 1;
            push2 = 0;
        }
        if (c2.isStatic) {
            push1 = 0;
            push2 = 2;
        }

        float bounce = 1 - (1 - c1.bounce) * (1 - c2.bounce);

        Vector4 contact = UFunc.Slerp4(contact1, contact2, push2);

        contactNormal = UFunc.ProjectToVectorNormal(contactNormal, contact).normalized;

        if (!c1.isStatic) c1.transform4.LeftMult(UFunc.RotateTowardsMatrix(contact1, contact));
        if (!c2.isStatic) c2.transform4.LeftMult(UFunc.RotateTowardsMatrix(contact2, contact));


        //print(c1.rigidbody4.gameObject.name+" "+c2.rigidbody4.gameObject.name);

        //the new stuff here

        //rb 1
        float centerDis1 = UFunc.DistanceS(c1.transform4.positionNorm, contact);
        Vector4 dirContactToRb1 = UFunc.DirectionFromTo(contact, c1.transform4.positionNorm) * centerDis1;
        Vector4 linearVel1 = new Rotor(c1.transform4.positionNorm, contact) * c1.rigidbody4.velocity;
        Vector4 angularVel1 = new Rotor(c1.transform4.positionNorm, contact) * c1.rigidbody4.angularVelocity4;

        float linearDot1 = Vector4.Dot(linearVel1, contactNormal);

        Vector4 angularAllign1 = -UFunc.HyperCross(dirContactToRb1,contactNormal,contact);
        float angularDot1 = Vector4.Dot(angularVel1, angularAllign1);

        //rb2
        float centerDis2 = UFunc.DistanceS(c2.transform4.positionNorm, contact);
        Vector4 dirContactToRb2 = UFunc.DirectionFromTo(contact, c2.transform4.positionNorm) * centerDis2;
        Vector4 linearVel2 = new Rotor(c2.transform4.positionNorm, contact) * c2.rigidbody4.velocity;
        Vector4 angularVel2 = new Rotor(c2.transform4.positionNorm, contact) * c2.rigidbody4.angularVelocity4;

        float linearDot2 = Vector4.Dot(linearVel2, contactNormal);

        Vector4 angularAllign2 = -UFunc.HyperCross(dirContactToRb2,contactNormal,contact);
        float angularDot2 = Vector4.Dot(angularVel2, angularAllign2);

        //the totals
        
        float totalVelocityDifference = (linearDot2 - linearDot1) + (angularDot2 - angularDot1);
        float theBottomHalf1 = (1 + angularAllign1.sqrMagnitude/c1.angularMassMult)/mass1; //not sure what this part acutally represents, but its part the bottom half of the equation i ended up with for calculating the needed impulse
        float theBottomHalf2 = (1 + angularAllign2.sqrMagnitude/c2.angularMassMult)/mass2;
        if (c1.isStatic) theBottomHalf1 = 0;
        if (c2.isStatic) theBottomHalf2 = 0;

        float impulse = -totalVelocityDifference / (theBottomHalf1 + theBottomHalf2);

        print(totalVelocityDifference+" "+theBottomHalf1+" "+theBottomHalf2+" "+angularAllign2.sqrMagnitude+" "+angularDot2);

        c1.rigidbody4.AddImpulse(-contactNormal*impulse, contact);
        c2.rigidbody4.AddImpulse(contactNormal*impulse, contact);

        /*
        if (!c1.isStatic && !c2.isStatic)
        {
            c1.rigidbody4.angularVelocity4 = finalAngularVel1 + contactToRb1*angularOutComponent1;
            c2.rigidbody4.angularVelocity4 = finalAngularVel2 + contactToRb2*angularOutComponent2;
        }
        */

        /*
        //linear bounce

        float bounceSign = linearDot2 > linearDot1 ? 1 : -1;
        bounce = 0;
        //difference of velocity, times other mass, diveded by total mass (1), gives to add to both side to preserve energy. Multiply by bounce factor
        float finalLinearVel1 = totalLinearVelocity + bounceSign * bounce * push2*Mathf.Abs(linearDot1-linearDot2);
        float finalLinearVel2 = totalLinearVelocity - bounceSign * bounce * push1*Mathf.Abs(linearDot1-linearDot2);


        c1.rigidbody4.velocity = UFunc.SetVectorDirectionValue(c1.rigidbody4.velocity, contactToRb1 * contactNormal, finalLinearVel1);
        c1.rigidbody4.angularVelocity4 = finalAngularVel1;

        c2.rigidbody4.velocity = UFunc.SetVectorDirectionValue(c2.rigidbody4.velocity, contactToRb2 * contactNormal, finalLinearVel2);
        c2.rigidbody4.angularVelocity4 = finalAngularVel2;
        */


        return true;


        /*

        //velocity stuff

        float centerDis1 = UFunc.DistanceS(c1.transform4.positionNorm, contact);
        Vector4 linVel1 = c1.rigidbody4.GetLinearVelocityAtAnchor(contact);
        Vector4 angVel1 = -c1.rigidbody4.GetAngularVelocityAtAnchor(contact);
        float linDot1 = UFunc.Dot(linVel1, contactNormal);
        float angDot1 = UFunc.Dot(angVel1, contactNormal);
        float vel1 = linDot1 + angDot1;

        float centerDis2 = UFunc.DistanceS(c2.transform4.positionNorm, contact);
        Vector4 linVel2 = c2.rigidbody4.GetLinearVelocityAtAnchor(contact);
        Vector4 angVel2 = -c2.rigidbody4.GetAngularVelocityAtAnchor(contact);
        float linDot2 = UFunc.Dot(linVel2, contactNormal);
        float angDot2 = UFunc.Dot(angVel2, contactNormal);
        float vel2 = linDot2 + angDot2;

        if (vel1 < vel2) return true; //if the velDifference is negative, then the objects arnt moving towards eachotjher, so doint do velocity calucations

        if (c1.isStatic)
        {
            c2.rigidbody4.ApplyStaticForce(contactNormal, 0, contact, bounce);

            //Vector4 tanLin2 = linVel2 - linDot2*contactNormal;
            //Vector4 tanAng2 = angVel2 - angDot2*contactNormal;
            //Vector4 tanVel2 = tanLin2 + tanAng2;

            //c2.rigidbody4.ApplyStaticForce(-tanVel2.normalized, -tanVel2.magnitude*Mathf.Pow(1-c2.friction,Time.fixedDeltaTime), contact, 0);
            c2.rigidbody4.AddStaticContact(contactNormal, 0, contact);

            return true;
        }
        else if (c2.isStatic)
        {
            c1.rigidbody4.ApplyStaticForce(-contactNormal, 0, contact, bounce);

            //Vector4 tanLin1 = linVel1 - linDot1*contactNormal;
            //Vector4 tanAng1 = angVel1 - angDot1*contactNormal;
            //Vector4 tanVel1 = tanLin1 + tanAng1;

            //c1.rigidbody4.ApplyStaticForce(-tanVel1.normalized, -tanVel1.magnitude*Mathf.Pow(1-c1.friction,Time.fixedDeltaTime), contact, 0);
            c1.rigidbody4.AddStaticContact(-contactNormal, 0, contact);

            return true;
        }
        else
        {
            float angularPush1 = c1.angularMass / (c1.angularMass + c2.angularMass);
            float angularPush2 = c2.angularMass / (c1.angularMass + c2.angularMass);

            float linearVel = linDot1 * (push2) + linDot2 * (push1);
            float angularVel = angDot1 * angularPush1 + angDot2 * angularPush2;
            float vel = linearVel + angularVel;

            c1.rigidbody4.ApplyStaticForce(-contactNormal, -vel, contact, bounce);
            c2.rigidbody4.ApplyStaticForce(contactNormal, vel, contact, bounce);
        }

        return true;
        */
    }

    struct ContactData
    {
        public float overlap;
        public Vector4 contact1;
        public Vector4 contact2;
        public Vector4 contactNormal;
    }

    static ContactData FindCollisionType(ColliderS col1, ColliderS col2)
    {
        int type1 = col1.colliderType;
        int type2 = col2.colliderType;

        bool swap = false;
        if (type1 > type2) {
            (col1,col2) = (col2,col1);
            (type1,type2) = (type2,type1);
            swap = true;
        }
        
        Vector4 con1 = new Vector4();
        Vector4 con2 = new Vector4();
        Vector4 conNorm = new Vector4();

        float over = Find();

        ContactData outC = new ContactData()
        {
            overlap = over,
            contact1 = con1,
            contact2 = con2,
            contactNormal = conNorm
        };

        if (swap)
        {
            outC.contact1 = con2;
            outC.contact2 = con1;
            outC.contactNormal *= -1;
        }

        return outC;

        float Find()
        {
            switch (type1) {
                default: //sphere
                    return SphereOn(col1.sphere, col2, ref con1, ref con2, ref conNorm);
                case 1: //capsule
                    return CapsuleOn(col1.capsule, col2, ref con1, ref con2, ref conNorm);
                    /*
                    switch (type2) {
                        default: //capsule-capsule
                            return CapsuleOnCapsule(col1.capsule, col2.capsule, ref con1, ref con2, ref conNorm);
                        case 2:
                            return CapsuleOnTriangle(col1.capsule, col2.triangle, ref con1, ref con2, ref conNorm);
                        case 3: //sphere-triangle
                            return CapsuleOnMesh(col1.capsule, col2.mesh, ref con1, ref con2, ref conNorm);
                    }
                    */
                case 3: //mesh
                    if (type2 == 3)
                    {
                        return MeshOnMesh(col1.mesh, col2.mesh, ref con1, ref con2, ref conNorm);
                    } 
                    else
                    {
                        return 0;
                    }
            }
        }
    }

    public static float PointRadiusContact(Vector4 p1, float r1, Vector4 p2, float r2, ref Vector4 contact1, ref Vector4 contact2, ref Vector4 contactNorm)
    {
        //find angle between positions of both
        float dot = UFunc.Dot(p1, p2);
        float dis = Mathf.Acos(Mathf.Clamp(dot,-1,1));

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

    public static float CapsuleOn(CapsuleColliderS c1, ColliderS c2, ref Vector4 contact1, ref Vector4 contact2, ref Vector4 contactNorm)
    {
        Vector4 close1 = new Vector4();
        Vector4 close2 = new Vector4();

        c2.LineClose(c1.point1, c1.point2, ref close1, ref close2);

        return PointRadiusContact(close1, c1.m_radius, close2, c2.m_radius, ref contact1, ref contact2, ref contactNorm);
    }

    public static float CapsuleOnTriangle(CapsuleColliderS c1, TriColliderS c2, ref Vector4 contact1, ref Vector4 contact2, ref Vector4 contactNorm)
    {
        Vector4 linePoint = new Vector4();
        Vector4 triPoint = new Vector4();
        c2.LineClose(c1.point1, c1.point2, ref linePoint, ref triPoint);

        return PointRadiusContact(linePoint, c1.m_radius, triPoint, 0, ref contact1, ref contact2, ref contactNorm);
    }

    public static float CapsuleOnMesh(CapsuleColliderS c1, MeshColliderS c2, ref Vector4 contact1, ref Vector4 contact2, ref Vector4 contactNorm)
    {
        Vector4 linePoint = new Vector4();
        Vector4 meshPoint = new Vector4();

        c2.LineClose(c1.point1, c1.point2, ref linePoint, ref meshPoint);

        return PointRadiusContact(linePoint, c1.m_radius, meshPoint, 0, ref contact1, ref contact2, ref contactNorm);
    }

    public static float MeshOnMesh(MeshColliderS c1, MeshColliderS c2, ref Vector4 contact1, ref Vector4 contact2, ref Vector4 contactNorm)
    {
        return 0;
    }
}
