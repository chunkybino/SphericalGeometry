using UnityEngine;
using System.Collections.Generic;

public class PhysicsHandlerS : MonoBehaviour
{
    public static PhysicsHandlerS singleton;

    public List<Rigidbody4D> rigidbodyList = new List<Rigidbody4D>();
    public List<Rigidbody4D> staticBodies = new List<Rigidbody4D>();
    public List<Rigidbody4D> dynamicBodies = new List<Rigidbody4D>();

    void Awake()
    {
        if (!singleton) {
            singleton = this;
        } else if (singleton != this) {
            Destroy(this);
        }
    }

    public void AddRigidbody(Rigidbody4D rb)
    {
        if (!rigidbodyList.Contains(rb)) rigidbodyList.Add(rb);

        if (rb.isStatic) {
            if (!staticBodies.Contains(rb)) staticBodies.Add(rb);
        } else {
            if (!dynamicBodies.Contains(rb)) dynamicBodies.Add(rb);
        }
    }

    public void UpdateRigidbodyStatic(Rigidbody4D rb)
    {
        if (rb.isStatic) {
            dynamicBodies.Remove(rb);
            staticBodies.Add(rb);
        } else {
            dynamicBodies.Add(rb);
            staticBodies.Remove(rb);
        }
    }

    void FixedUpdate()
    {
        //normal physics update
        foreach (Rigidbody4D rb in rigidbodyList)
        {
            rb.PhysicsUpdate();
        }

        //collision

        //do all dynamic against static collisions first
        foreach (Rigidbody4D rb in dynamicBodies)
        {
            if (rb.collider == null) continue;
            foreach (Rigidbody4D staticRb in staticBodies)
            {
                if (staticRb.collider == null) continue;
                Vector4 prev1 = rb.transform4.positionNorm;
                Vector4 prev2 = staticRb.transform4.positionNorm;
                ColliderS.CollisionPhysic(rb.collider, staticRb.collider);
            }
        }

        //do dynamic against dynamic collisions
        for (int i = 0; i < dynamicBodies.Count; i++)
        {
            Rigidbody4D rb = dynamicBodies[i];
            if (rb.collider == null) continue;
            for (int j = i+1; j < dynamicBodies.Count; j++)
            {
                Rigidbody4D rb2 = dynamicBodies[j];
                if (rb2.collider == null) continue;
                ColliderS.CollisionPhysic(rb.collider, rb2.collider);
            }
        }
    }
}
