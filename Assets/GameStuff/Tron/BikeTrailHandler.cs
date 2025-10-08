using UnityEngine;
using System.Collections.Generic;

public class BikeTrailHandler : MonoBehaviour
{
    public static BikeTrailHandler singleton;
    TronGameManager gameManager;

    public List<BikeGuy> bikes = new List<BikeGuy>();
    //public List<LineRendererS> lines = new List<LineRendererS>();

    [SerializeField] LineRendererS linePrefab;

    [SerializeField] float lineFrameInterval = 0.1f;
    float lineFrameTimer;

    [SerializeField] float defaultLineTime = 10;

    public List<List<Vector4>> collisionPoints = new List<List<Vector4>>();

    public List<BikeTrail> trailDatas = new List<BikeTrail>();

    float trailThick = 0.1f;

    public float worldRingCollisionRadius = 0.05f;

    [System.Serializable]
    public class BikeTrail
    {
        public BikeGuy bike;
        public LineRendererS line;
        public List<Vector4> collisionPoints;
        public List<Vector4> collisionTopPoints;
        public List<Vector4> collisionBottomPoints;
        public List<Vector4> collisionBinorms;
        public List<Vector4> collisionNorms;
        public float lineTime;
    }

    void OnEnable()
    {
        CheckSingleton();

        gameManager = TronGameManager.singleton;
    }

    void CheckSingleton()
    {
        if (singleton == null)
        {
            singleton = this;
        }
        else if (singleton != this)
        {
            Destroy(this);
        }
    }

    public void AddBike(BikeGuy bike)
    {
        if (!bikes.Contains(bike))
        {
            bikes.Add(bike);

            BikeTrail trailData = new BikeTrail();
            trailData.bike = bike;
            trailData.line = Instantiate(linePrefab);
            trailData.collisionPoints = new List<Vector4>();
            trailData.collisionTopPoints = new List<Vector4>();
            trailData.collisionBottomPoints = new List<Vector4>();
            trailData.collisionBinorms = new List<Vector4>();
            trailData.collisionNorms = new List<Vector4>();
            trailData.lineTime = defaultLineTime;

            trailData.line.color = gameManager.teamColors.GetColor(bikes.Count-1);

            trailDatas.Add(trailData);
        }
    }

    public void UpdateBike(int i)
    {
        trailDatas[i].lineTime = defaultLineTime + trailDatas[i].bike.lengthIncreaseGet;
    }

    void FixedUpdate()
    {
        if (!gameManager.roundStarted) return;

        CheckCollision();
        DrawLine();
    }

    void DrawLine()
    {
        lineFrameTimer -= Time.fixedDeltaTime;
        if (lineFrameTimer <= 0)
        {
            lineFrameTimer = lineFrameInterval;

            for (int i = 0; i < trailDatas.Count; i++)
            {
                BikeGuy bike = trailDatas[i].bike;
                LineRendererS line = trailDatas[i].line;

                Vector4 pos = bike.transform4.positionNorm;
                Vector4 binorm = bike.transform4.xBasis;

                trailDatas[i].collisionPoints.Add(pos);
                trailDatas[i].collisionTopPoints.Add(UFunc.Slerp4Angle(pos,binorm,trailThick));
                trailDatas[i].collisionBottomPoints.Add(UFunc.Slerp4Angle(pos,binorm,-trailThick));
                trailDatas[i].collisionBinorms.Add(binorm);
                trailDatas[i].collisionNorms.Add(bike.transform4.yBasis);

                if (trailDatas[i].collisionPoints.Count > 3)
                {
                    line.AddPos(trailDatas[i].collisionPoints[^3], binorm);
                }

                trailDatas[i].lineTime = defaultLineTime + bike.lengthIncreaseGet;

                if (line.positions.Count > trailDatas[i].lineTime / lineFrameInterval)
                {
                    line.RemovePos();
                    trailDatas[i].collisionPoints.RemoveAt(0);
                }
            }
        }
    }

    void CheckCollision()
    {
        for (int i = 0; i < gameManager.alivePlayers.Count; i++)
        {  
            int playerIndex = gameManager.alivePlayers[i];
            bool hit = CheckBike(bikes[playerIndex]);
            if (hit) gameManager.PlayerDie(playerIndex);
        }

        bool CheckBike(BikeGuy bike)
        {
            float boundingDot = Mathf.Cos(trailThick + bike.wide + bike.radius);

            bool collisionYes = false;
            for (int j = 0; j < trailDatas.Count; j++)
            {
                List<Vector4> points = trailDatas[j].collisionPoints;
                for (int i = 0; i < points.Count - 5; i++)
                {
                    float dot = Vector4.Dot(bike.transform4.position, points[i]);

                    if (dot > boundingDot)
                    {
                        bool yeah = CheckPlane(i, trailDatas[j]);
                        if (yeah) return true;
                    }
                }
            }

            for (int i = 0; i < 3; i++)
            {
                for (int j = i + 1; j < 4; j++)
                {
                    bool yeah = CheckRing(i, j);
                    if (yeah) return true;
                }
            }

            return false;

            bool CheckPlane(int index, BikeTrail trail)
            {
                Vector4 pos1 = trail.collisionPoints[index];
                Vector4 pos2 = trail.collisionPoints[index + 1];
                Vector4 binorm1 = trail.collisionBinorms[index];
                Vector4 binorm2 = trail.collisionBinorms[index + 1];

                Vector4 point1 = UFunc.Slerp4Angle(pos1, binorm1, trailThick);
                Vector4 point2 = UFunc.Slerp4Angle(pos1, binorm1, -trailThick);
                Vector4 point3 = UFunc.Slerp4Angle(pos2, binorm2, trailThick);
                Vector4 point4 = UFunc.Slerp4Angle(pos2, binorm2, -trailThick);

                //float overlap1 = TriColliderS.LineCloseTri(bike.widePoint1, bike.widePoint2, point1, point2, point3, ref bikeClose, ref triClose);
                //float overlap2 = TriColliderS.LineCloseTri(bike.widePoint1, bike.widePoint2, point3, point4, point2, ref bikeClose, ref triClose);

                Vector4 close1 = TriColliderS.PointCloseTri(bike.transform4.positionNorm, point1, point2, point3);
                Vector4 close2 = TriColliderS.PointCloseTri(bike.widePoint1, point1, point2, point3);
                Vector4 close3 = TriColliderS.PointCloseTri(bike.widePoint2, point1, point2, point3);

                float distance1 = UFunc.DistanceS(close1, bike.transform4.positionNorm);
                float distance2 = UFunc.DistanceS(close2, bike.widePoint1);
                float distance3 = UFunc.DistanceS(close3, bike.widePoint2);

                float minDis = Mathf.Min(distance1, distance2, distance3);

                if (minDis < bike.radius) return true;

                return false;
            }

            bool CheckRing(int axis1, int axis2)
            {
                float rad = worldRingCollisionRadius + bike.radius;

                Vector4 bike1 = bike.widePoint1;
                Vector4 bike2 = bike.widePoint2;

                Vector4 ring1 = new Vector4();
                Vector4 ring2 = new Vector4();
                ring1[axis1] = 1;
                ring2[axis2] = 1;

                Vector4 bikeClose = new Vector4();
                Vector4 ringClose = new Vector4();
                UFunc.DoubleArcCloseUnclamped(bike.widePoint1, bike.widePoint2, ring1, ring2, ref bikeClose, ref ringClose);

                bool between = UFunc.BetweenS(bike1,bike2,bikeClose);
                if (!between)
                {
                    bikeClose *= -1;
                    ringClose *= -1;
                    between = UFunc.BetweenS(bike1,bike2,bikeClose);
                }

                if (between)
                {
                    if (UFunc.DistanceS(bikeClose, ringClose) < bike.radius) return true;
                }

                Vector4 closeEnd1 = UFunc.SlerpPointCloseUnclamped(ring1, ring2, bike1);
                Vector4 closeEnd2 = UFunc.SlerpPointCloseUnclamped(ring1, ring2, bike1);

                if (UFunc.DistanceS(bike1, closeEnd1) < rad) return true;
                if (UFunc.DistanceS(bike2, closeEnd2) < rad) return true;

                return false;
            }
        }
    }

    public bool CheckCollisionPoint(Vector4 checkPoint, float radius)
    {
        float boundingDot = Mathf.Cos(trailThick + radius);

        bool collisionYes = false;
        for (int j = 0; j < trailDatas.Count; j++)
        {
            List<Vector4> points = trailDatas[j].collisionPoints;
            for (int i = 0; i < points.Count - 5; i++)
            {
                float dot = Vector4.Dot(checkPoint, points[i]);

                if (dot > boundingDot)
                {
                    bool yeah = CheckPlane(i, trailDatas[j]);
                    if (yeah) return true;
                }
            }
        }

        return false;

        bool CheckPlane(int index, BikeTrail trail)
        {
            Vector4 pos1 = trail.collisionPoints[index];
            Vector4 pos2 = trail.collisionPoints[index + 1];
            Vector4 binorm1 = trail.collisionBinorms[index];
            Vector4 binorm2 = trail.collisionBinorms[index + 1];

            Vector4 point1 = UFunc.Slerp4Angle(pos1, binorm1, trailThick);
            Vector4 point2 = UFunc.Slerp4Angle(pos1, binorm1, -trailThick);
            Vector4 point3 = UFunc.Slerp4Angle(pos2, binorm2, trailThick);
            Vector4 point4 = UFunc.Slerp4Angle(pos2, binorm2, -trailThick);

            Vector4 close1 = TriColliderS.PointCloseTri(checkPoint, point1, point2, point3);
            Vector4 close2 = TriColliderS.PointCloseTri(checkPoint, point3, point4, point2);


            if (UFunc.DistanceS(close1, checkPoint) < radius || UFunc.DistanceS(close1, checkPoint) < radius)
            {
                return true;
            }
            return false;
        }
    }

    public void Reset()
    {
        for (int i = 0; i < trailDatas.Count; i++)
        {
            Destroy(trailDatas[i].line.gameObject);
        }

        bikes.Clear();
        trailDatas.Clear();
        collisionPoints.Clear();
    }
}
