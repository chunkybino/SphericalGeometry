using UnityEngine;
using UnityEngine.Rendering;

public class TronRingMachine : MonoBehaviour
{
    
    new public MeshRenderer renderer;
    public MeshFilter filter;
    public Mesh mesh;

    public int axis1 = 0;
    public int axis2 = 1;
    int altAxis1 = 2;
    int altAxis2 = 3;

    public float thickness = 0.1f;
    public int subdiv = 32;

    public Vector4[] vertex;
    public Vector2[] uv;
    public int[] triangles;

    public bool setVertex;

    void OnValidate()
    {
        if (setVertex)
        {
            setVertex = false;
            SetVertex();
        }
    }


    void OnEnable()
    {
        InitializeMesh();
        
        SetVertex();
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
        newBounds.max = new Vector3(99999, 99999, 99999);
        newBounds.min = new Vector3(-99999, -99999, -99999);
        renderer.bounds = newBounds;
    }
    

    void SetVertex()
    {
        if (subdiv < 1) return;

        vertex = new Vector4[subdiv * 4];
        triangles = new int[subdiv * 8 * 3 * 2];
        uv = new Vector2[subdiv * 4];

        altAxis1 = -1;
        for (int i = 0; i < 4; i++)
        {
            if (i != axis1 && i != axis2)
            {
                if (altAxis1 == -1)
                {
                    altAxis1 = i;
                }
                else
                {
                    altAxis2 = i;
                }
            }
        }

        Vector4 axisV1 = new Vector4();
        Vector4 axisV2 = new Vector4();
        Vector4 axisThick1 = new Vector4();
        Vector4 axisThick2 = new Vector4();
        axisV1[axis1] = 1;
        axisV2[axis2] = 1;
        axisThick1[altAxis1] = 1;
        axisThick2[altAxis2] = 1;

        Vector4[] linePos = new Vector4[subdiv];
        float subdivAngle = Mathf.PI * 2 / subdiv;
        for (int i = 0; i < subdiv; i++)
        {
            linePos[i] = UFunc.Slerp4Angle(axisV1, axisV2, subdivAngle * i);
        }

        for (int i = 0; i < subdiv; i++)
        {
            int vertexIndexOffset = 4 * i;

            Vector4 lineV = linePos[i];
            vertex[vertexIndexOffset + 0] = UFunc.Slerp4Angle(lineV, axisThick1, thickness);
            vertex[vertexIndexOffset + 1] = UFunc.Slerp4Angle(lineV, axisThick2, thickness);
            vertex[vertexIndexOffset + 2] = UFunc.Slerp4Angle(lineV, axisThick1, -thickness);
            vertex[vertexIndexOffset + 3] = UFunc.Slerp4Angle(lineV, axisThick2, -thickness);

            uv[vertexIndexOffset + 0] = new Vector2(0, 1);
            uv[vertexIndexOffset + 1] = new Vector2(1, 0);
            uv[vertexIndexOffset + 2] = new Vector2(1, 0.5f);
            uv[vertexIndexOffset + 3] = new Vector2(0.5f, 1);

            int triIndexOffset = 24 * i;
            for (int j = 0; j < 4; j++)
            {
                triangles[triIndexOffset + 6 * j + 0] = vertexIndexOffset + j + 0;
                triangles[triIndexOffset + 6 * j + 1] = vertexIndexOffset + j + 1;
                triangles[triIndexOffset + 6 * j + 2] = vertexIndexOffset + j + 4;

                triangles[triIndexOffset + 6 * j + 3] = vertexIndexOffset + j + 4;
                triangles[triIndexOffset + 6 * j + 4] = vertexIndexOffset + j + 1;
                triangles[triIndexOffset + 6 * j + 5] = vertexIndexOffset + j + 5;

                if (j == 3)
                {
                    triangles[triIndexOffset + 6 * j + 1] = vertexIndexOffset;
                    triangles[triIndexOffset + 6 * j + 4] = vertexIndexOffset;
                    triangles[triIndexOffset + 6 * j + 5] = vertexIndexOffset + 4;
                }
                if (i == subdiv - 1)
                {
                    triangles[triIndexOffset + 6 * j + 2] = j;
                    triangles[triIndexOffset + 6 * j + 3] = j;
                    triangles[triIndexOffset + 6 * j + 5] = j + 1;
                    if (j == 3)
                    {
                        triangles[triIndexOffset + 6 * j + 5] = 0;
                    }
                }

                //double tri
                triangles[subdiv * 24 + triIndexOffset + 6 * j + 0] = triangles[triIndexOffset + 6 * j + 1];
                triangles[subdiv * 24 + triIndexOffset + 6 * j + 1] = triangles[triIndexOffset + 6 * j + 0];
                triangles[subdiv * 24 + triIndexOffset + 6 * j + 2] = triangles[triIndexOffset + 6 * j + 2];

                triangles[subdiv * 24 + triIndexOffset + 6 * j + 3] = triangles[triIndexOffset + 6 * j + 4];
                triangles[subdiv * 24 + triIndexOffset + 6 * j + 4] = triangles[triIndexOffset + 6 * j + 3];
                triangles[subdiv * 24 + triIndexOffset + 6 * j + 5] = triangles[triIndexOffset + 6 * j + 5];
            }
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

        /*
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
        */
    }
    
    public struct Vertex4D
    {
        public Vector4 pos;
        public Vector2 uv;
    }
}
