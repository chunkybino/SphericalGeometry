using UnityEngine;

[CreateAssetMenu(fileName = "TeamColor", menuName = "ScriptableObject/TeamColor")]
public class TeamColor_SO : ScriptableObject
{
    public Color[] colors;

    public Color GetColor(int i)
    {
        i = i % colors.Length;
        return colors[i];
    }
}
