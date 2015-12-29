using UnityEngine;
using System.Collections;

public class SegmentMeshGenerator : MonoBehaviour
{
	void Start ()
    {
        InitDebugMesh();
	}
	
	void Update ()
    {
	
	}

    void InitDebugMesh()
    {
        var meshFilter = GetComponent<MeshFilter>();
        var mesh = new Mesh();
        meshFilter.mesh = mesh;

        var vertices = new Vector3[3];
        vertices[0] = new Vector3(1.0f, 0.0f, 0.0f);
        vertices[1] = new Vector3(2.0f, 0.0f, 0.0f);
        vertices[2] = new Vector3(2.0f, 2.0f, 0.0f);
        mesh.vertices = vertices;

        // faces
        var tris = new int[3];
        tris[0] = 0;
        tris[1] = 2;
        tris[2] = 1;
        mesh.triangles = tris;

        mesh.RecalculateNormals();
    }
}
