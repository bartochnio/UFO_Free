using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Track : MonoBehaviour {

    BezierSpline spline;

	void Start () 
    {
        spline = GetComponent<BezierSpline>();
        GenerateMesh();
        GenerateCollider();
	}
	
	void Update () 
    {
	
	}

    void GenerateMesh()
    {
        MeshBuilder builder = new MeshBuilder();
        for (int i = 0; i < spline.CurveCount; ++i)
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
        float curveLen = spline.GetCurveLength(index, 100);
        Vector3 prevPos = spline.GetCurvePoint(index, 0.0f);
        int detail = 30;
        float v = 0.0f;
        Vector3 lenDir, widthDir;
        Vector3 normal = -Vector3.forward;

        for (int j = 0; j <= detail; ++j)
        {
            float progress = (float)j / (float)detail;
            Vector3 pos = spline.GetCurvePoint(index, progress);
            widthDir = spline.GetCurveVelocity(index, progress).normalized;
            lenDir = Vector3.Cross(widthDir, -Vector3.forward).normalized * 0.5f;

            v += Vector2.Distance(pos, prevPos) / curveLen;
            v = Mathf.Clamp01(v);

            builder.PushVert(transform.InverseTransformPoint(pos - lenDir * spline.Width));
            builder.PushNormal(normal);
            builder.PushUV(new Vector2(0.0f, v));

            builder.PushVert(transform.InverseTransformPoint(pos + lenDir * spline.Width));
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
}
