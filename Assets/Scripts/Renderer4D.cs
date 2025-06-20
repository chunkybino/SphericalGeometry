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
    public Mesh mesh;

    public Color meshColor = Color.white;

    MaterialPropertyBlock matBlock;

    public bool initialize;

    public Matrix4x4 transformMatrix;
    public Vector3 transformScale;

    //public Vertex4D[] vertexArray;

    [SerializeField] Vector3[] vertices;
    [SerializeField] Vector2[] uvs;
    [SerializeField] int[] triangles;

    
    [SerializeField] bool doVertex4;
    /*
    [SerializeField] Vector4[] vertices4;
    [SerializeField] bool setVertex4;
    */

    void Start()
    {
        Initialize();
    }

    void OnValidate()
    {
        if (initialize) {
            initialize = false;
            Initialize();
        }

        /*
        if (doVertex4 && setVertex4) {
            setVertex4 = false;
            SetVertex4();
        }
        */
    }

    void Update()
    {
        if (!transform4 || !renderer || !filter) return;

        transformMatrix = transform4.matrix.inverse;
        transformScale = transform4.scale;

        transformMatrix = transform4.matrix.inverse;
        transformScale = transform4.scale;

        if (matBlock == null) matBlock = new MaterialPropertyBlock();

        matBlock.SetVector("_Scale", new Vector4(transformScale.x,transformScale.y,transformScale.z,0));
        matBlock.SetVector("_MatC0", transformMatrix.GetColumn(0));
        matBlock.SetVector("_MatC1", transformMatrix.GetColumn(1));
        matBlock.SetVector("_MatC2", transformMatrix.GetColumn(2));
        matBlock.SetVector("_MatC3", transformMatrix.GetColumn(3));

        matBlock.SetVector("_Color", meshColor);
        
        matBlock.SetFloat("_Radius", Transform4D.radius);

        matBlock.SetFloat("_DoV4", doVertex4 ? 1f : 0f);

        renderer.SetPropertyBlock(matBlock);
    }

    void Initialize()
    {
        mesh = filter.sharedMesh;

        matBlock = new MaterialPropertyBlock();

        Bounds newBounds = new Bounds();
        newBounds.max = new Vector3(99999,99999,99999);
        newBounds.min = new Vector3(-99999,-99999,-99999);
        renderer.bounds = newBounds;
    }

    /*
    public struct Vertex4D
    {
        public Vector4 pos;
        public Vector2 uv;
    }

    void SetVertex4()
    {
        var layout = new[]
        {
            new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 4),
            new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2)
        };

        int vertexCount = vertices4.Length;
        mesh.SetVertexBufferParams(vertexCount, layout);

        vertexArray = new Vertex4D[vertexCount];

        for (int i = 0; i < vertexCount; i++) 
        {
            vertexArray[i] = new Vertex4D() {
                pos = vertices4[i],
                uv = uvs[i]
            };
        }

        mesh.triangles = triangles;
        mesh.SetVertexBufferData(vertexArray, 0, 0, vertexCount);
    }
    */
}
