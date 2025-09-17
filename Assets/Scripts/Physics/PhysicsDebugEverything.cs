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

    [SerializeField] SphereColliderS sphere1;
    [SerializeField] bool meshSphereClose;

    [SerializeField] int meshEdgeNum = -1;

    [SerializeField] Vector4 arcPos1;
    [SerializeField] Vector4 arcPos2;
    [SerializeField] Vector4 arcPos3;
    [SerializeField] Vector4 arcPos4;
    [SerializeField] bool testArc;

    [SerializeField] bool flipPos;

    [SerializeField] RingColliderS ring1;
    [SerializeField] bool doRingSphereClose;
    [SerializeField] bool doRingCapClose;

    // Update is called once per frame
    void Update()
    {
        if (meshCapClose)
        {
            Vector4 linePoint = new Vector4();
            Vector4 meshPoint = new Vector4();
            mesh1.LineClose(cap1.point1,cap1.point2, ref linePoint, ref meshPoint);

            
            if (meshEdgeNum >= 0) {
                UFunc.DoubleArcCloseUnclamped(cap1.point1,cap1.point2, mesh1.verticiesWorld[mesh1.edges[1].x], mesh1.verticiesWorld[mesh1.edges[1].y], ref linePoint, ref meshPoint);
            }

            if (flipPos) {
                UFunc.DoubleArcCloseUnclamped(mesh1.verticiesWorld[mesh1.edges[1].x], mesh1.verticiesWorld[mesh1.edges[1].y],cap1.point1,cap1.point2, ref meshPoint, ref linePoint);
            }

            float distance = UFunc.DistanceS(meshPoint,linePoint);

            if (!flipPointClose)
            {
                transform4.MoveRotor(new Rotor(transform4.positionNorm,meshPoint));
            }
            else
            {
                transform4.MoveRotor(new Rotor(transform4.positionNorm,linePoint));
            }
        }   

        if (meshSphereClose)
        {
            Vector4 closeMesh = mesh1.PointClose(sphere1.center);

            transform4.MoveRotor(new Rotor(transform4.positionNorm,closeMesh));
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

            if (flipPos) 
            {
                close1 *= -1;
                close2 *= -1;
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

        if (testArc)
        {
            Vector4 close1 = new Vector4();
            Vector4 close2 = new Vector4();
            UFunc.DoubleArcCloseUnclamped(arcPos1, arcPos2, arcPos3, arcPos4, ref close1, ref close2);
        }

        if (doRingSphereClose)
        {
            Vector4 close1 = sphere1.transform4.position;
            Vector4 close2 = ring1.PointClose(close1);

            if (flipPos) {
                transform4.MoveRotor(new Rotor(transform4.positionNorm,close2));
            } else {
                transform4.MoveRotor(new Rotor(transform4.positionNorm,close1));
            }
        }
        if (doRingCapClose)
        {
            Vector4 close1 = new Vector4();
            Vector4 close2 = new Vector4();
            ring1.LineClose(cap1.point1,cap1.point2,ref close1, ref close2);

            if (flipPos) {
                transform4.MoveRotor(new Rotor(transform4.positionNorm,close2));
            } else {
                transform4.MoveRotor(new Rotor(transform4.positionNorm,close1));
            }
        }
    }
}
