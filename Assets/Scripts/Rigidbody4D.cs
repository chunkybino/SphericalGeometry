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
    public float velocityMagnitude;
    public float angularVelocityMagnitude;

    public float gravityScale;
    public Vector4 gravity = new Vector4(0,-1,0,0);
    Vector4 tangentGravity;

    public float mass = 1;
    public float bounce = 0;

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

    public ColliderS collider;

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
    }

    public void PhysicsUpdate()
    {
        if (isStatic) {
            velocity = Vector4.zero;
            return;
        }

        if (doYUpLock) YUpLock();

        DoGravity();
        MoveTangent(velocity * Time.fixedDeltaTime);

        if (angularVelocity != Vector3.zero) Rotate(angularVelocity * Time.fixedDeltaTime);

        velocityMagnitude = velocity.magnitude;
        angularVelocityMagnitude = angularVelocity.magnitude;
    }

    void DoGravity()
    {
        tangentGravity = UFunc.ProjectToVectorNormal(gravity, positionNorm);
        tangentGravity = tangentGravity.normalized * gravity.magnitude * gravityScale;

        velocity += tangentGravity * Time.fixedDeltaTime;

        velocity = UFunc.ProjectToVectorNormal(velocity, positionNorm).normalized * velocity.magnitude; //make sure its tangent just incase we pick up some imprecision along the way
    }

    public void SetVelocityTowards(Vector4 target, float vel)
    {
        Vector4 tangetVel = UFunc.ProjectToVectorNormal(target, positionNorm).normalized;
        velocity += tangetVel * (vel - UFunc.Dot(velocity, tangetVel));

        velocity = UFunc.ProjectToVectorNormal(velocity, position4).normalized * velocity.magnitude; //make sure its tangent just incase we pick up some imprecision along the way
    }

    public void MoveTangent(Vector4 moveVel)
    {
        Vector4 target = (positionNorm + moveVel).normalized;

        Matrix4x4 mat = UFunc.RotateTowardsMatrix(positionNorm, target, -moveVel.magnitude / radius);

        transform4.LeftMult(mat);
    }

    public void MoveFromAnchor(Vector4 moveVel, Vector4 anchor) //move, but with the movement anchored to a point other than the center
    {
        Vector4 target = (anchor + moveVel).normalized;

        Matrix4x4 mat = UFunc.RotateTowardsMatrix(anchor, target, -moveVel.magnitude / radius);

        transform4.LeftMult(mat);
    }

    void MoveRelative(Vector3 moveAmount)
    {
        transform4.MoveRelative(moveAmount);

        if (doYUpLock) YUpLock();
    }

    public Vector4 GetLinearVelocityAtAnchor(Vector4 anchor)
    {
        return UFunc.RotateTowardsMatrix(positionNorm, anchor) * velocity;
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

        //print(gameObject.name+" "+angularDir3+" "+angularDir+" "+angular);
        //print(angularVelocity.magnitude+" "+anchor3.magnitude);

        return linear + angular;
    }

    public void AddLinearVelocityAtAnchor(Vector4 vel, Vector4 anchor)
    {
        velocity += UFunc.RotateTowardsMatrix(anchor, positionNorm) * vel;
    }
    public void AddVelocityAtAnchor(Vector4 vel, Vector4 anchor)
    {
        AddLinearVelocityAtAnchor(vel, anchor);
        return;

        if (dontReciveAngularVelocity)
        {
            AddLinearVelocityAtAnchor(vel, anchor);
            return;
        }
    }
    public void SetLinearVelocityAtAnchor(Vector4 vel, Vector4 anchor)
    {
        velocity = UFunc.RotateTowardsMatrix(anchor, positionNorm) * vel;
    }
    public void SetAngularVelocityAtAnchor(Vector4 vel, Vector4 anchor)
    {
        float distance = UFunc.DistanceS(transform4.positionNorm, anchor);
        Vector3 vel3 = transform4.RelativeDirectionTo(vel);
        Vector3 anchor3 = transform4.RelativeDirectionTo(anchor);
        Vector3 axis = Vector3.Cross(vel3,anchor3).normalized;

        angularVelocity = axis * vel.magnitude / distance;
    }

    public void SetVelocityAtAnchorNormal(Vector4 anchor, Vector4 normal, float elasticity = 0)
    {   
        Vector4 currentLinear = GetLinearVelocityAtAnchor(anchor);
        float linearDot = Vector3.Dot(currentLinear, normal);
        Vector4 currentAngular = -GetAngularVelocityAtAnchor(anchor);
        float angularDot = Vector3.Dot(currentAngular, normal);

        if (dontReciveAngularVelocity)
        {
            SetLinearVelocityAtAnchor(currentLinear - linearDot*normal*(1+elasticity), anchor);
            return;
        }

        float distance = UFunc.DistanceS(anchor, positionNorm);

        Vector4 normalVel = -(linearDot+angularDot)*normal * (1+elasticity);

        Vector4 centerDir = UFunc.ProjectToVectorNormal(positionNorm-anchor, anchor).normalized;
        Vector4 centerVel = centerDir*Vector4.Dot(normalVel, centerDir);
        Vector4 perpVel = normalVel - centerVel;

        Vector4 newLinear = currentLinear + centerVel;
        Vector4 newAngular = currentAngular + perpVel;

        SetLinearVelocityAtAnchor(newLinear, anchor);
        SetAngularVelocityAtAnchor(newAngular, anchor);

        print(gameObject.name+" "+linearDot+" "+angularDot);
        print(normal+" "+normalVel);
        print(centerVel+" "+perpVel);
        print(currentLinear+" "+newLinear+" "+currentLinear.magnitude+" "+newLinear.magnitude);
        print(currentAngular+" "+newAngular+" "+currentAngular.magnitude+" "+newAngular.magnitude);
        print((currentAngular+currentLinear)+" "+(newLinear+newAngular)+" "+(currentAngular+currentLinear).magnitude+" "+(newLinear+newAngular).magnitude);
        print(Vector4.Dot((newLinear+newAngular), normal));

        //(-0.10, 0.04, 0, 0) -0.04
        //
    }

    public void SetRelativeVelocityAxis(float vel, int axisIndex)
    {
        Vector4 velVec = transform4.GetBasis(axisIndex);
        float dot = UFunc.Dot(velVec, velocity);
        velocity += (-dot+vel) * velVec;
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

    void OnTransformLeftMult(Matrix4x4 mat)
    {
        velocity = mat * velocity;
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
 