using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ExecuteAlways]
public class TronGameManager : MonoBehaviour
{
    public static TronGameManager singleton;

    public int playerCount = 1;
    public int foodNum = 1;
    public float moveSpeed = 1;

    public List<PlayerProfile> players = new List<PlayerProfile>();

    public List<BikeGuy> playerBikes = new List<BikeGuy>();
    public BikeGuy[] bikePrefabs;

    public CameraFollow cameraFollow;
    public List<CameraFollow> cameras;

    public TeamColor_SO teamColors;

    public bool spawnPlayersNow;
    public bool startRound;

    public bool gameStarted;
    public bool roundStarted;

    public List<int> alivePlayers;
    int winPlayer;

    [System.Serializable]
    public class PlayerProfile
    {
        public BikeGuy bike;
        public CameraFollow camera;

        public int bikeType;

        public int score;
    }

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

    /*
    void Update()
    {
        if (!gameStarted)
        {
            if (Input.GetKeyDown("space"))
            {
                StartCoroutine("StartGameCoroutine");
            }
        }
    }
    */

    public void InitializeGame()
    {
        SnakeFoodManager.singleton.foodSpawnNum = foodNum;

        StartCoroutine("StartGameCoroutine");
    }

    IEnumerator StartGameCoroutine()
    {
        gameStarted = true;

        BikeTrailHandler.singleton.Reset();
        SnakeFoodManager.singleton.Reset();

        SpawnPlayers();

        yield return new WaitForSeconds(2);

        StartRound();
    }

    public void CreatePlayerProfiles()
    {
        players = new List<PlayerProfile>();
        for (int i = 0; i < playerCount; i++) { players.Add(new PlayerProfile()); }
    }

    public void SpawnPlayers()
    {
        if (players == null || players.Count != playerCount) CreatePlayerProfiles();

        for (int i = 0; i < playerBikes.Count; i++) { Destroy(playerBikes[i].gameObject); }

        alivePlayers = new List<int>();
        playerBikes.Clear();

        for (int i = 0; i < players.Count; i++)
        {
            PlayerProfile player = players[i];

            BikeGuy bikeType = bikePrefabs[player.bikeType];
            BikeGuy bike = Instantiate(bikeType);
            bike.bikeIndex = i;
            bike.transform4.MoveTo(UFunc.RandPosS());

            bike.controlIndex = i;

            player.bike = bike;

            playerBikes.Add(bike);

            if (i >= cameras.Count)
            {
                if (i == 0)
                {
                    cameraFollow.followTransform = bike.camTransform;
                    cameras.Add(cameraFollow);
                    player.camera = cameraFollow;
                }
                else
                {
                    CameraFollow newCam = Instantiate(cameraFollow);
                    newCam.followTransform = bike.camTransform;
                    cameras.Add(newCam);
                    player.camera = newCam;
                }
            }
            else
            {
                cameras[i].followTransform = bike.camTransform;
                player.camera = cameras[i];
            }

            player.camera.gameObject.SetActive(true);

            BikeTrailHandler.singleton.AddBike(bike);

            alivePlayers.Add(i);
        }

        if (cameras.Count > 1)
        {
            cameras[0].camObj.rect = new Rect(0.0f, 0.5f, 1.0f, 1.0f);
            cameras[1].camObj.rect = new Rect(0.0f, 0.0f, 1.0f, 0.5f);
        }
    }

    public void StartRound()
    {
        for (int i = 0; i < playerBikes.Count; i++)
        {
            playerBikes[i].gameActive = true;
        }
        roundStarted = true;

        SnakeFoodManager.singleton.RoundStart();
    }

    public void PlayerDie(int index)
    {
        BikeGuy bike = playerBikes[index];

        bike.BlowUp();

        if (alivePlayers.Count <= 2)
        {
            winPlayer = index;
            StartCoroutine("RoundEndCoroutine");
        }

        alivePlayers.Remove(index);
    }

    IEnumerator RoundEndCoroutine()
    {
        players[winPlayer].score++;

        yield return new WaitForSeconds(2);

        roundStarted = false;
        StartCoroutine("StartGameCoroutine");
    }
}
