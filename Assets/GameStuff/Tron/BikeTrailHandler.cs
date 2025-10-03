using UnityEngine;
using System.Collections.Generic;

public class BikeTrailHandler : MonoBehaviour
{
    public static BikeTrailHandler singleton;
    TronGameManager gameManager;

    public List<BikeGuy> bikes = new List<BikeGuy>();
    public List<LineRendererS> lines = new List<LineRendererS>();

    [SerializeField] LineRendererS linePrefab;

    [SerializeField] float lineFrameInterval = 0.1f;
    float lineFrameTimer;

    [SerializeField] float lineTime = 10;

    public List<List<Vector4>> collisionPoints = new List<List<Vector4>>();

    void OnEnable()
    {
        CheckSingleton();
        
        /*
        BikeGuy[] bikesFound = FindObjectsOfType<BikeGuy>();
        for (int i = 0; i < bikesFound.Length; i++)
        {
            AddBike(bikesFound[i]);
        }
        */

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
            lines.Add(Instantiate(linePrefab));
            collisionPoints.Add(new List<Vector4>());
        }
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

            for (int i = 0; i < bikes.Count; i++)
            {
                BikeGuy bike = bikes[i];
                LineRendererS line = lines[i];

                Vector4 pos = bike.transform4.positionNorm;
                Vector4 norm = bike.transform4.yBasis;

                line.AddPos(pos, norm);
                collisionPoints[i].Add(pos);

                if (line.positions.Count > lineTime / lineFrameInterval)
                {
                    line.RemovePos();
                    collisionPoints[i].RemoveAt(0);
                }
            }
        }
    }

    void CheckCollision()
    {
        float boundingDot = Mathf.Cos(0.1f);

        for (int i = 0; i < bikes.Count; i++)
        {
            bool hit = CheckBike(bikes[i]);
            if (hit) gameManager.PlayerDie(i);
        }

        bool CheckBike(BikeGuy bike)
        {
            bool collisionYes = false;
            for (int j = 0; j < collisionPoints.Count; j++)
            {
                List<Vector4> points = collisionPoints[j];
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
}
