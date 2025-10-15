using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public Camera4D menuCamera;

    public Matrix4x4 menuCamerStartMatrix;
    public float menuCameraSpeed = 0.2f;


    MenuButton selectedButton { get { return currentMenu.buttonSelected; } }
    MenuButtonLayout currentMenu;

    public MenuButtonLayout mainMenu;
    public MenuButtonLayout snakeMenu;

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

    void Update()
    {
        menuCamera.transform4.MoveTo(UFunc.Slerp4Angle(menuCamera.transform4.positionNorm, -menuCamera.transform4.zBasis, menuCameraSpeed * Time.deltaTime));

        MenuFunc();
    }

    void MenuFunc()
    {
        //menuCamera.transform4.MoveTo(UFunc.Slerp4Angle(menuCamera.transform4.positionNorm, -menuCamera.transform4.zBasis, menuCameraSpeed * Time.deltaTime));

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
                slerpCorners[i] = Vector2.LerpUnclamped((Vector2)fourCornersFrom[i], (Vector2)fourCornersTarget[i], progress);
                slerpCorners[i] *= 600f / Screen.height;
            }

            buttonSelection.anchoredPosition = (slerpCorners[0] + slerpCorners[1] + slerpCorners[2] + slerpCorners[3]) / 4;

            buttonSelection.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, buttonSelectorMargin + slerpCorners[2].x - slerpCorners[0].x);
            buttonSelection.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, buttonSelectorMargin + slerpCorners[2].y - slerpCorners[0].y);

            UFunc.TickTimer(ref buttonSelectorMoveTimer);
        }
    }

    public void SetActive(bool yes)
    {
        if (yes)
        {
            Initialize();
        }
        else
        {
            gameObject.SetActive(false);
            menuCamera.gameObject.SetActive(false);
        }
    }

    void Initialize()
    {
        gameObject.SetActive(true);
        menuCamera.gameObject.SetActive(true);

        menuCamera.transform4.matrix = menuCamerStartMatrix;

        SetActiveMenu(mainMenu);

        SetButtonSelectorTarget(selectedButton);
    }

    void SetButtonSelectorTarget(MenuButton button)
    {
        buttonSelectorFromRect = buttonSelectorTargetRect;
        buttonSelectorTargetRect = button.buttonRect;
        if (buttonSelectorFromRect == null) buttonSelectorFromRect = buttonSelectorTargetRect;

        buttonSelectorMoveTimer = buttonSelectorMoveTime;
    }

    public void SetActiveMenu(MenuButtonLayout men)
    {
        if (currentMenu != null) currentMenu.SetActive(false);
        currentMenu = men;
        currentMenu.SetActive(true);

        SetButtonSelectorTarget(selectedButton);
    }
    public void SetActiveMenu(string menName)
    {
        MenuButtonLayout men = null;

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

    public void StartSnake()
    {
        TronMenuManager.singleton.StartSnake(
            foodNumButton.ReadInt(),
            speedButton.ReadFloat()
        );
    }
    public void ExitGame()
    {
        TronMenuManager.singleton.KillTheGame();
    }
}
