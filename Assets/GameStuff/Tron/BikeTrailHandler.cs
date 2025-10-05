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

    [System.Serializable]
    public class BikeTrail
    {
        public BikeGuy bike;
        public LineRendererS line;
        public List<Vector4> collisionPoints;
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
                Vector4 norm = bike.transform4.yBasis;

                line.AddPos(pos, norm);
                trailDatas[i].collisionPoints.Add(pos);

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
        float boundingDot = Mathf.Cos(0.1f);

        for (int i = 0; i < gameManager.alivePlayers.Count; i++)
        {  
            int playerIndex = gameManager.alivePlayers[i];
            bool hit = CheckBike(bikes[playerIndex]);
            if (hit) gameManager.PlayerDie(playerIndex);
        }

        bool CheckBike(BikeGuy bike)
        {
            bool collisionYes = false;
            for (int j = 0; j < trailDatas.Count; j++)
            {
                List<Vector4> points = trailDatas[j].collisionPoints;
                for (int i = 0; i < points.Count - 5; i++)
                {
                    float dot = Vector4.Dot(bike.transform4.position, points[i]);

                    if (dot > boundingDot)
                    {
                        return true;
                    }
                }
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
