using UnityEngine;
using System.Collections;
using System;

public class SegmentMeshGenerator : MonoBehaviour
{
    public int numRotations = 8;

    public int numPoints = 0;
    private int maxNumPoints = 65536 / 4;

    // raw points of the curve
    Vector3[] points;

    // mesh
    Vector3[] vertices;
    Vector3[] verticesPrev;
    Vector4[] verticesNext;
    Vector2[] uvs;
    int[] tris;

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
        int numDebugPoints = 20;

        for( int i = 0; i < numDebugPoints; i+=1 )
        {
            float angle = i * 0.3f;
            float x = 1.0f + angle;//
            float y = Mathf.Cos(angle);
            float z = 0.0f;

            points[i] = new Vector3(x, y, z);
        }

        numPoints += numDebugPoints;
    }

    // call update only if points chaneg
    void UpdateVertices()
    {
        // don't do anything until there are three points
        if (numPoints < 3)
            return;

        Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);

        for (int i = 0; i < numPoints; i += 1)
        {
            // indices
            int pointIndexPrev = Mathf.Max(0, i - 1);
            int pointIndexCurr = i;
            int pointIndexNext = Mathf.Min(numPoints - 1, i + 1);

            // pos
            var pointPosPrev = points[pointIndexPrev];
            var pointPosCurr = points[pointIndexCurr];
            var pointPosNext = points[pointIndexNext];

            // offset
            int vertOffsetCurr = pointIndexCurr * 2;

            // set verts
            vertices[vertOffsetCurr + 0] = pointPosCurr;
            vertices[vertOffsetCurr + 1] = pointPosCurr;
            verticesPrev[vertOffsetCurr + 0] = pointPosPrev;
            verticesPrev[vertOffsetCurr + 1] = pointPosPrev;
            verticesNext[vertOffsetCurr + 0] = pointPosNext;
            verticesNext[vertOffsetCurr + 1] = pointPosNext;

            // tris
            int triOffset = pointIndexCurr * 6;
            tris[triOffset + 0] = vertOffsetCurr + 0;
            tris[triOffset + 1] = vertOffsetCurr + 3;
            tris[triOffset + 2] = vertOffsetCurr + 2;
            tris[triOffset + 3] = vertOffsetCurr + 0;
            tris[triOffset + 4] = vertOffsetCurr + 1;
            tris[triOffset + 5] = vertOffsetCurr + 3;

            // add point to bounds
            bounds.Encapsulate(pointPosCurr);
        }

        // handle first previous point (because has no previous)
        var firstPointPrev = points[0] * 2.0f - points[1];
        verticesPrev[0] = firstPointPrev;
        verticesPrev[1] = firstPointPrev;

        // handle last point's next vert (because has no next)
        var lastPointNext = points[numPoints - 1] * 2 - points[numPoints - 2];
        verticesNext[(numPoints - 1) * 2 + 0] = lastPointNext;
        verticesNext[(numPoints - 1) * 2 + 1] = lastPointNext;

        // last triangle set should be zerod out
        int triOffsetFinal = (numPoints-1) * 6;
        tris[triOffsetFinal + 0] = 0;
        tris[triOffsetFinal + 1] = 0;
        tris[triOffsetFinal + 2] = 0;
        tris[triOffsetFinal + 3] = 0;
        tris[triOffsetFinal + 4] = 0;
        tris[triOffsetFinal + 5] = 0;

        var meshFilter = GetComponent<MeshFilter>();
        var mesh = meshFilter.sharedMesh;

        // set mesh properties
        mesh.vertices = vertices;
        mesh.normals = verticesPrev;
        mesh.tangents = verticesNext;
        mesh.triangles = tris;
        mesh.bounds = bounds;
    }

    void InitMesh()
    {
        var meshFilter = GetComponent<MeshFilter>();
        var mesh = new Mesh();
        meshFilter.mesh = mesh;

        int numVerts = maxNumPoints * 2;

        // init verts
        vertices = new Vector3[numVerts];
        verticesPrev = new Vector3[numVerts];
        verticesNext = new Vector4[numVerts];
        tris = new int[(maxNumPoints - 1) * 6];
        uvs = new Vector2[numVerts];

        // verts
        for (int i = 0; i < numVerts; i+=1 )
        {
            vertices[i] = Vector3.zero;
            verticesPrev[i] = Vector3.zero;
            verticesNext[i] = Vector4.zero;
        }

        // faces
        for( int i = 0; i < tris.Length; ++i )
        {
            tris[i] = 0;
        }

        // uvs
        for (int i = 0; i < maxNumPoints; i += 1)
        {
            uvs[i * 2 + 0] = new Vector2(0.0f, -1.0f);
            uvs[i * 2 + 1] = new Vector2(0.0f, +1.0f);
        }

        mesh.vertices = vertices;
        mesh.normals = verticesPrev;
        mesh.tangents = verticesNext;
        mesh.triangles = tris;
        mesh.uv = uvs;

        mesh.MarkDynamic();
    }

    void GenerateRotations()
    {
        float deltaAngle = Mathf.PI * 2.0f / numRotations;

        var sharedMesh = this.GetComponent<MeshFilter>().sharedMesh;
        var sharedMaterial = this.GetComponent<MeshRenderer>().sharedMaterial;

        for ( int i = 1; i < numRotations; i+=1 )
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
