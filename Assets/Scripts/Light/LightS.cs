using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Rendering;

[ExecuteAlways]
public class LightS : MonoBehaviour
{
    LightHandlerS lightHandler;

    public Transform4D transform4;

    public LightData data;

    //int m_lightIndex;
    //public int lightIndex {get{return m_lightIndex;}set{m_lightIndex = value;}}

    public Color color;
    public float intensity;

    public bool doFalloff;
    //public float falloffStart;
    //public float falloffRange;

    public float falloffDegree = 2;

    public float ambience;

    public Vector3 direction = new Vector3(1,0,0);
    Vector4 direction4;
    [Range(0,180)] public float range = 180;
    [Range(0,180)] public float rangeFalloff = 180;

    public bool castShadows;
    bool m_castShadows;

    public bool dirty;

    public ComputeShader shadowMapCompute;
    public ComputeBuffer shadowMapGeometryBuffer;
    public ComputeBuffer shadowMapShadowBuffer;
    //public bool[,,] shadowMapData;

    public bool setShadows;

    public Vector4[] shadowSideNormals = new Vector4[0];
    public Vector4[] dynamicShadowSideNormals = new Vector4[0];

    public List<Vector4> allSideNormals = new List<Vector4>();
    public List<Vector2Int> sideNormalSpan = new List<Vector2Int>();

    public List<Renderer4D> sideNormalRenderer = new List<Renderer4D>();

    public ComputeShader edgeVertexCompute;

    void OnEnable()
    {
        lightHandler = LightHandlerS.singleton;
        if (!transform4) transform4 = GetComponent<Transform4D>();

        lightHandler.AddLight(this);

        if (!edgeVertexCompute) edgeVertexCompute = lightHandler.edgeVertexCompute;
    }
    void OnDisable()
    {
        lightHandler.RemoveLight(this);
    }

    void Update()
    {
        if (setShadows)
        {
            setShadows = false;
            SetShadowNormals(true);
        }

        if (m_castShadows != castShadows) {
            m_castShadows = castShadows;
            lightHandler.UpdateLightCastShadow();
        }

        direction4 = transform4.RelativeToWorld(direction.normalized);
        //rangeCos = Mathf.Cos(Mathf.Deg2Rad * range);
        //rangeFalloffCos = Mathf.Cos(Mathf.Deg2Rad * Mathf.Clamp(range+rangeFalloff,0,180));
        //if (rangeFalloffCos > rangeCos) rangeFalloffCos = rangeCos;

        if (
            data.position != transform4.positionNorm ||
            data.color != new Vector3(color.r,color.g,color.b) ||
            data.intensity != intensity ||
            //data.doFalloff != (doFalloff ? 1 : 0) ||
            //data.falloffStart != falloffStart ||
            //data.falloffRange != falloffRange ||
            data.falloffDegree != falloffDegree ||
            data.ambience != ambience ||
            data.direction != direction4 ||
            data.rangeAngle != range ||
            data.rangeFalloffAngleMult != rangeFalloff
            ) {
            dirty = true;
        }

        data.position = transform4.positionNorm;
        data.color = new Vector3(color.r,color.g,color.b);
        data.intensity = intensity;

        //data.doFalloff = doFalloff ? 1 : 0;
        /*
        data.falloffStart = falloffStart;
        if (falloffRange != 0) {
            data.falloffRange = 1/falloffRange;
        } else {
            data.falloffRange = 9999;
        }
        */

        data.falloffDegree = doFalloff ? falloffDegree : 0;

        data.ambience = ambience;

        data.direction = direction4;
        data.rangeAngle = Mathf.Deg2Rad*range;
        if (rangeFalloff != 0) {
            data.rangeFalloffAngleMult = 1/(Mathf.Deg2Rad*rangeFalloff);
        } else {
            data.rangeFalloffAngleMult = 9999;
        }
    }

    struct EdgeVertexData
    {
        public Vector3 vertex;
        public int nextIndex;
        public int result;
    }

    public void SetShadowNormals(bool setStatic)
    {
        List<Renderer4D> staticShadows = LightHandlerS.singleton.staticShadowRenderers;
        //List<Renderer4D> dynamicShadows = LightHandlerS.singleton.dynamicShadowRenderers;
        List<Renderer4D> dynamicShadows = sideNormalRenderer;

        allSideNormals.Clear();
        sideNormalSpan.Clear();

        for (int i = 0; i < dynamicShadows.Count; i++)
        {
            Renderer4D ren = dynamicShadows[i];

            Vector3[] renVerticies = ren.GetVertex3();
            Vector3 direction = ren.transform4.RelativeDirectionTo(transform4.positionNorm).normalized;

            ComputeBuffer vertexDataBuffer = new ComputeBuffer(renVerticies.Length, sizeof(float)*5);

            EdgeVertexData[] vertexData = new EdgeVertexData[renVerticies.Length];
            for (int j = 0; j < vertexData.Length; j++) {
                vertexData[j].vertex = renVerticies[j];
            }

            /*
            for (int j = 0; j < vertexData.Length; j++)
            {
                EdgeFind(vertexData, direction, j);
            }
            */
            vertexDataBuffer.SetData(vertexData);

            edgeVertexCompute.SetBuffer(0, "verticies", vertexDataBuffer);
            edgeVertexCompute.SetVector("direction4", direction);

            edgeVertexCompute.Dispatch(0, Mathf.CeilToInt((float)renVerticies.Length / 8), 1, 1);

            vertexDataBuffer.GetData(vertexData);
            vertexDataBuffer.Release();

            List<int> outsideIndex = new List<int>();
            int nextIndex = -1;
            int startEdgeIndex = 0;
            for (int j = 0; j < vertexData.Length; j++)
            {
                if (nextIndex == -1)
                {
                    if (vertexData[j].result == 1) {
                        nextIndex = j;
                        startEdgeIndex = j;
                    } else {
                        continue;
                    }
                }

                outsideIndex.Add(nextIndex);
                nextIndex = vertexData[nextIndex].nextIndex;

                if (startEdgeIndex == nextIndex) break;
            }

            sideNormalSpan.Add(new Vector2Int(allSideNormals.Count,allSideNormals.Count+outsideIndex.Count));

            Vector4[] vertexWorld = ren.GetVertexWorld();
            for (int j = 0; j < outsideIndex.Count; j++)
            {
                allSideNormals.Add(-UFunc.HyperCross(transform4.positionNorm,GetVertexWorld(j),GetVertexWorld(j+1)));
            }

            Vector4 GetVertexWorld(int index)
            {
                if (index >= outsideIndex.Count) return vertexWorld[outsideIndex[0]];
                return vertexWorld[outsideIndex[index]];
            }
        }

        
        /*
        List<Vector4>
        //Vector4[] inVertex = ;//setStatic ? LightHandlerS.singleton.shadowTriVerticies : LightHandlerS.singleton.dynamicShadowTriVerticies;
        Vector4[] outVertex = new Vector4[inVertex.Length * 5/4];

        for (int i = 0; i < outVertex.Length/5; i++)
        {
            Vector4 center = inVertex[4*i + 0];
            Vector4 v1 = inVertex[4*i + 1];
            Vector4 v2 = inVertex[4*i + 2];
            Vector4 v3 = inVertex[4*i + 3];

            outVertex[5*i + 0] = center;

            outVertex[5*i + 1] = -center + Vector4.Dot(center,transform4.positionNorm)*transform4.positionNorm;

            outVertex[5*i + 2] = UFunc.HyperCross(transform4.positionNorm,v1,v2);
            outVertex[5*i + 3] = UFunc.HyperCross(transform4.positionNorm,v2,v3);
            outVertex[5*i + 4] = UFunc.HyperCross(transform4.positionNorm,v3,v1);
        }

        if (setStatic) {
            shadowSideNormals = outVertex;
        } else {
            dynamicShadowSideNormals = outVertex;
        }
        */
    }
}
