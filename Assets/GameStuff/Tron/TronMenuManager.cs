using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.SceneManagement;

public class TronMenuManager : MonoBehaviour
{
    public static TronMenuManager singleton;
    public TronGameManager tronGameManager;


    public bool isMenu = true;

    public MainMenu mainMenuObject;

    bool sceneLoadedFlag;
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        sceneLoadedFlag = true;

        tronGameManager = FindObjectOfType<TronGameManager>();
        mainMenuObject = FindObjectOfType<MainMenu>();
    }

    public string mainMenuSceneName;
    public string snakeSceneName;


    void Awake()
    {
        InitialzeMenu();
    }

    void OnEnable()
    {
        CheckSingleton();

        tronGameManager = TronGameManager.singleton;

        Object.DontDestroyOnLoad(this);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void CheckSingleton()
    {
        if (singleton == null)
        {
            singleton = this;
        }
        else if (singleton != this)
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        if (isMenu)
        {
            //MenuFunc();
        }
        else
        {
            if (Input.GetKeyDown("escape"))
            {
                StartMenu();
            }
        }
    }



    public void InitialzeMenu()
    {
        //menuCamera.transform4.matrix = menuCamerStartMatrix;

        //menuCamera.gameObject.SetActive(true);

        if (!mainMenuObject) mainMenuObject = FindObjectOfType<MainMenu>();

        //mainMenu = mainMenuObject.mainMenu;
        //snakeMenu = mainMenuObject.snakeMenu;

        //foodNumButton = mainMenuObject.foodNumButton;
        //speedButton = mainMenuObject.speedButton;
        //startButton = mainMenuObject.startButton;

        mainMenuObject.SetActive(true);

        isMenu = true;

        //SetActiveMenu(mainMenu);

        //SetButtonSelectorTarget(selectedButton);
    }

    public void DisableMenu()
    {
        isMenu = false;

        mainMenuObject.SetActive(false);
        //currentMenu.SetActive(false);
    }

    public void KillTheGame()
    {
        Application.Quit();
    }

    void LoadScene(string name)
    {
        SceneManager.LoadScene(name);
    }

    public void StartMenu()
    {
        StartCoroutine(StartMenuCoroutine());
    }
    IEnumerator StartMenuCoroutine()
    {
        sceneLoadedFlag = false;
        LoadScene(mainMenuSceneName);
        yield return new WaitUntil(() => sceneLoadedFlag == true);
        sceneLoadedFlag = false;

        InitialzeMenu();
    }

    public void StartSnake(int foodNum = 1, float speed = 1)
    {
        DisableMenu();

        StartCoroutine(StartSnakeCoroutine(foodNum, speed));
    }
    IEnumerator StartSnakeCoroutine(int foodNum, float speed)
    {
        sceneLoadedFlag = false;

        LoadScene(snakeSceneName);

        yield return new WaitUntil(() => sceneLoadedFlag == true);

        sceneLoadedFlag = false;

        tronGameManager = TronGameManager.singleton;

        tronGameManager.foodNum = foodNum;
        tronGameManager.moveSpeed = speed;

        tronGameManager.InitializeGame();
    }
}
