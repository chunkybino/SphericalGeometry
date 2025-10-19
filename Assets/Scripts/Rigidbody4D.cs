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
    public Vector4 angularVelocity4
    {
        get { return transform4.matrix * new Vector4(angularVelocity.x, angularVelocity.y, angularVelocity.z, 0); }
        set
        {
            angularVelocity = transform4.RelativeDirectionTo(value);
        }
    }

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
    public float angularFrictionMult = 0;

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
    public bool kineticStatic; //lets the rb move with velocity even while static

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
    //[SerializeField] List<Vector4> staticContactPoints = new List<Vector4>();
    //[SerializeField] List<float> staticContactVels = new List<float>();

    bool moveThisFixedFrame; //did we mupdate our transform this fixed frame

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

    void OnDisable()
    {
        physicsS?.RemoveRigidbody(this);

        transform4.onLeftMult.RemoveListener(OnTransformLeftMult);
        transform4.onMatrixUpdate.RemoveListener(OnMatrixUpdate);
        transform4.onRotorLeft.RemoveListener(OnRotor);
    }

    public void PhysicsUpdate()
    {
        if (isStatic) 
        {
            if (kineticStatic) 
            {
                MoveTangent(velocity * Time.fixedDeltaTime);
                if (angularVelocity != Vector3.zero) Rotate(angularVelocity * Time.fixedDeltaTime);
            }
            else
            {
                velocity = Vector4.zero;
            }
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
        //staticContactPoints.Clear();
        //staticContactVels.Clear();
    }
    public void PhysicsUpdate2()
    {
        collider?.PhysicsUpdate2();

        if (!globalSector && moveThisFixedFrame) {
            moveThisFixedFrame = false;
            physicsS?.UpdateRigidbodySector(this);
        }
    }

    void DoGravity()
    {
        Vector4 tangentGravity = UFunc.ProjectToVectorNormal(gravity, positionNorm);
        tangentGravity = tangentGravity.normalized * gravity.magnitude * gravityScale;

        //friction
        for (int i = 0; i < staticContactNormals.Count; i++)
        {
            if (friction == 0) continue;

            float normDot = Vector4.Dot(tangentGravity, staticContactNormals[i]);
            normDot = Mathf.Max(-normDot, 0);

            Vector4 tangentVel = UFunc.ProjectToVectorNormal(velocity,staticContactNormals[i]);
 
            velocity += -tangentVel.normalized * Mathf.Min(normDot * friction * Time.fixedDeltaTime, tangentVel.magnitude);

            Vector3 normal3 = transform4.RelativeDirectionTo(staticContactNormals[i]).normalized;
            Vector3 allignAngular = normal3*Vector3.Dot(angularVelocity,normal3);

            angularVelocity -= allignAngular.normalized * Mathf.Min(normDot * friction*angularFrictionMult * Time.fixedDeltaTime, allignAngular.magnitude);
        }

        velocity += tangentGravity * Time.fixedDeltaTime;

        velocity = UFunc.ProjectToVectorNormal(velocity, positionNorm).normalized * velocity.magnitude; //make sure its tangent just incase we pick up some imprecision along the way
    }

    public void SetVelocity(Vector4 vel)
    {
        velocity = UFunc.ProjectToVectorNormal(vel, positionNorm);
    }
    public void SetVelocityTowards(Vector4 target, float vel)
    {
        Vector4 currentTangent = velocity; //velocityR.RotateFull(positionNorm) * velocityR.angle;
        Vector4 tangentVel = UFunc.ProjectToVectorNormal(target, positionNorm).normalized;
        currentTangent += tangentVel * (vel - UFunc.Dot(currentTangent, tangentVel));

        velocity = UFunc.ProjectToVectorNormal(velocity, position4).normalized * velocity.magnitude; //make sure its tangent just incase we pick up some imprecision along the way

        velocity = currentTangent;
    }

    public void SetRelativeVelocity(Vector3 vel)
    {
        velocity = new Vector4();
        for (int i = 0; i < 3; i++) {
            velocity += vel[i]*transform4.matrix.GetColumn(i);
        }
    }
    public void SetRelativeVelocity(float vel, int axisIndex)
    {
        SetVelocityTowards(transform4.GetBasis(axisIndex), vel);
    }
    public void SetRelativeVelocityX(float vel) {
        SetRelativeVelocity(vel, 0);
    }
    public void SetRelativeVelocityY(float vel) {
        SetRelativeVelocity(vel, 1);
    }
    public void SetRelativeVelocityZ(float vel) {
        SetRelativeVelocity(vel, 2);
    }
    public Vector3 GetRelativeVelocity()
    {
        Vector3 outV = new Vector3();
        for (int i = 0; i < 3; i++) {
            outV[i] = Vector4.Dot(velocity, transform4.matrix.GetColumn(i));
        }
        return outV;
    }
    public Vector2 GetRelativeVelocityXZ()
    {
        return new Vector2(GetRelativeVelocity(0), GetRelativeVelocity(2));
    }
    public float GetRelativeVelocity(int component)
    {
        return Vector4.Dot(velocity, transform4.matrix.GetColumn(component));
    }

    public void SetAngularVelocity(Vector3 ang)
    {
        angularVelocity = ang;
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
        Rotor rot = new Rotor(positionNorm,anchor);
        Vector4 directionTo = UFunc.DirectionFromTo(positionNorm,anchor).normalized;

        return rot * (velocity + UFunc.HyperCross(directionTo,angularVelocity4,positionNorm));
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
    public void AddAngularVelocityAtAnchor(Vector4 vel, Vector4 anchor)
    {
        float distance = UFunc.DistanceS(transform4.positionNorm, anchor);
        Vector3 vel3 = transform4.RelativeDirectionTo(vel).normalized;
        Vector3 anchor3 = transform4.RelativeDirectionTo(anchor).normalized;
        Vector3 axis = Vector3.Cross(vel3,anchor3);

        Matrix4x4 mat = transform4.matrix.transpose;

        Vector4 localVel = (mat * (new Rotor(anchor,positionNorm) * vel)).normalized;
        Vector4 localAnchor = (mat * anchor);
        Vector3 localVel3 = new Vector3(localVel.x,localVel.y,localVel.z).normalized;
        Vector3 localAnchor3 = new Vector3(localAnchor.x,localAnchor.y,localAnchor.z).normalized;

        Vector3 cross3 = Vector3.Cross(localVel3, localAnchor3).normalized;

        angularVelocity += axis * vel.magnitude / distance;
    }
    public void ApplyStaticForce(Vector4 direction, float attackVel, Vector4 attackPoint, float elasticity)//, bool doFriction = true)
    {
        Vector4 currentLinear = GetLinearVelocityAtAnchor(attackPoint);
        float linearDot = Vector4.Dot(direction, currentLinear);

        //if (doFriction) ApplyFriction(attackPoint, (attackVel-linearDot)*direction);

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

        if (relativeLinear < 0) return;

        float sinA = Vector4.Dot(direction, attackPerpDir);

        float pointAngularInertia = (angularMassMult/(distance*distance * sinA*sinA));

        float linearMassPush = 1 / (1 + pointAngularInertia);
        float angularMassPush = sinA == 0 ? 1 : pointAngularInertia / (1 + pointAngularInertia);

        float attackLinearVel = relativeLinear*(1-linearMassPush);
        float attackAngularVel = relativeLinear*(1-angularMassPush);

        float newAngularDot = (1+elasticity)*attackAngularVel;
        if (angularNormalDot > newAngularDot) newAngularDot = angularPerpDot;
        AddAngularVelocityAtAnchor(direction*(newAngularDot), attackPoint);

        float newLinearDot = (1+elasticity)*attackLinearVel + linearDot;
        if (linearDot > newLinearDot) newLinearDot = linearDot;
        Vector4 newLinear = GetFinalVel(newLinearDot);
        SetLinearVelocityAtAnchor(newLinear, attackPoint);

        Vector4 GetFinalVel(float newDot)
        {
            return UFunc.SetVectorDirectionValue(currentLinear, direction, newDot);
        }
    }

    public void AddImpulse(Vector4 impulse, Vector4 attackPoint)
    {
        if (isStatic) return;

        Vector4 toAttackPoint = UFunc.DirectionFromTo(positionNorm, attackPoint).normalized * UFunc.DistanceS(positionNorm, attackPoint);
        impulse = new Rotor(attackPoint, positionNorm) * impulse;

        Vector4 direction = impulse.normalized;

        Vector4 angularAlign = UFunc.HyperCross(toAttackPoint, direction, positionNorm);

        float currentVel = Vector4.Dot(velocity,direction) + Vector4.Dot(angularVelocity4, angularAlign);
        float impulseNeed = -currentVel / (1/mass + angularAlign.sqrMagnitude/angularMass);
        //impulse = direction * impulseNeed;

        Vector4 linearAdd = impulse/mass;
        Vector4 angularAdd = UFunc.HyperCross(toAttackPoint, impulse, positionNorm)/angularMass;

        if (!dontReciveAngularVelocity)
        {
            print(impulse.magnitude+" "+impulseNeed);
            print(currentVel+" current "+(1/mass + angularAlign.sqrMagnitude/angularMass)+" "+angularAlign.sqrMagnitude);
            print(angularVelocity4+" "+angularAlign+" "+angularAdd+" "+(-angularAdd));

            Vector4 velAtPoint = GetVelocityAtAnchor(attackPoint);
            print(velAtPoint+" "+velAtPoint.magnitude);

            velocity += linearAdd;
            angularVelocity4 += angularAdd;

            velAtPoint = GetVelocityAtAnchor(attackPoint);
            print(velAtPoint+" "+UFunc.Dot(velAtPoint,impulse.normalized));

            return;
        }

        velocity += linearAdd;
        angularVelocity4 += angularAdd;
    }

    public void CollidePointMass(Vector4 attackPoint, Vector4 attackVel, float attackMass)
    {
        if (isStatic) return;

        float totalMass = mass + attackMass;
        float mass1 = mass / totalMass;
        float mass2 = attackMass / totalMass;

        Vector4 centroid = UFunc.Slerp4(positionNorm, attackPoint, mass1);

        float thisCentroidDis = UFunc.DistanceS(positionNorm, centroid);
        float attackCentroidDis = UFunc.DistanceS(attackPoint, centroid);
        Vector4 directionCentroidToAttack = UFunc.DirectionFromTo(centroid, attackPoint).normalized * attackCentroidDis;

        Vector4 attackVelCentroid = new Rotor(attackPoint, centroid) * attackVel;
        Vector4 thisVelCentroid = GetLinearVelocityAtAnchor(centroid);

        Vector4 totalLinearMomentum = thisVelCentroid * mass1 + attackVelCentroid * mass2;

        Vector4 thisAngularCentroid = new Rotor(positionNorm, centroid) * angularVelocity4;
        float thisAngularMass = mass1 * (angularMassMult + thisCentroidDis * thisCentroidDis);

        Vector4 attackAngularMomentum = mass2 * UFunc.HyperCross(directionCentroidToAttack, attackVelCentroid, centroid);

        Vector4 totalAngularMomentum = thisAngularCentroid * thisAngularMass + attackAngularMomentum;
        float totalAngularMass = thisAngularMass + mass2 * attackCentroidDis * attackCentroidDis;

        Vector4 linearVel = new Rotor(centroid, positionNorm) * totalLinearMomentum;
        Vector4 angularVel = new Rotor(centroid, positionNorm) * totalAngularMomentum / totalAngularMass;

        velocity = linearVel;
        angularVelocity4 = angularVel;
    }
    

    public void AddStaticContact(Vector4 normal, float vel, Vector4 pos)
    {
        staticContactNormals.Add(new Rotor(pos, positionNorm) * normal);
        //staticContactPoints.Add(pos);
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
        moveThisFixedFrame = true;
        //if (!globalSector) physicsS?.UpdateRigidbodySector(this);
    }

    //bad function dont use this
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
 