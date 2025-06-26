using UnityEngine;

public class BadPhysic : MonoBehaviour
{
    [SerializeField] Vector3[] verticies = new Vector3[4];
    [SerializeField] Face[] faces;

    int[] edges;

    [System.Serializable]
    public struct Face
    {
        BadPhysic parent;

        public int[] vertexIndicies;
        public Vector3 normal;

        public Vector3[] edgeNormals;

        public Vector3 this[int i]
        {
            get {
                return GetVertex(i);
            }
            set {
                parent.verticies[vertexIndicies[i]] = value;
            }
        }
        public Vector3 GetVertex(int i) {
            if (i > vertexIndicies.Length) i = i % vertexIndicies.Length;
            return parent.verticies[vertexIndicies[i]];
        }

        public Vector3 normalOffset {get{return -parent.transform.position - GetVertex(0);}}

        public Face(BadPhysic par, params int[] vertexIndex)
        {
            parent = par;
            vertexIndicies = vertexIndex;
            normal = UFunc.TriNormal(
                parent.verticies[vertexIndicies[0]],
                parent.verticies[vertexIndicies[1]],
                parent.verticies[vertexIndicies[2]]
            );

            edgeNormals = new Vector3[vertexIndicies.Length];
            for (int i = 0; i < edgeNormals.Length; i++) {
                int iNext = i == edgeNormals.Length-1 ? 0 : i+1;
                edgeNormals[i] = Vector3.Cross(parent.verticies[vertexIndicies[iNext]]-parent.verticies[vertexIndicies[i]], normal).normalized;
            }
        }

        public bool InEdgeNormalBound(Vector3 v)
        {
            for (int i = 0; i < edgeNormals.Length; i++)
            {
                if (UFunc.Dot(v-GetVertex(i), edgeNormals[i]) > 0) return false;
            }
            return true;
        }
    }

    [SerializeField] bool initTetra;

    [SerializeField] bool testPoint;
    [SerializeField] Vector3 testPointPoint;
    [SerializeField] Vector3 testPointPoint2;

    [SerializeField] bool testLineIntersect;

    [SerializeField] bool testCollision;
    [SerializeField] BadPhysic testCollisionObject;

    void OnValidate()
    {
        if (initTetra) {
            initTetra = false;
            InitializeTetrahedron();
        }

        if (testPoint) {
            testPoint = false;
            print(OverlapPoint(testPointPoint));
        }

        if (testLineIntersect) {
            testLineIntersect = false;
            Vector3 intersect = new Vector3();
            print(LineIntersectFacePoint(testPointPoint, testPointPoint2, faces[1], ref intersect));
            print(intersect);
        }

        if (testCollision) {
            testCollision = false;
            transform.position += testCollisionObject.PhysicCollision(this);
        }
    }

    void InitializeTetrahedron()
    {
        verticies = new Vector3[] {
            new Vector3(0,0,0),
            new Vector3(1,0,0),
            new Vector3(0,1,0),
            new Vector3(0,0,1)
        };
        faces = new Face[] {
            new Face(this, 0,2,1),
            new Face(this, 0,1,3),
            new Face(this, 0,3,2),
            new Face(this, 1,2,3)
        };

        edges = new int[] {
            0,1,
            0,2,
            0,3,
            1,2,
            1,3,
            2,3
        };
    }

    void Update()
    {

    }

    
    //return the velocity from the collision
    public Vector3 PhysicCollision(BadPhysic col)
    {
        for (int i = 0; i < faces.Length; i++)
        {
            Face face = faces[i];

            float[] dots = new float[col.verticies.Length];
            float smallestDot = 0;
            bool positive = true;
            
            for (int j = 0; j < col.verticies.Length; j++)
            {
                dots[j] = UFunc.Dot(col.verticies[j] + face.normalOffset, face.normal);
                if (dots[j] < 0)  {
                    positive = false;
                    if (dots[j] < smallestDot) smallestDot = dots[j];
                }
            }

            if (positive == true) return Vector3.zero; //if all verticies are to one side of one of the faces, they dont collide

            return -face.normal*smallestDot;
        }

        return Vector3.zero;
    }

    public bool OverlapPoint(Vector3 point)
    {
        point = point - transform.position;

        bool negative = true;
        for (int i = 0; i < faces.Length; i++) {
            float dot = UFunc.Dot(point-faces[i][0], faces[i].normal);
            if (dot > 0) {
                negative = false; 
                break;
            }
        }

        return negative;
    }

    public bool LineIntersectFacePoint(Vector3 v1, Vector3 v2, Face f, ref Vector3 intersectPos)
    {
        Vector3 posOffset = -transform.position - f[0];
        float dot1 = UFunc.Dot(v1 + posOffset, f.normal);
        float dot2 = UFunc.Dot(v2 + posOffset, f.normal);

        print(dot1.ToString() + " " + dot2.ToString());

        if (Mathf.Sign(dot1) == Mathf.Sign(dot2)) return false; //they dont cross the face

        float t = Mathf.Abs(dot1) / (Mathf.Abs(dot1)+Mathf.Abs(dot2));
        print(t);
        intersectPos = UFunc.LerpVec3(v1, v2, t);

        return f.InEdgeNormalBound(intersectPos - transform.position);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;

        Vector3[] linePoints = new Vector3[edges.Length];
        for (int i = 0; i < edges.Length; i++) {linePoints[i] = verticies[edges[i]] + transform.position;}

        Gizmos.DrawLineList(linePoints);
    }
}
