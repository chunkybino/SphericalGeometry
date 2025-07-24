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

    public float gravityScale;
    public Vector4 gravity = new Vector4(0,-1,0,0);
    Vector4 tangentGravity;

    public float mass = 1;
    public float bounce = 0;

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

    [SerializeField] bool doYPlaneBound;
    [SerializeField] float doYPlaneBoundVal;

    [SerializeField] bool doYUpLock; //lock the cameras orientation to up is always toawrds the y axis

    void Awake()
    {
        if (!transform4) transform4 = GetComponent<Transform4D>();
        if (!collider) collider = GetComponent<ColliderS>();

        transform4.onLeftMult.AddListener(OnTransformLeftMult);
    }
    void Start()
    {
        PhysicsHandlerS.singleton?.AddRigidbody(this);
    }

    public void PhysicsUpdate()
    {
        if (isStatic) {
            velocity = Vector4.zero;
            return;
        }

        if (doYPlaneBound) CheckYBound();
        if (doYUpLock) YUpLock();

        DoGravity();
        MoveTangent(velocity * Time.fixedDeltaTime);

        if (angularVelocity != Vector3.zero) Rotate(angularVelocity * Time.fixedDeltaTime);

        velocityMagnitude = velocity.magnitude;
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

        if (doYPlaneBound) CheckYBound();
        if (doYUpLock) YUpLock();
    }

    public Vector4 GetVelocityAtAnchor(Vector4 anchor)
    {
        return UFunc.RotateTowardsMatrix(positionNorm, anchor) * velocity;
    }
    public void AddVelocityAtAnchor(Vector4 vel, Vector4 anchor)
    {
        Matrix4x4 mat = UFunc.RotateTowardsMatrix(anchor, positionNorm);
        velocity += mat * vel;
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

    void Rotate(Vector3 rotateAmount)
    {
        transform4.RotateRelativeXZ(rotateAmount.y);
        transform4.RotateRelativeYZ(rotateAmount.x);
        transform4.RotateRelativeXY(rotateAmount.z);
    }

    void CheckYBound()
    {
        if (position4.y < doYPlaneBoundVal)
        {
            Vector4 targetPos = position4;
            targetPos.y = doYPlaneBoundVal;

            targetPos *= Mathf.Sqrt((1 - doYPlaneBoundVal*doYPlaneBoundVal) / new Vector3(targetPos.x, targetPos.z, targetPos.w).magnitude);

            float moveAmount = UFunc.AngleBetweenVectors(position4, targetPos);

            transform4.matrix = UFunc.RotateTowardsMatrix(position4, targetPos, -moveAmount) * transform4.matrix;

            velocity.y = Mathf.Max(0, velocity.y);
        }
    }

    void YUpLock()
    {
        if (transform4.xBasis.y != 0)
        {
            //zero the y component of xBasis
            Vector4 XIntersectY = UFunc.LineYIntersect(transform4.xBasis, transform4.yBasis, 0);
            float angle = -UFunc.AngleBetweenVectors(transform4.xBasis, XIntersectY);
            if (Mathf.Sign(transform4.xBasis.y) == Mathf.Sign(transform4.yBasis.y)) angle *= -1;
            transform4.matrix = transform4.matrix * UFunc.MatXYRot(angle);
        }

        if (transform4.zBasis.y != 0)
        {
            //zero the y component of xBasis
            Vector4 ZIntersectY = UFunc.LineYIntersect(transform4.zBasis, transform4.yBasis, 0);
            float angle = -UFunc.AngleBetweenVectors(transform4.zBasis, ZIntersectY);
            if (Mathf.Sign(transform4.zBasis.y) == Mathf.Sign(transform4.yBasis.y)) angle *= -1;
            transform4.matrix = transform4.matrix * UFunc.MatZYRot(angle);
        }
    }
}
 