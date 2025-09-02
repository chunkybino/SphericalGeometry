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

    [SerializeField] MeshColliderS mesh1;
    [SerializeField] bool meshCapClose;

    // Update is called once per frame
    void Update()
    {
        if (meshCapClose)
        {
            Vector4 linePoint = new Vector4();
            Vector4 meshPoint = new Vector4();
            mesh1.LineClose(cap1.point1,cap1.point2, ref linePoint, ref meshPoint);

            transform4.MoveRotor(new Rotor(transform4.positionNorm,meshPoint));
        }   

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

            if (!flipPointClose)
            {
                transform4.MoveRotor(new Rotor(transform4.positionNorm,close1));
            }
            else
            {
                transform4.MoveRotor(new Rotor(transform4.positionNorm,close2));
            }
        }
    }
}
