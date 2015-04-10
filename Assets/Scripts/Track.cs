using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Track : MonoBehaviour {

    BezierSpline spline;
    GameObject[] meshGOs;
    Material material;

	void Start () 
    {
        spline = GetComponent<BezierSpline>();
        GenerateMesh();
        GenerateCollider();
    }

    void Update() 
    {
	
	}

    void GenerateMesh()
    {
        material = GetComponent<MeshRenderer>().material;

        meshGOs = new GameObject[spline.CurveCount];

        MeshBuilder builder = new MeshBuilder();
        for (int i = 0; i < spline.CurveCount; ++i)
        {
            meshGOs[i] = new GameObject("Curve_" + i);
            meshGOs[i].transform.SetParent(transform);
            MeshRenderer meshRenderer = meshGOs[i].AddComponent<MeshRenderer>();
            MeshFilter filter = meshGOs[i].AddComponent<MeshFilter>();

            GenerateCurveMesh(i, builder);

            Mesh mesh = builder.CreateMesh();
            filter.sharedMesh = mesh;
            meshRenderer.material = material;

            meshGOs[i].tag = "Track";

            builder.Clear();
        }
    }


    void ChangeCurveAlpha(int index, float alpha)
    {
        Color color = Color.red;
        color.a = alpha;
        meshGOs[index].GetComponent<MeshRenderer>().material.SetColor("_Color", color);
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
        List<Vector2> points = new List<Vector2>();
        for(int i = 0; i < spline.CurveCount; ++i)
        {
            MeshFilter filter = meshGOs[i].GetComponent<MeshFilter>();
            PolygonCollider2D collider = meshGOs[i].AddComponent<PolygonCollider2D>() as PolygonCollider2D;
            Vector3[] verts = filter.sharedMesh.vertices;

            for (int j = 0; j < verts.Length; ++j)
            {
                if (j % 2 == 0)
                    points.Add(verts[j]);
            }

            for (int j = verts.Length - 1; j > 0; --j)
            {
                if (j % 2 != 0)
                    points.Add(verts[j]);
            }

            collider.points = points.ToArray();
            points.Clear();
        }
    }
}
