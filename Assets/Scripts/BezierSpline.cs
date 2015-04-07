using UnityEngine;
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
                GenerateMesh();
            }
        }
    }

    public float Width
    {
        get { return meshWidth; }
        set
        {
            meshWidth = value;
            GenerateMesh();
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
        GenerateMesh();
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

        GenerateMesh();
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

        GenerateMesh();
    }

    public void OnSpawn()
    {
        GenerateMesh();
    }

    public float GetCurveLength(int index, int lineStep)
    {
        int idx = index * 3;
        Vector2 p0 = points[idx];
        Vector2 p1 = points[idx + 1];
        Vector2 p2 = points[idx + 2];
        Vector2 p3 = points[idx + 3];

        float result = 0.0f;

        Vector2 lineStart = p0;
        for (int i = 1; i <= lineStep; ++i)
        {
            Vector2 lineEnd = Bezier.GetPoint(p0, p1, p2, p3, i / (float)lineStep);
            result += Vector2.Distance(lineStart, lineEnd);
            lineStart = lineEnd;
        }

        return result;
    }

    void Start()
    {
        GenerateBounds();
        GenerateCollider();
    }

    void GenerateCollider()
    {
        MeshFilter filter = GetComponent<MeshFilter>();
        PolygonCollider2D collider = gameObject.AddComponent<PolygonCollider2D>() as PolygonCollider2D;
        Vector3[] verts = filter.sharedMesh.vertices;
        List<Vector2> points = new List<Vector2>();

        for (int i = 0; i < verts.Length; ++i)
        {
            if (i % 2 == 0)
                points.Add(verts[i]);
        }

        for (int i = verts.Length - 1; i > 0; --i)
        {
            if (i % 2 != 0)
                points.Add(verts[i]);
        }

        collider.points = points.ToArray();
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

    void GenerateMesh()
    {
        MeshBuilder builder = new MeshBuilder();
        for (int i = 0; i < CurveCount; ++i)
        {
            GenerateCurveMesh(i, builder);
        }

        Mesh mesh = builder.CreateMesh();
        MeshFilter filter = GetComponent<MeshFilter>();
        if (filter != null)
        {
            filter.sharedMesh = mesh;
        }
    }

    void GenerateCurveMesh(int index, MeshBuilder builder)
    {
        float curveLen = GetCurveLength(index, 100);
        Vector3 prevPos = GetCurvePoint(index, 0.0f);
        int detail = 30;
        float v = 0.0f;
        Vector3 lenDir, widthDir;
        Vector3 normal = -Vector3.forward;

        for (int j = 0; j <= detail; ++j)
        {
            float progress = (float)j / (float)detail;
            Vector3 pos = GetCurvePoint(index, progress);
            widthDir = this.GetCurveVelocity(index, progress).normalized;
            lenDir = Vector3.Cross(widthDir, -Vector3.forward).normalized * 0.5f;

            v += Vector2.Distance(pos, prevPos) / curveLen;
            v = Mathf.Clamp01(v);

            builder.PushVert(transform.InverseTransformPoint(pos - lenDir * meshWidth));
            builder.PushNormal(normal);
            builder.PushUV(new Vector2(0.0f, v));

            builder.PushVert(transform.InverseTransformPoint(pos + lenDir * meshWidth));
            builder.PushNormal(normal);
            builder.PushUV(new Vector2(1.0f, v));

            prevPos = pos;
            if (j != 0)
            {
                int baseIndex = builder.vertices.Count - 4;
                builder.AddTriangle(baseIndex, baseIndex + 1, baseIndex + 2);
                builder.AddTriangle(baseIndex + 2, baseIndex + 1, baseIndex + 3);
            }
        }   
    }
}
