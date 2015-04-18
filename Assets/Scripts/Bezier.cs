using UnityEngine;
using System.Collections;

public static class Bezier
{
    public static Vector3 GetPoint(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        t = Mathf.Clamp01(t);

        float t2 = t * t;
        float t3 = t2 * t;
        float mt = 1.0f - t;
        float mt2 = mt * mt;
        float mt3 = mt2 * mt;

        return p0 * mt3 + p1 * mt2 * t * 3.0f + p2 * mt * t2 * 3.0f + p3 * t3;
    }

    //WARNING: not normalized
    public static Vector3 GetTangent(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        t = Mathf.Clamp01(t);

        float t2 = t * t;
        float mt = 1.0f - t;
        float mt2 = mt * mt;
        float mid = 2.0f * t * mt;

        return p0 * (-mt2) + p1 * (mt2 - mid) + p2 * (-t2 + mid) + p3 * t2;
    }
    
}
