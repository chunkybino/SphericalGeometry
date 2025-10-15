using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class MenuButtonLayout
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
