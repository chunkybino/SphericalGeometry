using UnityEngine;

public class SnakeFood : MonoBehaviour
{
    public Transform4D transform4;
    public Vector4 positionNorm { get { return transform4.positionNorm; } }
    public float size = 1;

    public float radius { get { return size * transform4.scale; } }

    void OnEnable()
    {
        transform4 = GetComponent<Transform4D>();
    }
}
