using UnityEngine;
using System.Collections.Generic;

[ExecuteInEditMode]
public class Rigidbody4D : MonoBehaviour
{
    public Transform4D transform4;
    Vector4 position4 {
        get{
            if (transform4) return transform4.position;
            return new Vector4(0,0,0,1);
        }
    }
    Vector4 positionNorm {
        get{
            if (transform4) return transform4.positionNorm;
            return new Vector4(0,0,0,1);
        }
    }

    float radius {get{return Transform4D.radius;}}

    public Vector4 velocity;
    public Vector3 angularVelocity;

    [SerializeField] float velocityMagnitude;
    [SerializeField] float angularVelocityMagnitude;

    [SerializeField] float linearMomentum;
    [SerializeField] float angularMomentum;
    [SerializeField] float totalMomentum;

    public float gravityScale;
    public Vector4 gravity = new Vector4(0,-1,0,0);

    public float mass = 1;
    public float angularMassMult = 1;
    public float angularMass {get{return mass*angularMassMult;}}

    public float bounce = 0;

    public float friction = 0;

    public bool dontReciveAngularVelocity;

    [HideInInspector] [SerializeField] bool m_isStatic;
    public bool isStatic {
        get {
            return m_isStatic;
        }
        set {
            if (value != m_isStatic) {
                m_isStatic = value;
                PhysicsHandlerS.singleton?.UpdateRigidbodyStatic(this);
            } else {
                m_isStatic = value;
            }
        }
    }

    public bool dontDoVelocity;

    new public ColliderS collider;

    [SerializeField] bool doYUpLock; //lock the cameras orientation to up is always toawrds the y axis

    public int sectorIndex = -1;
    public List<int> allSectors = new List<int>(); //if we overlap into multiple sectors cause radius
    public bool globalSector;

    //used for sectoring in physics, object gets added to sectors in this radius of it
    public float boundingRadius {get{
        if (collider) return collider.boundingRadius;
        return 0;
    }}

    PhysicsHandlerS physicsS;

    [SerializeField] List<Vector4> staticContactNormals = new List<Vector4>();
    [SerializeField] List<float> staticContactVels = new List<float>();
    void Awake()
    {
        if (!transform4) transform4 = GetComponent<Transform4D>();
        if (!collider) collider = GetComponent<ColliderS>();

        physicsS = PhysicsHandlerS.singleton;
    }
    void OnEnable()
    {
        physicsS?.AddRigidbody(this);

        transform4.onLeftMult.AddListener(OnTransformLeftMult);
        transform4.onMatrixUpdate.AddListener(OnMatrixUpdate);
        transform4.onRotorLeft.AddListener(OnRotor);
    }

    public void PhysicsUpdate()
    {
        if (isStatic) {
            velocity = Vector4.zero;
            return;
        }

        if (doYUpLock) YUpLock();

        if (!dontDoVelocity)
        {
            DoGravity();

            velocityMagnitude = velocity.magnitude;
            angularVelocityMagnitude = angularVelocity.magnitude;

            linearMomentum = velocityMagnitude*mass;
            angularMomentum = angularVelocityMagnitude*angularMass;
            totalMomentum = linearMomentum+angularMomentum;

            MoveTangent(velocity * Time.fixedDeltaTime);
            if (angularVelocity != Vector3.zero) Rotate(angularVelocity * Time.fixedDeltaTime);
        }

        collider?.PhysicsUpdate();

        staticContactNormals.Clear();
        staticContactVels.Clear();}
    public void PhysicsUpdate2()
    {
        collider?.PhysicsUpdate2();
    }

    void DoGravity()
    {
        Vector4 tangentGravity = UFunc.ProjectToVectorNormal(gravity, positionNorm);
        tangentGravity = tangentGravity.normalized * gravity.magnitude * gravityScale;
        for (int i = 0; i < staticContactNormals.Count; i++) {
            if (Vector4.Dot(tangentGravity,staticContactNormals[i]) < 0) {
                tangentGravity = UFunc.SetVectorDirectionValue(tangentGravity, staticContactNormals[i], 0);
            }
        }

        velocity += tangentGravity * Time.fixedDeltaTime;

        velocity = UFunc.ProjectToVectorNormal(velocity, positionNorm).normalized * velocity.magnitude; //make sure its tangent just incase we pick up some imprecision along the way
    }

    public void SetVelocityTowards(Vector4 target, float vel)
    {
        Vector4 currentTangent = velocity; //velocityR.RotateFull(positionNorm) * velocityR.angle;
        Vector4 tangentVel = UFunc.ProjectToVectorNormal(target, positionNorm).normalized;
        currentTangent += tangentVel * (vel - UFunc.Dot(currentTangent, tangentVel));

        velocity = UFunc.ProjectToVectorNormal(velocity, position4).normalized * velocity.magnitude; //make sure its tangent just incase we pick up some imprecision along the way

        velocity = currentTangent;
    }

    public void MoveRotor(Rotor rot)
    {
        transform4.MoveRotor(rot);
    }
    public void MoveRotor(Rotor rot, float angleMult)
    {
        rot = angleMult * rot;
        transform4.MoveRotor(rot);
    }
    public void MoveTangent(Vector4 moveVel)
    {
        transform4.MoveTangent(moveVel);
    }

    void MoveRelative(Vector3 moveAmount)
    {
        transform4.MoveRelative(moveAmount);

        if (doYUpLock) YUpLock();
    }

    public Vector4 GetLinearVelocityAtAnchor(Vector4 anchor)
    {
        Vector4 vel = new Rotor(positionNorm, anchor) * velocity;
        if (vel.magnitude > 0) vel *= velocity.magnitude/vel.magnitude;
        return vel;
    }
    public Vector4 GetAngularVelocityAtAnchor(Vector4 anchor)
    {
        Vector4 anchorRel = transform4.matrix.transpose * anchor;
        Vector3 anchor3 = new Vector3(anchorRel.x,anchorRel.y,anchorRel.z).normalized;

        Vector3 angularDir3 = Vector3.Cross(angularVelocity.normalized, anchor3);
        Vector4 angularDir = transform4.matrix * new Vector4(angularDir3.x,angularDir3.y,angularDir3.z,0);

        return angularDir * angularVelocity.magnitude * UFunc.DistanceS(positionNorm,anchor);
    }
    public Vector4 GetVelocityAtAnchor(Vector4 anchor)
    {
        Matrix4x4 mat = UFunc.RotateTowardsMatrix(positionNorm, anchor);

        Vector4 linear = mat * velocity;

        Vector4 anchorRel = transform4.matrix.transpose * anchor;
        Vector3 anchor3 = new Vector3(anchorRel.x,anchorRel.y,anchorRel.z);

        if (angularVelocityMagnitude == 0) return linear;

        Vector3 angularDir3 = Vector3.Cross(angularVelocity, anchor3);
        Vector4 angularDir = new Vector4(angularDir3.x,angularDir3.y,angularDir3.z,0).normalized;

        Vector4 angular = angularDir * angularVelocity.magnitude * UFunc.DistanceS(positionNorm,anchor);

        return linear + angular;
    }

    public void AddLinearVelocityAtAnchor(Vector4 vel, Vector4 anchor)
    {
        velocity += UFunc.RotateTowardsMatrix(anchor, positionNorm) * vel;
    }
    public void SetLinearVelocityAtAnchor(Vector4 vel, Vector4 anchor)
    {
        velocity = UFunc.RotateTowardsMatrix(anchor, positionNorm) * vel;
    }
    public void SetLinearVelocityInDirection(Vector4 direction, float vel, Vector4 anchor)
    {
        direction = UFunc.RotateTowardsMatrix(anchor, positionNorm) * direction;
        velocity = velocity + (vel-Vector4.Dot(direction,velocity))*direction;
    }
    public void SetAngularVelocityAtAnchor(Vector4 vel, Vector4 anchor)
    {
        float distance = UFunc.DistanceS(transform4.positionNorm, anchor);
        Vector3 vel3 = transform4.RelativeDirectionTo(vel);
        Vector3 anchor3 = transform4.RelativeDirectionTo(anchor);
        Vector3 axis = Vector3.Cross(vel3,anchor3).normalized;

        angularVelocity = axis * vel.magnitude / distance;
    }
    public void ApplyStaticForce(Vector4 direction, float attackVel, Vector4 attackPoint, float elasticity)
    {
        Vector4 currentLinear = GetLinearVelocityAtAnchor(attackPoint);
        float linearDot = Vector4.Dot(direction, currentLinear);

        if (dontReciveAngularVelocity) {
            float newVelDot = -(linearDot-attackVel)*elasticity + attackVel;
            Vector4 newVel = GetFinalVel(newVelDot);
            SetLinearVelocityAtAnchor(newVel,attackPoint);
            return;
        }

        Vector4 centerDir = UFunc.ProjectToVectorNormal(positionNorm-attackPoint,attackPoint).normalized;
        Vector4 attackPerpDir = UFunc.ProjectToVectorNormal(direction,centerDir).normalized;

        Vector4 currentAngular = -GetAngularVelocityAtAnchor(attackPoint);
        float angularPerpDot = Vector4.Dot(attackPerpDir, currentAngular);
        float angularNormalDot = Vector4.Dot(direction, currentAngular);

        float distance = UFunc.DistanceS(positionNorm, attackPoint);

        float relativeLinear = attackVel - linearDot - angularNormalDot;
        float sinA = Vector4.Dot(direction, attackPerpDir);

        float linearMassPush = 1 / (1 + (angularMassMult/(distance*distance))/sinA);
        float angularMassPush = sinA == 0 ? 1 : (angularMassMult/(distance*distance))/sinA / (1 + (angularMassMult/(distance*distance))/sinA);

        float attackLinearVel = relativeLinear*(1-linearMassPush);
        float attackAngularVel = relativeLinear*(1-angularMassPush);

        float newAngularDot = (1+elasticity)*attackAngularVel + angularPerpDot;
        if (angularPerpDot > attackAngularVel) newAngularDot = angularPerpDot;
        Vector4 newAngular = UFunc.SetVectorDirectionValue(currentAngular, attackPerpDir, newAngularDot);
        SetAngularVelocityAtAnchor(newAngular, attackPoint);

        float newLinearDot = (1+elasticity)*attackLinearVel + linearDot;
        if (relativeLinear < 0) newLinearDot = linearDot;
        Vector4 newLinear = GetFinalVel(newLinearDot);
        SetLinearVelocityAtAnchor(newLinear, attackPoint);

        Vector4 GetFinalVel(float newDot)
        {
            if (elasticity != 1 && Mathf.Abs(newDot) < 0.1f)
            {
                newDot = Mathf.MoveTowards(newDot, 0, 1*Time.fixedDeltaTime);
                //if (Mathf.Abs(newDot) < 0.01f) newDot = 0;
            }
            if (velocity.magnitude == 0 || UFunc.CloseTo(Mathf.Abs(linearDot/velocity.magnitude),1,0.05f))
            {
                //return direction * newDot;
            }
            return UFunc.SetVectorDirectionValue(currentLinear, direction, newDot);
        }
    }
    public void AddStaticContact(Vector4 normal, float vel, Vector4 pos)
    {
        staticContactNormals.Add(new Rotor(pos, positionNorm) * normal);
    }

    public void SetRelativeVelocityAxis(float vel, int axisIndex)
    {
        SetVelocityTowards(transform4.GetBasis(axisIndex), vel);
    }
    public void SetRelativeVelocityX(float vel) {
        SetRelativeVelocityAxis(vel, 0);
    }
    public void SetRelativeVelocityY(float vel) {
        SetRelativeVelocityAxis(vel, 1);
    }
    public void SetRelativeVelocityZ(float vel) {
        SetRelativeVelocityAxis(vel, 2);
    }

    public void OnRotor(Rotor r)
    {
        //velocityR.TranslateRotor(r);
        velocity = r * velocity;
    }
    void OnTransformLeftMult(Matrix4x4 mat)
    {
        velocity = mat * velocity;
        //if (velocity.magnitude > 0) velocity *= velocityMagnitude/velocity.magnitude;
    }
    void OnMatrixUpdate(Matrix4x4 mat)
    {
        if (!globalSector) {
            physicsS?.UpdateRigidbodySector(this);
        }
    }

    void Rotate(Vector3 rotateAmount)
    {
        transform4.RotateRelativeXZ(-rotateAmount.y);
        transform4.RotateRelativeYZ(rotateAmount.x);
        transform4.RotateRelativeXY(rotateAmount.z);
    }

    void YUpLock()
    {
        if (transform4.xBasis.y != 0)
        {
            //zero the y component of xBasis
            Vector4 XIntersectY = UFunc.LineYIntersect(transform4.xBasis, transform4.yBasis, 0);
            float angle = -UFunc.VectorAngle(transform4.xBasis, XIntersectY);
            if (Mathf.Sign(transform4.xBasis.y) == Mathf.Sign(transform4.yBasis.y)) angle *= -1;
            transform4.matrix = transform4.matrix * UFunc.MatXYRot(angle);
        }

        if (transform4.zBasis.y != 0)
        { 
            Vector4 ZIntersectY = UFunc.LineYIntersect(transform4.zBasis, transform4.yBasis, 0);
            float angle = -UFunc.VectorAngle(transform4.zBasis, ZIntersectY);
            if (Mathf.Sign(transform4.zBasis.y) == Mathf.Sign(transform4.yBasis.y)) angle *= -1;
            transform4.matrix = transform4.matrix * UFunc.MatZYRot(angle);
        }
    }
}
 