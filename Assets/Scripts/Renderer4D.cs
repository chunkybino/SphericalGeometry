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

    public MeshGroup meshGroup;
    [SerializeField] bool doLOD;
    [SerializeField] float LODdistanceMult = 1; //have higher level meshes happen soon if this number is higher

    public Color meshColor = Color.white;

    MaterialPropertyBlock matBlock;

    public bool initialize;

    public Matrix4x4 transformMatrix;
    public Vector3 transformScale;

    [SerializeField] Vector3[] vertices;
    [SerializeField] Vector2[] uvs;
    [SerializeField] int[] triangles;
    
    [SerializeField] bool doVertex4;

    [SerializeField] bool lit = true;
    [SerializeField] bool doubleSideLit = false;

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

        matBlock.SetFloat("_Lit", lit ? 1f : 0f);
        matBlock.SetFloat("_DoubleSideLit", doubleSideLit ? 1f : 0f);

        renderer.SetPropertyBlock(matBlock);

        if (doLOD && meshGroup && Camera4D.mainCamera)
        {
            float dis = UFunc.DistanceS(transform4.positionNorm, Camera4D.mainCamera.transform4.positionNorm);
            filter.sharedMesh = meshGroup.GetMeshFromDistance(dis*LODdistanceMult);
        }
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
}
