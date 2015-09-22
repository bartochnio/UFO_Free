using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;

public class Track : MonoBehaviour {
	static public bool lauchFromEditorHack = false;
	static public string levelToLoadHack = null;

    public GameObject finishLinePrefab;

    BezierSpline spline;
    GameObject[] meshGOs;
    Material material;

	void Start () 
    {
		spline = GetComponent<BezierSpline>();

		LoadLevel (levelToLoadHack);

        GenerateMesh();
        GenerateCollider();

        CreateFinishLine();
    }

	void OnGUI () {
		if (lauchFromEditorHack) {
			if (GUI.Button (new Rect (Screen.width - 150, 0, 150, 30), "Editor")) {
				Application.LoadLevel ("LevelEditor");
			}
		}
	}

    void CreateFinishLine()
    {
        float t = 0.5f;
        int curveIndex = 0;

        GameObject go = Instantiate(finishLinePrefab);
        go.transform.position = spline.GetCurvePoint(curveIndex, t) + new Vector3(0.0f, 0.0f, -3.0f);

        Vector3 tangent = spline.GetCurveVelocity(curveIndex, t);
        go.transform.up = tangent.normalized;
    }

	bool LoadLevel (string levelName) {
		if (levelName == null)
			return false;

		string filename = levelName + ".ufo";
		string sepa = "" + System.IO.Path.DirectorySeparatorChar;

		List<string> paths = new List<string> ();
		paths.Add (Application.persistentDataPath); // persistent data path

		if (Application.platform == RuntimePlatform.IPhonePlayer) {
			paths.Add (Application.dataPath.Replace (Application.productName + ".app/Data", "/Documents/"));
		}

		// look in persistent data path
		//
		foreach (string path in paths) {
			string fullpath = path + sepa + filename;
			if (System.IO.File.Exists (fullpath))
			{
				string fileContents = System.IO.File.ReadAllText (fullpath);

				XDocument doc = XDocument.Parse (fileContents);
				if (doc == null) return false;
				if (doc.Root == null) return false;
				
				XElement splineElem = doc.Root.Element("spline");
				if (splineElem == null) return false;
				
				spline.Loop = false;
				if (!spline.LoadFromXmlElement(splineElem)) return false;

				return true;
			}
		}

		return false;
	}

    void Update() 
    {
	
	}

    void ChangeCurveAlpha(int index, float alpha)
    {
        Color color = Color.white;
        color.a = alpha;
        meshGOs[index].GetComponent<MeshRenderer>().material.SetColor("_Color", color);
		//meshGOs[index].GetComponent<MeshRenderer>().material.color = color;
    }

    void GenerateMesh()
    {
        material = GetComponent<MeshRenderer>().material;

        meshGOs = new GameObject[spline.CurveCount];

        MeshBuilder builder = new MeshBuilder();
        for (int i = 0; i < spline.CurveCount; ++i)
        {
            meshGOs[i] = new GameObject(i.ToString());
            meshGOs[i].transform.SetParent(transform);

            GenerateCurveMesh(spline, i, builder);

            Mesh mesh = builder.CreateMesh();
			MeshFilter filter = meshGOs[i].AddComponent<MeshFilter>();
			filter.sharedMesh = mesh;

			MeshRenderer meshRenderer = meshGOs[i].AddComponent<MeshRenderer>();
			meshRenderer.material = material;

            meshGOs[i].tag = "Track";

            builder.Clear();
        }
    }

    public static void GenerateCurveMesh(BezierSpline spline, int index, MeshBuilder builder)
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

            builder.PushVert((pos - lenDir * spline.Width));
            builder.PushNormal(normal);
            builder.PushUV(new Vector2(0.0f, v));

            builder.PushVert((pos + lenDir * spline.Width));
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

    //Messages
    void EnableCurve(int index)
    {
        //meshGOs[index].GetComponent<MeshRenderer>().enabled = true;

        ChangeCurveAlpha(index, 1.0f);
    }

    void DisableCurve(int index)
    {
        //meshGOs[index].GetComponent<MeshRenderer>().enabled = false;

        ChangeCurveAlpha(index, 0.2f);
    }
    
    void DisableTrack()
    {
        for (int i = 0; i < spline.CurveCount; ++i)
        {
            DisableCurve(i);
            //meshGOs[i].GetComponent<MeshRenderer>().enabled = false;
        }
    }
}
