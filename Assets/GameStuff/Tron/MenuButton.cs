using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class MenuButton : MonoBehaviour
{
    public UnityEvent onPress;
    public UnityEvent onLeft;
    public UnityEvent onRight;

    public bool doValues;

    public int valueIndex;
    //public int minValue = 0;
    //public int maxValue = 2;

    public float[] values;

    public string[] displayStrings;
    public TextMeshProUGUI text;

    public float ReadFloat() { return values[valueIndex]; }
    public int ReadInt() { return Mathf.RoundToInt(values[valueIndex]); }

    public bool incrementOnLeftRight;

    public RectTransform buttonRect;

    public void Press()
    {
        onPress?.Invoke();
    }
    public void DoLeft()
    {
        onLeft?.Invoke();
        if (incrementOnLeftRight) DecreaseValue();
    }
    public void DoRight()
    {
        onRight?.Invoke();
        if (incrementOnLeftRight) IncreaseValue();
    }

    public void IncreaseValue()
    {
        valueIndex++;
        if (valueIndex >= values.Length) valueIndex = 0;
    }
    public void DecreaseValue()
    {
        valueIndex--;
        if (valueIndex < 0) valueIndex = values.Length - 1;
    }

    void Update()
    {
        if (!doValues || !text) return;

        if (valueIndex >= displayStrings.Length) return;

        text.text = displayStrings[valueIndex];
    }
}
