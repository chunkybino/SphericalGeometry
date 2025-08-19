using UnityEngine;
using System.Collections.Generic;

public class SphericalLightMap : MonoBehaviour
{
    public int subs = 4;
    public int gridSize;

    public List<Vector3Int> pointList = new List<Vector3Int>();
    public Dictionary<Vector3Int,int> pointDict = new Dictionary<Vector3Int,int>();

    public int indexLength = 0;

    public int[] indexMap = new int[0];

    public bool set;

    void OnValidate()
    {
        if (set) {
            set = false;
            SetGrid();
        }
    }

    void SetGrid()
    {
        gridSize = 1 << subs;

        pointList.Clear();
        pointDict.Clear();

        indexMap = new int[gridSize*gridSize*gridSize];

        for (int z = 0; z < gridSize; z++) 
        {
            for (int y = 0; y < gridSize; y++) 
            {
                for (int x = 0; x < gridSize; x++) 
                {
                    Vector3 pos = new Vector3(x,y,z);
                    Vector3 posNorm = pos.normalized * gridSize;

                    Vector3Int roundPosNorm = Vector3Int.RoundToInt(posNorm);

                    if (Vector3Int.RoundToInt(pos) == roundPosNorm)
                    {
                        pointDict.Add(roundPosNorm,pointList.Count);
                        pointList.Add(roundPosNorm);
                    }

                    //indexMap[GetIndex(roundPos)] = GetIndex(roundPosNorm);
                }
            }
        }

        indexLength = pointList.Count;

        for (int z = 0; z < gridSize; z++) 
        {
            for (int y = 0; y < gridSize; y++) 
            {
                for (int x = 0; x < gridSize; x++) 
                {
                    Vector3 pos = new Vector3(x,y,z);
                    Vector3 posNorm = pos.normalized * gridSize;

                    Vector3Int roundPos = Vector3Int.RoundToInt(pos);
                    Vector3Int roundPosNorm = Vector3Int.RoundToInt(posNorm);

                    indexMap[GetIndex(roundPos)] = pointDict[roundPosNorm];
                }
            }
        }

        int GetIndex(Vector3Int p) {
            return p.x + p.y*gridSize + p.z*gridSize*gridSize;
        }
    }
}
