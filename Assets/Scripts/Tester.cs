using UnityEngine;
using System.Collections.Generic;
using System.Collections;

[ExecuteInEditMode]
public class Tester : MonoBehaviour
{
    public string[] OGpoints = {"x","y","z","w","i","j","k","1"};
    public string[] points = {"x","y","z","w","i","j","k","1"};

    [SerializeField] bool resetPoints;

    [SerializeField] bool multPoints;
    [SerializeField] string multPointsMult;
    [SerializeField] bool multPointsRight;

    public Octonion[] octs;

    public bool multOct;
    public Vector3Int octToMult; //x=left, y=right, z=result
    public bool multOctCon;

    public float[] magnitudes;

    // Update is called once per frame
    void Update()
    {
        if (resetPoints) {
            resetPoints = false;
            points = new string[] {"x","y","z","w","i","j","k","1"};
        }

        if (multPoints) {
            multPoints = false;
            points = Octonion.TypeChartMult(points, multPointsMult, multPointsRight);
        }

        if (multOct)
        {
            multOct = false;

            Octonion left = octs[octToMult.x];
            Octonion right = octs[octToMult.y];
            if (multOctCon) right = right.conjugate;

            octs[octToMult.z] = left * right;
        }

        magnitudes = new float[octs.Length];

        for (int i = 0; i < magnitudes.Length; i++)
        {
            Octonion o = octs[i];

            float sum = 0;
            for (int k = 0; k < 8; k++) {sum += S(k);}

            magnitudes[i] = Mathf.Sqrt(sum);

            float S(int j)
            {
                return o[j] * o[j];
            }
        }
    }
}
