using UnityEngine;
using System.Collections;

public static class Utils
{
    public static Vector2 ClosestPoint(Vector2 va, Vector2 vb, Vector2 p)
    {
        Vector2 v1 = p - va;
        Vector2 v2 = (vb - va).normalized;

        float t = Vector2.Dot(v1, v2);
        float d = Vector2.Distance(va, vb);

        if (t <= 0)
            return va;
        if (t >= d)
            return vb;

        return va + v2 * t;
    }
}