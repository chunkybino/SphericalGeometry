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

    [System.Serializable]
    public class BikeTrail
    {
        public BikeGuy bike;
        public LineRendererS line;
        public List<Vector4> collisionPoints;
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

                line.AddPos(pos, binorm);
                trailDatas[i].collisionPoints.Add(pos);
                trailDatas[i].collisionBinorms.Add(binorm);
                trailDatas[i].collisionNorms.Add(bike.transform4.yBasis);

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
            float boundingDot = Mathf.Cos(trailThick + bike.wide);

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

            return false;

            bool CheckPlane(int index, BikeTrail trail)
            {
                Vector4 pos1 = trail.collisionPoints[index];
                Vector4 pos2 = trail.collisionPoints[index+1];
                Vector4 binorm1 = trail.collisionBinorms[index];
                Vector4 binorm2 = trail.collisionBinorms[index+1];

                Vector4 point1 = UFunc.Slerp4Angle(pos1,binorm1,trailThick);
                Vector4 point2 = UFunc.Slerp4Angle(pos1,binorm1,-trailThick);
                Vector4 point3 = UFunc.Slerp4Angle(pos2,binorm2,trailThick);
                Vector4 point4 = UFunc.Slerp4Angle(pos2,binorm2,-trailThick);

                Vector4 tangent = UFunc.ProjectToVectorNormal(pos2-pos1, pos1).normalized;

                Vector4 norm = trail.collisionNorms[index];
                Vector4 bikeNorm = bike.transform4.yBasis;

                Vector4 bikePoint1 = bike.widePoint1;
                Vector4 bikePoint2 = bike.widePoint2;

                //norm1
                float bikeDot1 = Vector4.Dot(bikePoint1,norm);
                float bikeDot2 = Vector4.Dot(bikePoint2,norm);
                print(bikeDot1+" "+bikeDot2);
                if (Mathf.Sign(bikeDot1) == Mathf.Sign(bikeDot2))
                {
                    return false;
                }

                bool sameSign = true;
                float dotSign = Mathf.Sign(Vector4.Dot(point1,bikeNorm));
                if (dotSign != Mathf.Sign(Vector4.Dot(point2,bikeNorm))) sameSign = false;
                if (dotSign != Mathf.Sign(Vector4.Dot(point3,bikeNorm))) sameSign = false;
                if (dotSign != Mathf.Sign(Vector4.Dot(point4,bikeNorm))) sameSign = false;

                if (!sameSign)
                {
                    print(Vector4.Dot(point1,bikeNorm)+" "+Vector4.Dot(point2,bikeNorm)+" "+Vector4.Dot(point3,bikeNorm)+" "+Vector4.Dot(point4,bikeNorm));
                }

                return !sameSign;
            }
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
