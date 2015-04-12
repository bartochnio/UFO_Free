﻿using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class BezierSpline : MonoBehaviour {

    [SerializeField]
    private Vector2[] points;

    [SerializeField]
    private bool loop;

    [SerializeField]
    private float meshWidth = 1.0f;

    private struct curveData
    {
        public Bounds bounds;
        public int curveID;
    };

    private curveData[] curveBounds;

    public bool Loop
    {
        get { return loop; }
        set
        {
            loop = value;
            if (value)
            {
                SetControlPoint(0, points[0]);
            }
        }
    }

    public float Width
    {
        get { return meshWidth; }
        set
        {
            meshWidth = value;
        }
    }

    public int ControlPointCount
    {
        get { return points.Length; }
    }

    public Vector2 GetControlPoint(int index)
    {
        return points[index];
    }

    //TODO: change the ref argument
    public Vector2 GetClosestPointToACurve(int idx, Vector2 p, ref float t)
    {
        int steps = 8;
        Vector2 closest = new Vector2(Mathf.Infinity, Mathf.Infinity);
        Vector2 lineStart = GetCurvePoint(idx, 0.0f);
        for (int i = 1; i <= steps; ++i)
        {
            t = i / (float)steps;
            Vector2 lineEnd = GetCurvePoint(idx, t);

            //Debug.DrawLine(lineStart, lineEnd, Color.cyan);

            Vector2 c = Utils.ClosestPoint(lineStart, lineEnd, p);
            if (Vector2.Distance(p, c) <= Vector2.Distance(p, closest))
            {
                closest = c;
            }

            lineStart = lineEnd;
        }

        return closest;
    }

    public Vector2 GetClosestPoint(Vector2 p)
    {
        float t = 0.0f;

        Array.Sort(curveBounds, (a, b) => a.bounds.SqrDistance(p) < b.bounds.SqrDistance(p) ? -1 : 1);
        Vector2 closest = GetClosestPointToACurve(curveBounds[0].curveID, p, ref t);
        for (int i = 1; i < 3; ++i)
        {
            Vector2 c = GetClosestPointToACurve(curveBounds[i].curveID, p, ref t);
            if (Vector2.Distance(p, c) <= Vector2.Distance(p, closest))
                closest = c;
        }

        return closest;
    }

    public void SetControlPoint(int index, Vector2 point)
    {
        if (index % 3 == 0)
        {
            Vector2 delta = point - points[index];
            if (loop)
            {
                if (index == 0)
                {
                    points[1] += delta;
                    points[points.Length - 2] += delta;
                    points[points.Length - 1] = point;
                }
                else if(index == points.Length - 1)
                {
                    points[0] = point;
                    points[1] += delta;
                    points[index - 1] += delta;
                }
                else
                {
                    points[index - 1] += delta;
                    points[index + 1] += delta;
                }
            }
            else
            {
                if (index > 0)
                    points[index - 1] += delta;
                if (index + 1 < points.Length)
                    points[index + 1] += delta;
            }
        }

        points[index] = point;
        EnforceMode(index);
    }

    private void EnforceMode(int index)
    {
        int modeIndex = (index + 1) / 3;

        if (!loop && (modeIndex == 0 || index >= points.Length - 2))
            return;

        int middleIndex = modeIndex * 3;
        int fixedIndex, enforcedIndex;
        if (index <= middleIndex)
        {
            fixedIndex = middleIndex - 1;
            if (fixedIndex < 0)
                fixedIndex = points.Length - 2;

            enforcedIndex = middleIndex + 1;
            if (enforcedIndex >= points.Length)
                enforcedIndex = 1;
        }
        else
        {
            fixedIndex = middleIndex + 1;
            if (fixedIndex >= points.Length)
                fixedIndex = 1;

            enforcedIndex = middleIndex - 1;
            if (enforcedIndex < 0)
                enforcedIndex = points.Length - 2;
        }

        Vector2 middle = points[middleIndex];
        Vector2 enforcedTangent = middle - points[fixedIndex];
        points[enforcedIndex] = middle + enforcedTangent;
    }

    public int CurveCount
    {
        get { return (points.Length - 1) / 3; }
    }

    public Vector2 GetPoint(float t)
    {
        int i;
        if (t >= 1.0f)
        {
            t = 1.0f;
            i = points.Length - 4;
        }
        else
        {
            t = Mathf.Clamp01(t) * CurveCount;
            i = (int)t;
            t -= i;
            i *= 3;
        }

        return transform.TransformPoint(Bezier.GetPoint(points[i], points[i + 1], points[i + 2], points[i + 3], t));
    }

    public Vector2 GetCurvePoint(int index, float t)
    {
        int idx = index * 3;
        Vector2 p0 = points[idx];
        Vector2 p1 = points[idx + 1];
        Vector2 p2 = points[idx + 2];
        Vector2 p3 = points[idx + 3];

        return transform.TransformPoint(Bezier.GetPoint(p0, p1, p2, p3, t));
    }

    public Vector2 GetCurveVelocity(int index, float t)
    {
        int idx = index * 3;
        Vector2 p0 = points[idx];
        Vector2 p1 = points[idx + 1];
        Vector2 p2 = points[idx + 2];
        Vector2 p3 = points[idx + 3];

        return  transform.TransformPoint(Bezier.GetTangent(p0, p1, p2, p3, t)) - transform.position;
    }

    public Vector3 GetVelocity(float t)
    {
        int i;
        if (t >= 1f)
        {
            t = 1f;
            i = points.Length - 4;
        }
        else
        {
            t = Mathf.Clamp01(t) * CurveCount;
            i = (int)t;
            t -= i;
            i *= 3;
        }
        return transform.TransformPoint(Bezier.GetTangent(points[i], points[i + 1], points[i + 2], points[i + 3], t)) - transform.position;
    }

    public void AddCurve()
    {
        Vector2 point = points[points.Length - 1];
        Array.Resize(ref points, points.Length + 3);

        point.x += 4.0f;
        points[points.Length - 3] = point;
        point.x += 4.0f;
        points[points.Length - 2] = point;
        point.x += 4.0f;
        points[points.Length - 1] = point;

        EnforceMode(points.Length - 4);

        if (loop)
        {
            points[points.Length - 1] = points[0];
            EnforceMode(0);
        }
    }

    public void Reset()
    {
        points = new Vector2[]
        {
            new Vector2(1.0f,0.0f),
            new Vector2(5.0f,0.0f),
            new Vector2(9.0f,0.0f),
            new Vector2(13.0f,0.0f),
        };
    }

    public float GetCurveLength(int index, int lineStep)
    {
        int idx = index * 3;
        Vector2 p0 = points[idx];
        Vector2 p1 = points[idx + 1];
        Vector2 p2 = points[idx + 2];
        Vector2 p3 = points[idx + 3];

        float result = 0.0f;

        Vector2 lineStart = transform.TransformPoint(p0);
        for (int i = 1; i <= lineStep; ++i)
        {
            Vector2 lineEnd = transform.TransformPoint(Bezier.GetPoint(p0, p1, p2, p3, i / (float)lineStep));
            result += Vector2.Distance(lineStart, lineEnd);
            lineStart = lineEnd;
        }

        return result;
    }

    void Start()
    {
        GenerateBounds();
    }

    void GenerateBounds()
    {
        curveBounds = new curveData[CurveCount];

        for(int i = 0; i < CurveCount; ++i)
        {
            int idx = i * 3;
            curveBounds[i].curveID = i;
            curveBounds[i].bounds.center = points[idx];
            curveBounds[i].bounds.Encapsulate(points[idx + 1]);
            curveBounds[i].bounds.Encapsulate(points[idx + 2]);
            curveBounds[i].bounds.Encapsulate(points[idx + 3]);
        }
    }
}
