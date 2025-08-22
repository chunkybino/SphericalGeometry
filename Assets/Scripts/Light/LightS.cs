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

    public bool setShadows;

    public Vector4[] shadowSideNormals = new Vector4[0];
    public Vector4[] dynamicShadowSideNormals = new Vector4[0];

    public List<Vector4> allSideNormals = new List<Vector4>();
    public List<Vector3Int> sideNormalSpan = new List<Vector3Int>(); //buffers start, face normal start index, buffer end

    public List<Renderer4D> sideNormalRenderer = new List<Renderer4D>();

    public ComputeShader edgeVertexCompute;

    public bool[,,] shadowMapData;
    public ComputeShader shadowMapCompute;

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
            Vector4[] vertexWorld = ren.GetVertexWorld();

            //Vector3 direction = ren.transform4.RelativeDirectionTo(transform4.positionNorm).normalized;
            Vector3 direction = UFunc.SterographicProjection(transform4.matrix.transpose * ren.transform4.positionNorm).normalized;
            //Vector3 direction = UFunc.GnomonicProjection(transform4.matrix.transpose * ren.transform4.positionNorm);

            ComputeBuffer vertexDataBuffer = new ComputeBuffer(vertexWorld.Length, sizeof(float)*5);


            EdgeVertexData[] vertexData = new EdgeVertexData[vertexWorld.Length];
            for (int j = 0; j < vertexData.Length; j++) {
                //vertexData[j].vertex = renVerticies[j];
                vertexData[j].vertex = UFunc.SterographicProjection(transform4.matrix.transpose * vertexWorld[j]);
                //vertexData[j].vertex = UFunc.GnomonicProjection(transform4.matrix.transpose * vertexWorld[j]);
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

            edgeVertexCompute.Dispatch(0, Mathf.CeilToInt((float)vertexWorld.Length / 8), 1, 1);

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

            int sideNormalCount = outsideIndex.Count;
            int faceNormalsCount = outsideIndex.Count - 2;

            sideNormalSpan.Add(new Vector3Int(allSideNormals.Count , allSideNormals.Count+sideNormalCount , allSideNormals.Count+sideNormalCount+faceNormalsCount));

            float furthestDistance = UFunc.DistanceS(transform4.positionNorm,ren.transform4.positionNorm);

            //Vector4[] vertexWorld = ren.GetVertexWorld();
            for (int j = 0; j < outsideIndex.Count; j++)
            {
                //furthestDistance = Mathf.Min(UFunc.DistanceS(transform4.positionNorm,vertexWorld[outsideIndex[j]]));

                allSideNormals.Add(UFunc.HyperCross(transform4.positionNorm,GetVertexWorld(j),GetVertexWorld(j+1)));
            }

            //face normal time
            int[] faceIndex = new int[] {0,1,outsideIndex.Count-1};
            for (int j = 0; j < outsideIndex.Count - 2; j++)
            {
                Vector4 cross = UFunc.HyperCross(GetVertexWorld(faceIndex[0]),GetVertexWorld(faceIndex[1]),GetVertexWorld(faceIndex[2]));

                allSideNormals.Add(cross);

                //print(Vector4.Dot(transform4.positionNorm,cross.normalized));

                if (j % 2 == 0)
                {
                    faceIndex[0] = faceIndex[1];
                    faceIndex[1]++;
                }
                else
                {
                    faceIndex[0] = faceIndex[2];
                    faceIndex[2]--;
                }
            }

            //print(furthestDistance);
            //allSideNormals[outsideIndex.Count] = new Vector4(furthestDistance,0,0,0);

            Vector4 GetVertexWorld(int index)
            {
                if (index >= outsideIndex.Count) return vertexWorld[outsideIndex[0]];
                return vertexWorld[outsideIndex[index]];
            }
        }
    }

    public void SetRaycastShadowNormals(bool setStatic)
    {
        Vector4[] inVertex = setStatic ? LightHandlerS.singleton.shadowTriNormals : LightHandlerS.singleton.dynamicShadowTriNormals;
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
    }

    public void SetShadowMap()
    {
        List<Vector4> shadowVerticies = lightHandler.shadowTriVerticies;
        Vector4[] shadowTri = new Vector4[shadowVerticies.Count * 4/3];

        Vector3 lightPos = UFunc.GnomonicProjection(transform4.positionNorm);

        for (int i = 0; i < shadowVerticies.Count/3; i++)
        {
            Vector3 v1 = UFunc.GnomonicProjection(shadowVerticies[3*i + 0]);
            Vector3 v2 = UFunc.GnomonicProjection(shadowVerticies[3*i + 1]);
            Vector3 v3 = UFunc.GnomonicProjection(shadowVerticies[3*i + 2]);

            Vector3 cross = Vector3.Cross((v2-v1),(v3-v1)).normalized;
            cross /= Vector3.Dot((v1-lightPos),cross); //give the face normal a length of the inverse distance from the light to the plane
            //this way we know if a point is past the plane by checking if its dot product with the face normal is greater than 1

            Vector3 norm1 = Vector3.Cross((v1-lightPos),(v2-lightPos));
            Vector3 norm2 = Vector3.Cross((v2-lightPos),(v3-lightPos));
            Vector3 norm3 = Vector3.Cross((v3-lightPos),(v1-lightPos));

            shadowTri[4*i + 0] = cross;
            shadowTri[4*i + 1] = norm1;
            shadowTri[4*i + 2] = norm2;
            shadowTri[4*i + 3] = norm3;
        }

        int resolution = 16;
        int threads = 8;

        shadowMapData = new bool[resolution,resolution,resolution]; 
        int cubeSize = resolution*resolution*resolution;

        ComputeBuffer shadowTriBuff = new ComputeBuffer(shadowTri.Length/4, sizeof(float)*16);
        shadowTriBuff.SetData(shadowTri);
        shadowMapCompute.SetBuffer(0, "shadowTri", shadowTriBuff);

        ComputeBuffer shadowMapBuff = new ComputeBuffer(cubeSize, sizeof(bool)*cubeSize);
        shadowMapBuff.SetData(shadowMapData);
        shadowMapCompute.SetBuffer(0, "map", shadowMapBuff);

        shadowMapCompute.SetInt("resolution", resolution);
        shadowMapCompute.SetVector("lightPos", lightPos);

        edgeVertexCompute.Dispatch(0, resolution/threads,resolution/threads,resolution/threads);

        shadowTriBuff.Release();
        shadowMapBuff.GetData(shadowMapData);
        shadowMapBuff.Release();
    }
}
