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


    public GameObject fullMenuObject;

    MenuButton selectedButton { get { return currentMenu.buttonSelected; } }

    ButtonLayout currentMenu;

    public ButtonLayout mainMenu;
    public ButtonLayout snakeMenu;


    [System.Serializable]
    public class ButtonLayout
    {
        public int defaultButton = 0;
        public int selectedIndex = 0;
        public List<MenuButton> menuButtons = new List<MenuButton>();
        public GameObject buttonObject;

        public MenuButton buttonSelected { get { return menuButtons[selectedIndex]; } }

        public void SetActive(bool yes)
        {
            buttonObject.SetActive(yes);
            selectedIndex = defaultButton;
        }

        public void IncrementSelected(int amount)
        {
            selectedIndex += amount;
            if (selectedIndex > menuButtons.Count - 1) selectedIndex -= menuButtons.Count;
            if (selectedIndex < 0) selectedIndex += menuButtons.Count;
        }

        public void DoLeft()
        {
            buttonSelected.DoLeft();
        }
        public void DoRight()
        {
            buttonSelected.DoRight();
        }
        public void DoPress()
        {
            buttonSelected.Press();
        }
    }

    public MenuButton playerNumButton;
    public MenuButton foodNumButton;
    public MenuButton speedButton;
    public MenuButton startButton;

    public RectTransform buttonSelection;
    public float buttonSelectorMoveTime = 1;
    float buttonSelectorMoveTimer = 1;
    public AnimationCurve buttonSelectorMoveCurve;
    public RectTransform buttonSelectorFromRect;
    public RectTransform buttonSelectorTargetRect;
    public float buttonSelectorMargin = 15;

    void Awake()
    {
        InitialzeMenu();
    }

    void OnEnable()
    {
        CheckSingleton();

        //startButton.onPress.AddListener(StartSnake);

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
            currentMenu.IncrementSelected(1);
            SetButtonSelectorTarget(selectedButton);
        }
        if (Input.GetKeyDown("s"))
        {
            currentMenu.IncrementSelected(-1);

            SetButtonSelectorTarget(selectedButton);
        }

        if (Input.GetKeyDown("space"))
        {
            currentMenu.DoPress();
        }
        if (Input.GetKeyDown("a"))
        {
            currentMenu.DoLeft();
        }
        if (Input.GetKeyDown("d"))
        {
            currentMenu.DoRight();
        }

        ButtonSelectorMovement();
    }

    void ButtonSelectorMovement()
    {
        if (buttonSelectorFromRect != null && buttonSelectorTargetRect != null)
        {
            float progress = buttonSelectorMoveCurve.Evaluate(1 - buttonSelectorMoveTimer / buttonSelectorMoveTime);

            Vector3[] fourCornersFrom = new Vector3[4];
            Vector3[] fourCornersTarget = new Vector3[4];
            buttonSelectorFromRect.GetWorldCorners(fourCornersFrom);
            buttonSelectorTargetRect.GetWorldCorners(fourCornersTarget);

            Vector2[] slerpCorners = new Vector2[4];
            for (int i = 0; i < 4; i++)
            {
                slerpCorners[i] = Vector2.Lerp((Vector2)fourCornersFrom[i], (Vector2)fourCornersTarget[i], progress);
                slerpCorners[i] *= 600f / Screen.height;
            }

            buttonSelection.anchoredPosition = (slerpCorners[0] + slerpCorners[1] + slerpCorners[2] + slerpCorners[3]) / 4;

            buttonSelection.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, buttonSelectorMargin + slerpCorners[2].x - slerpCorners[0].x);
            buttonSelection.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, buttonSelectorMargin + slerpCorners[2].y - slerpCorners[0].y);

            UFunc.TickTimer(ref buttonSelectorMoveTimer);
        }
    }


    public void InitialzeMenu()
    {
        menuCamera.transform4.matrix = menuCamerStartMatrix;

        menuCamera.gameObject.SetActive(true);

        fullMenuObject.SetActive(true);
        isMenu = true;

        SetActiveMenu(mainMenu);

        SetButtonSelectorTarget(selectedButton);

        FindObjectOfType<TronUI>().SetUIActive(false);
    }

    public void StartSnake()
    {
        DisableMenu();

        menuCamera.gameObject.SetActive(false);

        tronGameManager = TronGameManager.singleton;

        tronGameManager.foodNum = foodNumButton.ReadInt();
        tronGameManager.moveSpeed = speedButton.ReadFloat();
        //tronGameManager.playerCount = playerNumButton.ReadInt();

        tronGameManager.InitializeGame();

        FindObjectOfType<TronUI>().SetUIActive(true);
    }

    public void DisableMenu()
    {
        isMenu = false;

        fullMenuObject.SetActive(false);
        currentMenu.SetActive(false);
    }



    void SetButtonSelectorTarget(MenuButton button)
    {
        buttonSelectorFromRect = buttonSelectorTargetRect;
        buttonSelectorTargetRect = button.buttonRect;
        if (buttonSelectorFromRect == null) buttonSelectorFromRect = buttonSelectorTargetRect;

        buttonSelectorMoveTimer = buttonSelectorMoveTime;
    }

    public void SetActiveMenu(ButtonLayout men)
    {
        if (currentMenu != null) currentMenu.SetActive(false);
        currentMenu = men;
        currentMenu.SetActive(true);

        SetButtonSelectorTarget(selectedButton);
    }
    public void SetActiveMenu(string menName)
    {
        ButtonLayout men = null;

        switch (menName)
        {
            case "main":
                men = mainMenu;
                break;
            case "snake":
                men = snakeMenu;
                break;
        }

        SetActiveMenu(men);
    }

    public void KillTheGame()
    {
        Application.Quit();
    }
}
