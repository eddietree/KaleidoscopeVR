using UnityEngine;
using System.Collections;

public class SegmentMeshGenerator : MonoBehaviour
{
    public int numRotations = 8;

	void Start ()
    {
        InitDebugMesh();
        GenerateRotations();
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

    void GenerateRotations()
    {
        float deltaAngle = Mathf.PI * 2.0f / numRotations;

        for( int i = 0; i < numRotations; i+=1 )
        {
            float angle = deltaAngle * i;

            // create layer
            var layer = new GameObject("Layer_" + i);
            layer.transform.parent = this.transform;

            // set transform
            layer.transform.Rotate(transform.forward, angle * Mathf.Rad2Deg);

            var layerMeshFilter = layer.AddComponent<MeshFilter>();
            layerMeshFilter.sharedMesh = this.GetComponent<MeshFilter>().sharedMesh;


            var layerMeshRenderer = layer.AddComponent<MeshRenderer>();
            layerMeshRenderer.sharedMaterial = GetComponent<MeshRenderer>().sharedMaterial;
        }
    }

}
