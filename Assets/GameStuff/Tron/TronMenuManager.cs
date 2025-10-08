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

            SetButtonSelectorTarget(selectedButton);
        }
        if (Input.GetKeyDown("s"))
        {
            buttonSelected--;
            if (buttonSelected < 0) buttonSelected = menuButtons.Count - 1;

            SetButtonSelectorTarget(selectedButton);
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

        //the button selector

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
            }

            buttonSelection.anchoredPosition = (slerpCorners[0]+slerpCorners[1]+slerpCorners[2]+slerpCorners[3])/4;
            buttonSelection.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, buttonSelectorMargin + slerpCorners[2].x - slerpCorners[0].x);
            buttonSelection.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, buttonSelectorMargin + slerpCorners[2].y - slerpCorners[0].y);
            //buttonSelection.anchorMin = slerpCorners[0];
            //buttonSelection.anchorMax = slerpCorners[2];

            UFunc.TickTimer(ref buttonSelectorMoveTimer);
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

        SetButtonSelectorTarget(selectedButton);
    }

    public void StartSnake()
    {
        DisableMenu();

        menuCamera.gameObject.SetActive(false);

        tronGameManager = TronGameManager.singleton;

        tronGameManager.foodNum = foodNumButton.ReadInt();
        tronGameManager.moveSpeed = speedButton.ReadFloat();
        tronGameManager.playerCount = playerNumButton.ReadInt();

        tronGameManager.InitializeGame();
    }

    public void DisableMenu()
    {
        isMenu = false;
        menuObject.SetActive(false);
    }

    void SetButtonSelectorTarget(MenuButton button)
    {
        buttonSelectorFromRect = buttonSelectorTargetRect;
        buttonSelectorTargetRect = button.buttonRect;
        if (buttonSelectorFromRect == null) buttonSelectorFromRect = buttonSelectorTargetRect;

        buttonSelectorMoveTimer = buttonSelectorMoveTime;
    }
}
