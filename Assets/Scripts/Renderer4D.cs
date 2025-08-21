using UnityEngine;
using UnityEngine.Rendering;
using Unity.Collections;
using System.Collections.Generic;
using System.Collections;

[ExecuteAlways]
public class Renderer4D : MonoBehaviour
{
    public Transform4D transform4;

    new public MeshRenderer renderer;
    public MeshFilter filter;
    //public Mesh mesh;

    //public MeshGroup meshGroup;
    //[SerializeField] bool doLOD;
    //[SerializeField] float LODdistanceMult = 1; //have higher level meshes happen soon if this number is higher

    public Color meshColor = Color.white;

    MaterialPropertyBlock matBlock;

    public bool initialize;

    public Matrix4x4 transformMatrix;
    public Vector3 scaleMult = new Vector3(1,1,1);
    public Vector3 transformScale; 

    [SerializeField] Vector3[] vertices;
    Vector4[] verticiesWorld;
    //bool verticesWorldDirty;

    [SerializeField] Vector4[] vertex4;
    [SerializeField] Vector2[] uvs;
    [SerializeField] int[] triangles;
    
    public bool doVertex4;

    public bool lit = true;
    public bool doubleSideLit = false;

    public bool castShadows = false;
    bool m_castShadows = false;

    public bool isStatic;

    void Start()
    {
        Initialize();

        if (doVertex4) {
            vertex4 = ReadVertex4();
        } else {
            vertices = filter.sharedMesh.vertices;
        }
        triangles = filter.sharedMesh.triangles;

        //verticesWorldDirty = true;
    }

    void OnEnable()
    {
        if (!isStatic)
        {
            Rigidbody4D rb = GetComponent<Rigidbody4D>();
            if (rb) isStatic = rb.isStatic;
        }

        if (castShadows) {
            LightHandlerS.singleton.AddShadow(this, isStatic);
        }
    }
    void OnDisable()
    {
        if (castShadows) {
            LightHandlerS.singleton.AddShadow(this, isStatic);
        }
    }

    void OnValidate()
    {
        if (!gameObject.activeInHierarchy) return;

        if (initialize) {
            initialize = false;
            Initialize();
        }

        if (!LightHandlerS.singleton) return;
    }

    void Update()
    {
        //verticesWorldDirty = true;

        if (!transform4 || !renderer || !filter) return;

        transformMatrix = transform4.matrix.inverse;
        transformScale = scaleMult * transform4.scale;

        if (matBlock == null) matBlock = new MaterialPropertyBlock();

        matBlock.SetVector("_Scale", new Vector4(transformScale.x,transformScale.y,transformScale.z,0));
        matBlock.SetVector("_MatC0", transformMatrix.GetColumn(0));
        matBlock.SetVector("_MatC1", transformMatrix.GetColumn(1));
        matBlock.SetVector("_MatC2", transformMatrix.GetColumn(2));
        matBlock.SetVector("_MatC3", transformMatrix.GetColumn(3));

        matBlock.SetVector("_Color", meshColor);
        
        matBlock.SetFloat("_Radius", Transform4D.radius);

        matBlock.SetFloat("_DoV4", doVertex4 ? 1f : 0f);

        matBlock.SetFloat("_Lit", lit ? 1f : 0f);
        matBlock.SetFloat("_DoubleSideLit", doubleSideLit ? 1f : 0f);

        renderer.SetPropertyBlock(matBlock);



        if (castShadows != m_castShadows)
        {
            if (castShadows) {
                LightHandlerS.singleton.AddShadow(this, isStatic);
            } else {
                LightHandlerS.singleton.RemoveShadow(this, isStatic);
            }
        }
        m_castShadows = castShadows;

        /*
        if (doLOD && meshGroup && Camera4D.mainCamera)
        {
            float dis = UFunc.DistanceS(transform4.positionNorm, Camera4D.mainCamera.transform4.positionNorm);
            filter.sharedMesh = meshGroup.GetMeshFromDistance(dis*LODdistanceMult);
        }
        */
    }

    void Initialize()
    {
        //mesh = filter.sharedMesh;

        matBlock = new MaterialPropertyBlock();

        Bounds newBounds = new Bounds();
        newBounds.max = new Vector3(99999,99999,99999);
        newBounds.min = new Vector3(-99999,-99999,-99999);
        renderer.bounds = newBounds;
    }

    public Vector3[] GetVertex3()
    {
        return vertices;
    }
    public Vector4[] GetVertexWorld()
    {
        //if (!verticesWorldDirty) return verticiesWorld;

        verticiesWorld = new Vector4[vertices.Length];

        for (int i = 0; i < vertices.Length; i++)
        {
            verticiesWorld[i] = transform4.matrix * UFunc.SterographicInverse(Vector3.Scale(transformScale, vertices[i]),1);
        }

        return verticiesWorld;
    }
    public int[] GetTri()
    {
        return triangles;
    }

    public Vector4[] GetVertex4()
    {
        return vertex4;
    }

    public struct Vertex4D
    {
        public Vector4 pos;
        public Vector2 uv;
    }

    public Vector4[] ReadVertex4()
    {
        Vector4[] outV = new Vector4[0];

        using (var data = Mesh.AcquireReadOnlyMeshData(filter.sharedMesh))
        {
            NativeArray<Vertex4D> verts = data[0].GetVertexData<Vertex4D>();

            outV = new Vector4[verts.Length];
            for (int i = 0; i < verts.Length; i++)
            {
                outV[i] = verts[i].pos;
            }
        }

        return outV;
    }
}
