using UnityEngine;
using System.Collections;

public class SegmentMeshGenerator : MonoBehaviour
{
    public int numRotations = 8;

    public int numPoints = 0;
    private int maxNumPoints = 65536 / 4;

    // raw points of the curve
    Vector3[] points;

    // mesh
    Vector3[] vertices;
    Vector2[] uvs;

    void Start ()
    {
        points = new Vector3[maxNumPoints];

        InitMesh();
        GenerateRotations();

        // DEBUG
        InitDebugPoints();
        UpdateVertices();
    }
	
	void Update ()
    {
	
	}

    void InitDebugPoints()
    {
        int numDebugPoints = 200;

        for( int i = 0; i < numDebugPoints; i+=1 )
        {
            float angle = i * 0.5f;
            float x = angle;//
            float y = Mathf.Cos(angle);
            float z = 0.0f;

            points[i] = new Vector3(x, y, z);
        }

        numPoints += numDebugPoints;
    }

    // call update only if points chaneg
    void UpdateVertices()
    {
        for (int i = 0; i < numPoints; i += 1)
        {
            var pointPos = points[i];

            int vertOffset = i * 2;
            vertices[vertOffset + 0] = pointPos;
            vertices[vertOffset + 1] = pointPos;
        }

        var meshFilter = GetComponent<MeshFilter>();
        meshFilter.sharedMesh.vertices = vertices;

        //mesh.RecalculateNormals();
    }

    void InitMesh()
    {
        var meshFilter = GetComponent<MeshFilter>();
        var mesh = new Mesh();
        meshFilter.mesh = mesh;

        int numVerts = maxNumPoints * 2;

        // init verts
        vertices = new Vector3[numVerts];
        for (int i = 0; i < numVerts; i+=1 )
        {
            vertices[i] = Vector3.zero;
        }

        mesh.vertices = vertices;

        // faces
        var tris = new int[ (maxNumPoints - 1)* 6 ];
        for ( int i = 0; i < maxNumPoints - 1; i+=1 )
        {
            int triOffset = i * 6;
            int vertOffset = i * 2;

            tris[triOffset + 0] = vertOffset + 0;
            tris[triOffset + 1] = vertOffset + 2;
            tris[triOffset + 2] = vertOffset + 3;
            tris[triOffset + 3] = vertOffset + 0;
            tris[triOffset + 4] = vertOffset + 3;
            tris[triOffset + 5] = vertOffset + 1;

        }
        mesh.triangles = tris;

        // uvs
        uvs = new Vector2[numVerts];
        for (int i = 0; i < maxNumPoints; i += 1)
        {
            uvs[i * 2 + 0] = new Vector2(0.0f, -1.0f);
            uvs[i * 2 + 1] = new Vector2(0.0f, +1.0f);
        }
        mesh.uv = uvs;
        //mesh.RecalculateNormals();

        mesh.MarkDynamic();
    }

    void GenerateRotations()
    {
        float deltaAngle = Mathf.PI * 2.0f / numRotations;

        var sharedMesh = this.GetComponent<MeshFilter>().sharedMesh;
        var sharedMaterial = this.GetComponent<MeshRenderer>().sharedMaterial;

        for ( int i = 0; i < numRotations; i+=1 )
        {
            float angle = deltaAngle * i;

            // create layer
            var layer = new GameObject("Layer_" + i);
            layer.transform.parent = this.transform;

            // set transform
            layer.transform.Rotate(transform.forward, angle * Mathf.Rad2Deg);

            var layerMeshFilter = layer.AddComponent<MeshFilter>();
            layerMeshFilter.sharedMesh = sharedMesh;

            var layerMeshRenderer = layer.AddComponent<MeshRenderer>();
            layerMeshRenderer.sharedMaterial = sharedMaterial;
        }
    }

}
