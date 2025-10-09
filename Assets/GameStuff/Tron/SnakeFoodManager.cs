using UnityEngine;
using System.Collections.Generic;

public class SnakeFoodManager : MonoBehaviour
{
    public static SnakeFoodManager singleton;

    TronGameManager gameManager;
    //BikeTrailHandler trailHandler;

    public List<BikeGuy> bikes;

    public List<SnakeFood> foods = new List<SnakeFood>();
    public SnakeFood foodPrefab;

    public int foodSpawnNum = 2;

    public float foodSpawnBoundRadius = 1f; //when spawn food, make sures its not too close to any trails, multiplier on food radius

    void OnEnable()
    {
        CheckSingleton();

        gameManager = TronGameManager.singleton;
    }

    public void Reset()
    {
        for (int i = 0; i < foods.Count; i++)
        {
            Destroy(foods[i].gameObject);
        }
        foods.Clear();
    }

    public void RoundStart()
    {
        Reset();

        for (int i = 0; i < foodSpawnNum; i++)
        {
            SpawnFood();
        }
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

    void SpawnFood()
    {
        SnakeFood food = Instantiate(foodPrefab);

        MoveFood(food);

        foods.Add(food);
    }

    void FixedUpdate()
    {
        gameManager = TronGameManager.singleton;
        bikes = gameManager.playerBikes;

        for (int i = 0; i < gameManager.alivePlayers.Count; i++)
        {
            BikeGuy bike = bikes[gameManager.alivePlayers[i]];

            for (int j = 0; j < foods.Count; j++)
            {
                float dis = UFunc.DistanceSRad(bike.transform4.positionNorm, foods[j].positionNorm);

                if (dis <= foods[j].radius)
                {
                    FoodEat(bike, foods[j]);
                }
            }
        }
    }

    void FoodEat(BikeGuy bike, SnakeFood food)
    {
        Vector4 newPos = UFunc.RandPosS();

        MoveFood(food);

        bike.EatFood();
    }

    void MoveFood(SnakeFood food)
    {
        Vector4 newPos = UFunc.RandPosS();
        for (int i = 0; i < 10; i++)
        {
            bool check = BikeTrailHandler.singleton.CheckCollisionPoint(newPos, food.radius * foodSpawnBoundRadius);
            if (!check) break;
            newPos = UFunc.RandPosS();
        }
        food.transform4.MoveTo(newPos);
    }
}
