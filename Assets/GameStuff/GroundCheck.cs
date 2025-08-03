using UnityEngine;
using System.Collections.Generic;

public class GroundCheck : MonoBehaviour
{    
    public bool grounded;

    public ColliderS collider;

    public List<ColliderS> ignoreColliders;

    void FixedUpdate()
    {
        grounded = false;
        foreach (ColliderS c in collider.overlapColliders)
        {
            if (c.isTrigger) continue;
            if (ignoreColliders.Contains(c)) continue;
            grounded = true;
        }
    }
}
