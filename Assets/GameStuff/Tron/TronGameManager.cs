using UnityEngine;
using System.Collections.Generic;

[ExecuteAlways]
public class TronGameManager : MonoBehaviour
{
    public static TronGameManager singleton;

    public int playerCount = 1;

    public List<BikeGuy> playerBikes = new List<BikeGuy>();
    public BikeGuy[] bikePrefabs;


    public CameraFollow cameraFollow;


    public bool spawnPlayersNow;
    public bool startRound;

    public bool roundStarted;

    void OnEnable()
    {
        CheckSingleton();
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

    void OnValidate()
    {
        if (spawnPlayersNow)
        {
            spawnPlayersNow = false;
            SpawnPlayers();
        }

        if (startRound)
        {
            startRound = false;
            StartRound();
        }
    }


    public void SpawnPlayers()
    {
        for (int i = 0; i < playerCount; i++)
        {
            BikeGuy bikeType = bikePrefabs[Random.Range(0, bikePrefabs.Length - 1)];
            BikeGuy bike = Instantiate(bikeType);

            playerBikes.Add(bike);

            if (i == 0)
            {
                cameraFollow.followTransform = bike.camTransform;
            }

            BikeTrailHandler.singleton.AddBike(bike);
        }
    }

    public void StartRound()
    {
        for (int i = 0; i < playerBikes.Count; i++)
        {
            playerBikes[i].gameActive = true;
        }
        roundStarted = true;
    }

    public void PlayerDie(int index)
    {
        BikeGuy bike = playerBikes[index];

        bike.BlowUp();
    }
}
