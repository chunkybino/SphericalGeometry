using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class Octonion
{
    public float x;
    public float y;
    public float z;
    public float w;

    public float i;
    public float j;
    public float k;
    public float r;

    public Octonion(float x = 0, float y = 0, float z = 0, float w = 0, float i = 0, float j = 0, float k = 0, float r = 1)
    {
        this.x = x;
        this.y = y;
        this.z = z;
        this.w = w;
        this.i = i;
        this.j = j;
        this.k = k;
        this.r = r;
    }

    public float this[int index]
    {
        get {
            switch (index)
            {
                case 0:
                    return x;
                case 1:
                    return y;
                case 2:
                    return z;
                case 3:
                    return w;
                case 4:
                    return i;
                case 5:
                    return j;
                case 6:
                    return k;
                case 7:
                    return r;
            }
            return 0;
        }

        set {
            switch (index)
            {
                case 0:
                    x = value;
                    break;
                case 1:
                    y = value;
                    break;
                case 2:
                    z = value;
                    break;
                case 3:
                    w = value;
                    break;
                case 4:
                    i = value;
                    break;
                case 5:
                    j = value;
                    break;
                case 6:
                    k = value;
                    break;
                case 7:
                    r = value;
                    break;
            }
        }
    }

    public Octonion conjugate {get{
        return new Octonion(-x,-y,-z,-w,-i,-j,-k,r);
    }}

    static int[,] multTable =
    {
        {-8, -7,  6, -5,    4,  3, -2,  1},
        { 7, -8, -5, -6,   -3,  4,  1,  2},
        {-6,  5, -8, -7,    2, -1,  4,  3},
        { 5,  6,  7, -8,   -1, -2, -3,  4},

        {-4, -3,  2,  1,   -8, -7,  6,  5},
        { 3, -4, -1,  2,    7, -8, -5,  6},
        {-2,  1, -4,  3,   -6,  5, -8,  7},
        { 1,  2,  3,  4,    5,  6,  7,  8}
    };

    public static Dictionary<string, int> StringToIntType = new Dictionary<string, int>
    {
        {"x",1},
        {"y",2},
        {"z",3},
        {"w",4},
        {"i",5},
        {"j",6},
        {"k",7},
        {"1",8},

        {"-x",-1},
        {"-y",-2},
        {"-z",-3},
        {"-w",-4},
        {"-i",-5},
        {"-j",-6},
        {"-k",-7},
        {"-1",-8}
    };

    public static Dictionary<int, string> IntToStringType = new Dictionary<int, string>
    {
        {1,"x"},
        {2,"y"},
        {3,"z"},
        {4,"w"},
        {5,"i"},
        {6,"j"},
        {7,"k"},
        {8,"1"},

        {-1,"-x"},
        {-2,"-y"},
        {-3,"-z"},
        {-4,"-w"},
        {-5,"-i"},
        {-6,"-j"},
        {-7,"-k"},
        {-8,"-1"}
    };

    public static Octonion operator *(Octonion a, Octonion b)
    {
        Octonion c = new Octonion(0,0,0,0,0,0,0,0);

        for (int bi = 0; bi < 8; bi++)
        {
            for (int ai = 0; ai < 8; ai++)
            {
                int type = multTable[ai,bi];
                int index = Mathf.Abs(type)-1;
                float product = a[ai] * b[bi] * Mathf.Sign(type);
                c[index] += product;
            }
        }

        return c;
    }

    public static int[] TypeChartMult(int[] points, int mult)
    {
        int[] newPoints = new int[points.Length];

        for (int I = 0; I < points.Length; I++)
        {
            newPoints[I] = multTable[Mathf.Abs(mult)-1, Mathf.Abs(points[I])-1] * (int)Mathf.Sign(points[I]) * (int)Mathf.Sign(mult);
        }

        return newPoints;
    }

    public static string[] TypeChartMult(string[] points, string mult, bool right = false)
    {
        if (!StringToIntType.ContainsKey(mult)) return points;

        int intMult = StringToIntType[mult];

        string[] newPoints = new string[points.Length];

        for (int I = 0; I < points.Length; I++)
        {
            int realMult = multTable[Mathf.Abs(intMult)-1, Mathf.Abs(StringToIntType[points[I]])-1];
            if (right) realMult = multTable[Mathf.Abs(StringToIntType[points[I]])-1, Mathf.Abs(intMult)-1];
            newPoints[I] = IntToStringType[realMult * (int)Mathf.Sign(StringToIntType[points[I]]) * (int)Mathf.Sign(intMult)];
        }

        return newPoints;
    }
}
