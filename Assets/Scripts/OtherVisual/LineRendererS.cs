using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Rendering;

public class LineRendererS : MonoBehaviour
{
    public bool normalizePos;
    public bool setVertex;

    public float lineThick = 0.1f;

    public List<Vector4> positions = new List<Vector4>();
    public List<Vector4> posNormals = new List<Vector4>();

    public Vector4[] vertex;
    public int[] triangles;
    public Vector2[] uv;

    new public MeshRenderer renderer;
    public MeshFilter filter;
    public Mesh mesh;

    MaterialPropertyBlock matBlock;

    public Color color;

    void OnValidate()
    {
        if (normalizePos)
        {
            normalizePos = false;
            NormalizePositions();
        }
        if (setVertex)
        {
            setVertex = false;
            NormalizePositions();
            SetVertex();
        }
    }

    void NormalizePositions()
    {
        for (int i = 0; i < positions.Count; i++)
        {
            positions[i] = positions[i].normalized;
        }
    }

    void OnEnable()
    {
        InitializeMesh();
    }
    void OnDisable()
    {
        if (mesh) Destroy(mesh);
    }
    void InitializeMesh()
    {
        if (!mesh)
        {
            mesh = new Mesh();
        }

        if (!renderer)
        {
            renderer = GetComponent<MeshRenderer>();
            if (!renderer) renderer = gameObject.AddComponent<MeshRenderer>();
        }
        if (!filter)
        {
            filter = GetComponent<MeshFilter>();
            if (!filter) filter = gameObject.AddComponent<MeshFilter>();
        }

        filter.mesh = mesh;

        Bounds newBounds = new Bounds();
        newBounds.max = new Vector3(99999,99999,99999);
        newBounds.min = new Vector3(-99999,-99999,-99999);
        renderer.bounds = newBounds;
    }

    void Update()
    {
        SetVertex();
    }

    public void AddPos(Vector4 pos, Vector4 norm)
    {
        positions.Add(pos.normalized);
        posNormals.Add(norm.normalized);
    }
    public void RemovePos()
    {
        positions.RemoveAt(0);
        posNormals.RemoveAt(0);
    }

    void SetVertex()
    {
        if (positions == null || positions.Count <= 1) return;

        vertex = new Vector4[positions.Count * 2];
        triangles = new int[12 * (positions.Count - 1)];
        uv = new Vector2[positions.Count * 2];

        for (int i = 0; i < positions.Count; i++)
        {
            Vector4 v1 = UFunc.Slerp4Angle(positions[i], posNormals[i], lineThick);
            Vector4 v2 = UFunc.Slerp4Angle(positions[i], posNormals[i], -lineThick);

            vertex[2 * i + 0] = v1;
            vertex[2 * i + 1] = v2;

            uv[2 * i + 0] = new Vector2(i / (positions.Count - 1), 1);
            uv[2 * i + 1] = new Vector2(i / (positions.Count - 1), 0);

            if (i == positions.Count - 1) continue;

            triangles[12 * i + 0] = 2 * i + 0;
            triangles[12 * i + 1] = 2 * i + 1;
            triangles[12 * i + 2] = 2 * i + 3;

            triangles[12 * i + 3] = 2 * i + 0;
            triangles[12 * i + 4] = 2 * i + 3;
            triangles[12 * i + 5] = 2 * i + 2;

            triangles[12 * i + 6] = 2 * i + 1;
            triangles[12 * i + 7] = 2 * i + 0;
            triangles[12 * i + 8] = 2 * i + 3;

            triangles[12 * i + 9] = 2 * i + 3;
            triangles[12 * i + 10] = 2 * i + 0;
            triangles[12 * i + 11] = 2 * i + 2;
        }

        if (!mesh) InitializeMesh();

        var layout = new[]
        {
            new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 4),
            new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2)
        };

        int vertexCount = vertex.Length;
        mesh.SetVertexBufferParams(vertexCount, layout);

        Vertex4D[] vertexArray = new Vertex4D[vertexCount];

        for (int i = 0; i < vertexCount; i++)
        {
            vertexArray[i] = new Vertex4D()
            {
                pos = vertex[i],
                uv = uv[i]
            };
        }

        mesh.SetVertexBufferData(vertexArray, 0, 0, vertexCount);

        mesh.triangles = triangles;

        if (matBlock == null) matBlock = new MaterialPropertyBlock();
        //matBlock.SetVector("_Scale", new Vector4(transformScale.x,transformScale.y,transformScale.z,0));
        matBlock.SetVector("_MatC0", new Vector4(1,0,0,0));
        matBlock.SetVector("_MatC1", new Vector4(0,1,0,0));
        matBlock.SetVector("_MatC2", new Vector4(0,0,1,0));
        matBlock.SetVector("_MatC3", new Vector4(0,0,0,1));

        matBlock.SetVector("_Color", color);
        
        matBlock.SetFloat("_Radius", Transform4D.radius);

        matBlock.SetFloat("_DoV4", 1f);

        matBlock.SetFloat("_Lit", 1f);
        matBlock.SetFloat("_DoubleSideLit", 1);

        renderer.SetPropertyBlock(matBlock);
    }
    
    public struct Vertex4D
    {
        public Vector4 pos;
        public Vector2 uv;
    }
}
