using UnityEngine;

[ExecuteInEditMode]
public class GuyController4D : MonoBehaviour
{
    public Transform4D transform4;
    Vector4 position4 {
        get{
            if (transform4) return transform4.position;
            return new Vector4(0,0,0,1);
        }
    }

    float radius {get{return Transform4D.radius;}}

    [SerializeField] bool doMinY;
    [SerializeField] float minYPos;

    [SerializeField] bool doYUpLock; //lock the cameras orientation to up is always toawrds the y axis

    [SerializeField] bool check;

    void Awake()
    {
        if (!transform4) transform4 = GetComponent<Transform4D>();
    }

    void OnEnable()
    {
        transform4.onMove.AddListener(OnMove);
    }

    void Update()
    {
        if (doMinY) CheckMinY();

        if (doYUpLock) YUpLock();

        if (check)
        {
            check = false;
            YUpLock();
        }
    }

    void OnMove()
    {
        if (doMinY) CheckMinY();

        if (doYUpLock) YUpLock();
    }

    void CheckMinY()
    {
        if (position4.y < minYPos)
        {
            Vector4 targetPos = position4;
            targetPos.y = minYPos;

            targetPos *= Mathf.Sqrt((1 - minYPos*minYPos) / new Vector3(targetPos.x, targetPos.z, targetPos.w).magnitude);

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

        /*        
        if (transform4.zBasis.y != 0)
        {
            //zero the y component of zBasis
            Vector4 ZIntersectY = UFunc.LineYIntersect(transform4.zBasis, transform4.yBasis, 0);
            float angle = UFunc.AngleBetweenVectors(transform4.zBasis, ZIntersectY);
            print(ZIntersectY.normalized);
            if (Mathf.Sign(transform4.zBasis.y) == Mathf.Sign(transform4.yBasis.y)) angle *= -1;
            print(angle);
            transform4.matrix = transform4.matrix * UFunc.MatYZRot(angle);
        }
        */
    }
}
 