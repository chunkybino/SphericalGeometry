using UnityEngine;

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

    float radius {get{return Transform4D.radius;}}

    public Vector3 velocity;
    public Vector3 angularVelocity;

    public float gravityScale;
    public Vector3 gravity = new Vector3(0,-1,0);

    [SerializeField] bool doYPlaneBound;
    [SerializeField] float doYPlaneBoundVal;

    [SerializeField] bool doYUpLock; //lock the cameras orientation to up is always toawrds the y axis

    void Awake()
    {
        if (!transform4) transform4 = GetComponent<Transform4D>();
    }

    void FixedUpdate()
    {
        if (doYPlaneBound) CheckYBound();
        if (doYUpLock) YUpLock();

        if (velocity != Vector3.zero) Move(velocity * Time.fixedDeltaTime);

        velocity += gravity * gravityScale;

        if (angularVelocity != Vector3.zero) Rotate(angularVelocity * Time.fixedDeltaTime);
    }

    void Move(Vector3 moveAmount)
    {
        transform4.MoveRelative(moveAmount);

        if (doYPlaneBound) CheckYBound();
        if (doYUpLock) YUpLock();
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
 