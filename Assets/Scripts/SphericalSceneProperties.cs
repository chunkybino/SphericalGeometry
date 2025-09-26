using UnityEngine;
using UnityEngine.Events;

[ExecuteAlways]
public class SphericalSceneProperties : MonoBehaviour
{
    public static SphericalSceneProperties singleton;

    public float worldRadius = 1;

    public static UnityEvent onRadiusChange = new UnityEvent();

    void Awake()
    {
        CheckSingleton();
    }

    void OnEnable()
    {
        CheckSingleton();
    }

    void CheckSingleton()
    {
        if (!singleton)
        {
            singleton = this;
        }
        else if (singleton != this)
        {
            Destroy(this);
        }
    }
}
