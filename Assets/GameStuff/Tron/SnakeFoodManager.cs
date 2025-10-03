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

    void OnEnable()
    {
        CheckSingleton();

        gameManager = TronGameManager.singleton;
        //trailHandler = BikeTrailHandler.singleton;

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

        Vector4 newPos = UFunc.RandPosS();

        food.transform4.MoveTo(newPos);

        foods.Add(food);
    }

    void FixedUpdate()
    {
        gameManager = TronGameManager.singleton;
        bikes = gameManager.playerBikes;

        for (int i = 0; i < bikes.Count; i++)
        {
            for (int j = 0; j < foods.Count; j++)
            {
                float dis = UFunc.DistanceSRad(bikes[i].transform4.positionNorm, foods[j].positionNorm);

                print(dis);

                if (dis <= foods[j].radius)
                {
                    FoodEat(bikes[i], foods[j]);
                }
            }
        }
    }

    void FoodEat(BikeGuy bike, SnakeFood food)
    {
        Vector4 newPos = UFunc.RandPosS();

        food.transform4.MoveTo(newPos);

        bike.lengthIncreaseGet++;
    }
}
