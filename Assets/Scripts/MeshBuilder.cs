using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MeshBuilder
{
    public List<Vector3> vertices = new List<Vector3>();
    public List<Vector2> uvs = new List<Vector2>();
    public List<Vector2> uvs2 = new List<Vector2>();
    public List<Vector3> normals = new List<Vector3>();
    public List<int> indices = new List<int>();

    public List<Color> colors = new List<Color>();

    public void PushVert(Vector3 v)
    {
        vertices.Add(v);
    }

    public void PushNormal(Vector3 n)
    {
        normals.Add(n);
    }

    public void PushUV(Vector2 uv)
    {
        uvs.Add(uv);
    }

    public void PushUV2(Vector2 uv)
    {
        uvs2.Add(uv);
    }

    public void PushColor(Color c)
    {
        colors.Add(c);
    }

    public void AddTriangle(int idx0, int idx1, int idx2)
    {
        indices.Add(idx0);
        indices.Add(idx1);
        indices.Add(idx2);
    }

    public Mesh CreateMesh()
    {
        Mesh mesh = new Mesh();

        mesh.vertices = vertices.ToArray();
        mesh.triangles = indices.ToArray();

        if (normals.Count == vertices.Count)
            mesh.normals = normals.ToArray();

        if (uvs.Count == vertices.Count)
            mesh.uv = uvs.ToArray();

        if (uvs2.Count == vertices.Count)
            mesh.uv2 = uvs2.ToArray();

        if (colors.Count == vertices.Count)
            mesh.colors = colors.ToArray();

        mesh.RecalculateBounds();
        return mesh;
    }

    public void Clear()
    {
        vertices.Clear();
        indices.Clear();
        uvs.Clear();
        uvs2.Clear();
        colors.Clear();
        normals.Clear();
    }
}
