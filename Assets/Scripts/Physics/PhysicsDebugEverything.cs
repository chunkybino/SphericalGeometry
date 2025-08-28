using UnityEngine;

public class PhysiicsDebugEverything : MonoBehaviour
{
    [SerializeField] Transform4D transform4;

    [SerializeField] CapsuleColliderS cap1;
    [SerializeField] CapsuleColliderS cap2;
    [SerializeField] bool moveCapClose;

    [SerializeField] int pointIndex = 0;

    [SerializeField] bool doPointClose;
    [SerializeField] bool flipPointClose;

    // Update is called once per frame
    void Update()
    {
        /*
        Vector4 v1 = cap1.point1;
        Vector4 v2 = cap1.point2;
        Vector4 u1 = cap2.point1;
        Vector4 u2 = cap2.point2;

        Vector4 sphereNorm = UFunc.HyperCross(v1,v2,u1).normalized;

        Vector4 u3 = (u2 - Vector4.Dot(u2,sphereNorm)*sphereNorm).normalized; //u2 projected onto sphere of v1,v2,u1

        print(Vector4.Dot(u2,sphereNorm));
        print(u1 +" "+ u2 +" "+u3);

        //print(Vector4.Dot(sphereNorm,v1)+" "+Vector4.Dot(sphereNorm,v2)+" "+Vector4.Dot(sphereNorm,u1));

        Vector4 sphere1 = UFunc.HyperCross(v1,v2,sphereNorm).normalized;
        Vector4 sphere2 = UFunc.HyperCross(u1,u3,sphereNorm).normalized;

        Vector4 close1 = UFunc.HyperCross(sphere1,sphere2,sphereNorm).normalized;
        Vector4 close2 = UFunc.SlerpPointCloseUnclamped(u1,u2,close1);

        close2 = UFunc.ClampBetweenVectors(close2, u1,u2);
        close1 = UFunc.SlerpPointClose(v1,v2,close2);

        Vector4 pos = new Vector4();

        switch (pointIndex)
        {
            case 0:
                pos = u3;
                break;
            case 1:
                pos = close1;
                break;
            case 2:
                pos = close2;
                break;
            case 3:
                pos = sphereNorm;
                break;
            case 4:
                pos = sphere1;
                break;
            case 5:
                pos = sphere2;
                break;
        }

        if (doPointClose)
        {
            pos = UFunc.SlerpPointCloseUnclamped(v1,v2,u1);
            if (flipPointClose) {
                pos = UFunc.SlerpPointCloseUnclamped(v1,v2,u2);
            }
        }

        transform4.MoveRotor(new Rotor(transform4.positionNorm,pos));

        return;
        */

        if (doPointClose)
        {
            Vector4 pos = UFunc.SlerpPointCloseUnclamped(cap1.point1,cap1.point2,cap2.point1);
            if (flipPointClose) {
                pos = UFunc.SlerpPointCloseUnclamped(cap1.point1,cap1.point2,cap2.point2);
            }

            transform4.MoveRotor(new Rotor(transform4.positionNorm,pos));
        }

        if (moveCapClose)
        {
            Vector4 close1 = new Vector4();
            Vector4 close2 = new Vector4();

            UFunc.DoubleArcClose(cap1.point1,cap1.point2,cap2.point1,cap2.point2, ref close1, ref close2);

            Vector4 close1_2 = new Vector4();
            Vector4 close2_2 = new Vector4();
            UFunc.DoubleArcClose(cap2.point1,cap2.point2,cap1.point1,cap1.point2, ref close2_2, ref close1_2);

            if (Vector4.Dot(close1_2,close2_2) > Vector4.Dot(close1,close2))
            {
                close1 = close1_2;
                close2 = close2_2;
            }

            transform4.MoveRotor(new Rotor(transform4.positionNorm,close1));
        }
    }
}
