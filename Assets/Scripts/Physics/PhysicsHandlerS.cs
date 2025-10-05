using UnityEngine;
using System.Collections.Generic;

public class PhysicsHandlerS : MonoBehaviour
{
    public static PhysicsHandlerS singleton;

    public List<Rigidbody4D> rigidbodyList = new List<Rigidbody4D>();
    public List<Rigidbody4D> staticBodies = new List<Rigidbody4D>();
    public List<Rigidbody4D> dynamicBodies = new List<Rigidbody4D>();

    public List<Rigidbody4D> globalBodies = new List<Rigidbody4D>();

    [SerializeField] Sector[] sectors = new Sector[8];

    [SerializeField] int totalCollisionChecks;

    [SerializeField] bool clearNow;

    [System.Serializable]
    class Sector
    {
        public List<Rigidbody4D> staticBodies = new List<Rigidbody4D>();
        public List<Rigidbody4D> dynamicBodies = new List<Rigidbody4D>();

        public void AddRemove(Rigidbody4D rb, bool remove)
        {
            if (!remove)
            {
                if (rb.isStatic) {
                    if (!staticBodies.Contains(rb)) staticBodies.Add(rb);
                } else {
                    if (!dynamicBodies.Contains(rb)) dynamicBodies.Add(rb);
                }
            }
            else
            {
                if (rb.isStatic) {
                    staticBodies.Remove(rb);
                } else {
                    dynamicBodies.Remove(rb);
                }
            }
        }

        public void Clear()
        {
            staticBodies.Clear();
            dynamicBodies.Clear();
        }
    }

    void Awake()
    {
        CheckSingleton();

        rigidbodyList.Clear();
        staticBodies.Clear();
        dynamicBodies.Clear();
        globalBodies.Clear();

        if (sectors.Length != 8) sectors = new Sector[8];
        foreach (Sector s in sectors) {
            s.Clear();
        }
    }

    void OnEnable()
    {
        CheckSingleton();
    }

    void CheckSingleton()
    {
        if (!singleton) {
            singleton = this;
        } else if (singleton != this) {
            Destroy(this);
        }
    }

    void OnValidate()
    {
        if (clearNow) {
            clearNow = false;

            rigidbodyList.Clear();
            staticBodies.Clear();
            dynamicBodies.Clear();
            globalBodies.Clear();
            foreach (Sector s in sectors) {
                s.Clear();
            }
        }
    }

    public void AddRigidbody(Rigidbody4D rb)
    {
        if (rigidbodyList == null) return;

        if (!rigidbodyList.Contains(rb)) rigidbodyList.Add(rb);

        if (rb.isStatic) {
            if (!staticBodies.Contains(rb)) staticBodies.Add(rb);
        } else {
            if (!dynamicBodies.Contains(rb)) dynamicBodies.Add(rb);
        }

        if (!rb.globalSector) 
        {
            UpdateRigidbodySector(rb, true);
        }
        else
        {
            globalBodies.Add(rb);
        }
    }
    public void RemoveRigidbody(Rigidbody4D rb)
    {
        rigidbodyList.Remove(rb);
        staticBodies.Remove(rb);
        dynamicBodies.Remove(rb);
        globalBodies.Remove(rb);
    }

    public void UpdateRigidbodyStatic(Rigidbody4D rb)
    {
        if (rb.isStatic) {
            dynamicBodies.Remove(rb);
            staticBodies.Add(rb);
            if (rb.sectorIndex >= 0) {
                sectors[rb.sectorIndex].dynamicBodies.Remove(rb);
                sectors[rb.sectorIndex].staticBodies.Add(rb);
            }
        } else {
            dynamicBodies.Add(rb);
            staticBodies.Remove(rb);
            if (rb.sectorIndex >= 0) {
                sectors[rb.sectorIndex].dynamicBodies.Add(rb);
                sectors[rb.sectorIndex].staticBodies.Remove(rb);
            }
        }
    }

    public void UpdateRigidbodySector(Rigidbody4D rb, bool newObject = false) //-1 means is new and not currently part of any sector
    {
        Vector4 pos = rb.transform4.positionNorm;
        float boundingRadius = rb.boundingRadius;

        //int currentSector = newObject ? -1 : rb.sectorIndex;
        List<int> allCurrentSectors = rb.allSectors;

        List<int> allSectors = new List<int>();

        if (rb.globalSector || rb.boundingRadius > 4)
        {
            if (!globalBodies.Contains(rb)) globalBodies.Add(rb);
            //remove from sectors we arnt in anymore
            for (int i = 0; i < allCurrentSectors.Count; i++)
            {
                if (allCurrentSectors[i] == -1) continue;
                if (!allSectors.Contains(allCurrentSectors[i])) 
                {
                    sectors[allCurrentSectors[i]].AddRemove(rb, true);    
                }
            }
            rb.sectorIndex = -1;
            rb.allSectors.Clear();
            return;
        }

        { //find sectors of center
            int vecMax = UFunc.VectorSignificant(pos);
            int sector = vecMax * 2;
            if (pos[vecMax] < 0) sector += 1;
            allSectors.Add(sector);
        }

        CheckAxis(new Vector4(1,0,0,0));
        CheckAxis(new Vector4(0,1,0,0));
        CheckAxis(new Vector4(0,0,1,0));
        CheckAxis(new Vector4(0,0,0,1));

        void CheckAxis(Vector4 axis) //slerp pos towards axis by bounding radius, take that new point an take its largest component
        {
            Vector4 posNew = UFunc.Slerp4(pos, axis, boundingRadius);
            int vecMax = UFunc.VectorSignificant(posNew);

            int sector = vecMax * 2;
            if (posNew[vecMax] < 0) sector += 1;

            if (!allSectors.Contains(sector)) allSectors.Add(sector);
        }

        //add to sectors where we arnt already
        for (int i = 0; i < allSectors.Count; i++)
        {
            if (!allCurrentSectors.Contains(allSectors[i]) || newObject) 
            {
                sectors[allSectors[i]].AddRemove(rb, false);    
            }
        }
        //remove from sectors we arnt in anymore
        for (int i = 0; i < allCurrentSectors.Count; i++)
        {
            if (allCurrentSectors[i] == -1) continue;
            if (!allSectors.Contains(allCurrentSectors[i])) 
            {
                sectors[allCurrentSectors[i]].AddRemove(rb, true);    
            }
        }

        rb.sectorIndex = allSectors[0];
        rb.allSectors = allSectors;
    }

    void FixedUpdate()
    {
        //normal physics update
        DoPhysicsUpdate(0);

        //collision
        
        totalCollisionChecks = 0;

        foreach (Sector sec in sectors)
        {
            //do all dynamic against static collisions first
            foreach (Rigidbody4D rb in sec.dynamicBodies)
            {
                if (!RBValid(rb)) continue;
                foreach (Rigidbody4D staticRb in sec.staticBodies)
                {
                    if (!RBValid(staticRb)) continue;
                    if (!ObjectBoundOverlap(rb,staticRb)) continue;

                    CheckCollision(rb.collider,staticRb.collider);
                }
            }

            //do dynamic against dynamic collisions
            for (int i = 0; i < sec.dynamicBodies.Count; i++)
            {
                Rigidbody4D rb = sec.dynamicBodies[i];
                if (!RBValid(rb)) continue;
                for (int j = i+1; j < sec.dynamicBodies.Count; j++)
                {
                    Rigidbody4D rb2 = sec.dynamicBodies[j];
                    if (!RBValid(rb2)) continue;
                    if (!ObjectBoundOverlap(rb,rb2)) continue;

                    CheckCollision(rb.collider,rb2.collider);
                }
            }
        }

        foreach (Rigidbody4D rb in globalBodies)
        {
            if (!RBValid(rb)) continue;

            if (!rb.isStatic)
            {
                foreach (Rigidbody4D rbStatic in staticBodies)
                {
                    if (!RBValid(rbStatic)) continue;
                    if (!ObjectBoundOverlap(rb,rbStatic)) continue;

                    CheckCollision(rb.collider,rbStatic.collider);
                }
            }

            foreach (Rigidbody4D rbDynamic in dynamicBodies)
            {
                if (rbDynamic.collider == null) continue;
                if (!rbDynamic.enabled) continue;
                if (rb == rbDynamic) continue;
                if (!ObjectBoundOverlap(rb,rbDynamic)) continue;

                CheckCollision(rb.collider,rbDynamic.collider);
            }
        }

        //second physics update
        DoPhysicsUpdate(1);

        bool RBValid(Rigidbody4D rb)
        {
            return rb.collider != null && rb.enabled && rb.gameObject.activeInHierarchy;
        }
    }

    public void CheckCollision(ColliderS c1, ColliderS c2)
    {
        totalCollisionChecks++;

        bool yes = false;

        if (c1.isTrigger || c2.isTrigger)
        {
            yes = ColliderS.IsOverlap(c1, c2);
        }
        else
        {
            yes = ColliderS.CollisionPhysic(c1, c2);
        }

        if (yes)
        {
            c1.CollisionHappen(c2);
            c2.CollisionHappen(c1);
        }
    }

    public bool ObjectBoundOverlap(Rigidbody4D rb1, Rigidbody4D rb2)
    {
        float objDis = UFunc.DistanceS(rb1.transform4.positionNorm, rb2.transform4.positionNorm);
        return objDis < rb1.boundingRadius + rb2.boundingRadius;
    }

    void DoPhysicsUpdate(int i)
    {
        foreach (Rigidbody4D rb in rigidbodyList)
        {
            if (!rb.enabled) continue;

            if (i == 0)
            {
                rb.PhysicsUpdate();
            }
            else
            {
                rb.PhysicsUpdate2();
            }
        }
    }
}
