using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using System.Text;

public class BezierSpline : MonoBehaviour {

    [SerializeField]
    private Vector3[] points;

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

    public Vector3 GetControlPoint(int index)
    {
        return points[index];
    }

	public void DeletePoints (int start, int count) {
		Vector3[] newPoints = new Vector3[points.Length - 3];

		for (int k = 0; k < start; ++k) {
			newPoints [k] = points [k];
		}

		for (int k = start + 3; k < points.Length; ++k) {
			newPoints [k - 3] = points [k];
		}

		points = newPoints;
	}

	public void InsertPointsAt (int start, Vector3[] pts) {
		if (pts.Length == 0)
			return;

		Vector3[] newPoints = new Vector3 [points.Length + pts.Length];
		int j = 0;

		for (int k = 0; k < start; ++k) {
			newPoints [j++] = points [k];
		}

		for (int k = 0; k < pts.Length; ++k) {
			newPoints [j++] = pts [k];
		}

		for (int k = start; k < points.Length; ++k) {
			newPoints [j++] = points [k];
		}

		points = newPoints;
	}

    //TODO: change the ref argument
    public Vector3 GetClosestPointToACurve(int idx, Vector3 p, ref float t)
    {
        int steps = 8;
        Vector3 closest = new Vector3(Mathf.Infinity, Mathf.Infinity);
        Vector3 lineStart = GetCurvePoint(idx, 0.0f);
        for (int i = 1; i <= steps; ++i)
        {
            float ct = i / (float)steps;
            Vector3 lineEnd = GetCurvePoint(idx, ct);

           // Debug.DrawLine(lineStart, lineEnd, Color.cyan);

            Vector3 c = Utils.ClosestPoint(lineStart, lineEnd, p);
            if (Vector3.Distance(p, c) <= Vector3.Distance(p, closest))
            {
                closest = c;
                t = ct;
            }

            lineStart = lineEnd;
        }

        return closest;
    }

    public Vector3 GetClosestPoint(Vector3 p)
    {
        float t = 0.0f;

        Array.Sort(curveBounds, (a, b) => a.bounds.SqrDistance(p) < b.bounds.SqrDistance(p) ? -1 : 1);
        Vector3 closest = GetClosestPointToACurve(curveBounds[0].curveID, p, ref t);
        for (int i = 1; i < 3; ++i)
        {
            Vector3 c = GetClosestPointToACurve(curveBounds[i].curveID, p, ref t);
            if (Vector3.Distance(p, c) <= Vector3.Distance(p, closest))
                closest = c;
        }

        return closest;
    }

    public void SetControlPoint(int index, Vector3 point)
    {
        if (index % 3 == 0)
        {
            Vector3 delta = point - points[index];
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

        Vector3 middle = points[middleIndex];
        Vector3 enforcedTangent = middle - points[fixedIndex];
        points[enforcedIndex] = middle + enforcedTangent;
    }

    public int CurveCount
    {
        get { return (points.Length - 1) / 3; }
    }

    public Vector3 GetPoint(float t)
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

    public Vector3 GetPoint(float t, ref int idx)
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

        idx = i / 3;
        return transform.TransformPoint(Bezier.GetPoint(points[i], points[i + 1], points[i + 2], points[i + 3], t));
    }

    public Vector3 GetCurvePoint(int index, float t)
    {
        int idx = index * 3;
        Vector3 p0 = points[idx];
        Vector3 p1 = points[idx + 1];
        Vector3 p2 = points[idx + 2];
        Vector3 p3 = points[idx + 3];

        return transform.TransformPoint(Bezier.GetPoint(p0, p1, p2, p3, t));
    }

    public Vector3 GetCurveVelocity(int index, float t)
    {
        int idx = index * 3;
        Vector3 p0 = points[idx];
        Vector3 p1 = points[idx + 1];
        Vector3 p2 = points[idx + 2];
        Vector3 p3 = points[idx + 3];

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
        Vector3 point = points[points.Length - 1];
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
        points = new Vector3[]
        {
            new Vector3(1.0f,0.0f,0.0f),
            new Vector3(5.0f,0.0f,0.0f),
            new Vector3(9.0f,0.0f,0.0f),
            new Vector3(13.0f,0.0f,0.0f),
        };
    }

    public float GetCurveLength(int index, int lineStep)
    {
        int idx = index * 3;
        Vector3 p0 = points[idx];
        Vector3 p1 = points[idx + 1];
        Vector3 p2 = points[idx + 2];
        Vector3 p3 = points[idx + 3];

        float result = 0.0f;

        Vector3 lineStart = transform.TransformPoint(p0);
        for (int i = 1; i <= lineStep; ++i)
        {
            Vector3 lineEnd = transform.TransformPoint(Bezier.GetPoint(p0, p1, p2, p3, i / (float)lineStep));
            result += Vector3.Distance(lineStart, lineEnd);
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


	static char[] PointsSepa = new char[] {' '};

	public void WriteXml(XmlWriter writer)
	{
		writer.WriteStartElement("spline");
		{
			writer.WriteStartElement ("points");
			{
				writer.WriteAttributeString ("count", points.Length.ToString ());

				StringBuilder pointsStr = new StringBuilder ();
				for (int p = 0; p < points.Length - 1; ++p) {
					Vector3 pt = points [p];
					pointsStr.Append (pt.x.ToString ());
					pointsStr.Append (PointsSepa);
					pointsStr.Append (pt.y.ToString ());
					pointsStr.Append (PointsSepa);
					pointsStr.Append (pt.z.ToString ());
					pointsStr.Append (PointsSepa);
				}
				if (points.Length > 0) {
					Vector3 pt = points [points.Length - 1];
					pointsStr.Append (pt.x.ToString ());
					pointsStr.Append (PointsSepa);
					pointsStr.Append (pt.y.ToString ());
					pointsStr.Append (PointsSepa);
					pointsStr.Append (pt.z.ToString ());
				}

				writer.WriteValue (pointsStr.ToString ());
			}
			writer.WriteEndElement ();

			writer.WriteStartElement ("props");
			{
				writer.WriteElementString ("loop", loop.ToString ());
				writer.WriteElementString ("width", meshWidth.ToString ());

			}
			writer.WriteEndElement ();
		}
		writer.WriteEndElement ();
	}


	public bool LoadFromXmlElement(XElement element)
	{
		XElement ptsElem = element.Element ("points");

		int count = 0;
		if (!int.TryParse (ptsElem.Attribute ("count").Value, out count))
			return false;

		string[] floats = ptsElem.Value.Split(PointsSepa);
		if (floats.Length / 3 != count)
			return false;

		points = new Vector3[count];

		for (int k = 0; k < count; ++k)
		{
			int i = 3 * k;
			points[k] = new Vector3(float.Parse(floats[i + 0]), float.Parse(floats[i + 1]), float.Parse(floats[i + 2]));
		}

		XElement propsElem = element.Element("props");
		if (propsElem != null)
		{
			XElement loopElem = propsElem.Element("loop");
			if (loopElem != null)
			{
				Loop = bool.Parse(loopElem.Value);
			}
			else Loop = false;

			XElement widthElem = propsElem.Element ("width");
			if (widthElem != null)
			{
				meshWidth = float.Parse(widthElem.Value);
			}
			else meshWidth = 1.0f;
		}
		else {
			Loop = false;
			meshWidth = 1.0f;
		}

		GenerateBounds();
		return true;
	}

	public void Reverse () {
		Array.Reverse (points);
	}
}
