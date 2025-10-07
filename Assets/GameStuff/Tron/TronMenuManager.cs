using UnityEngine;
using System.Collections.Generic;

public class TronMenuManager : MonoBehaviour
{
    public TronMenuManager singleton;
    public TronGameManager tronGameManager;

    public Camera4D menuCamera;

    public bool isMenu = true;

    public Matrix4x4 menuCamerStartMatrix;
    public float menuCameraSpeed = 0.2f;


    public GameObject menuObject;

    public int buttonSelected = 0;
    public List<MenuButton> menuButtons = new List<MenuButton>();
    MenuButton selectedButton { get { return menuButtons[buttonSelected]; } }

    public MenuButton foodNumButton;
    public MenuButton speedButton;
    public MenuButton startButton;

    void Awake()
    {
        InitialzeMenu();
    }

    void OnEnable()
    {
        CheckSingleton();

        startButton.onPress.AddListener(StartSnake);

        tronGameManager = TronGameManager.singleton;
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

    void Update()
    {
        if (isMenu)
        {
            MenuFunc();
        }
        else
        {
            if (Input.GetKeyDown("escape"))
            {
                InitialzeMenu();
            }
        }
    }

    void MenuFunc()
    {
        menuCamera.transform4.MoveTo(UFunc.Slerp4Angle(menuCamera.transform4.positionNorm, -menuCamera.transform4.zBasis, menuCameraSpeed * Time.deltaTime));

        if (Input.GetKeyDown("w"))
        {
            buttonSelected++;
            if (buttonSelected >= menuButtons.Count) buttonSelected = 0;
        }
        if (Input.GetKeyDown("s"))
        {
            buttonSelected--;
            if (buttonSelected < 0) buttonSelected = menuButtons.Count - 1;
        }

        if (Input.GetKeyDown("space"))
        {
            selectedButton.Press();
        }
        if (Input.GetKeyDown("a"))
        {
            selectedButton.DoLeft();
        }
        if (Input.GetKeyDown("d"))
        {
            selectedButton.DoRight();
        }
    }


    public void InitialzeMenu()
    {
        menuCamera.transform4.matrix = menuCamerStartMatrix; //Matrix4x4.identity;
        //menuCamera.transform4.MoveTo(new Vector4(0.707f, 0f, 0.5f, 0.5f).normalized);
        //Rotor orientRotor = new Rotor(menuCamera.transform4.zBasis, UFunc.Slerp4Angle(menuCamera.transform4.positionNorm, new Vector4(0, 0.707f, -0.5f, 0.5f), Mathf.PI/2));
        //menuCamera.transform4.MoveRotor(orientRotor);

        menuCamera.gameObject.SetActive(true);

        menuObject.SetActive(true);
        isMenu = true;
    }

    public void StartSnake()
    {
        DisableMenu();

        menuCamera.gameObject.SetActive(false);

        tronGameManager = TronGameManager.singleton;

        tronGameManager.foodNum = Mathf.RoundToInt(foodNumButton.ReadValue());
        tronGameManager.moveSpeed = speedButton.ReadValue();

        tronGameManager.InitializeGame();
    }

    public void DisableMenu()
    {
        isMenu = false;
        menuObject.SetActive(false);
    }
}
