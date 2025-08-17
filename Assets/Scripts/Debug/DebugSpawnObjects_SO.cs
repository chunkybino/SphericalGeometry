using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/DebugSpawnObject", fileName = "DebugSpawnObjects")]
public class DebugSpawnObjects_SO : ScriptableObject
{
    public Rigidbody4D[] rigidbodyObjects = new Rigidbody4D[0];

    public Rigidbody4D GetRigidbody(string name)
    {
        for (int i = 0; i < rigidbodyObjects.Length; i++)
        {
            if (rigidbodyObjects[i].gameObject.name == name) {
                return rigidbodyObjects[i];
            }
        }

        return null;
    }
}
