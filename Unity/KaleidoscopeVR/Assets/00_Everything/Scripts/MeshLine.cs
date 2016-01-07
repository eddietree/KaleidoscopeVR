using UnityEngine;
using System.Collections;
using System;

public class MeshLine : MonoBehaviour
{
    public int numRotations = 8;

    public int numPoints = 0;
    public const int maxNumPoints = 42;

    // raw points of the curve
    public Vector3[] points;

    // drawing stuff
    public float lineThickness = 1.0f;

    // mesh data
    public Mesh mesh = null;
    Vector3[] vertices;
    Vector3[] verticesPrev;
    Vector4[] verticesNext;
    Vector2[] uvs;
    int[] tris;

    public void Init()
    {
        if (points == null )
            points = new Vector3[maxNumPoints];

        if (mesh == null)
            InitMesh();
    }

    public void Clear()
    {
        points = new Vector3[maxNumPoints];
        InitMesh();
    }

    public void AddPoint(Vector3 pt)
    {
        // cannot add anymore
        if (numPoints >= maxNumPoints)
            return;

        points[numPoints] = pt;
        numPoints++;
    }

    public void SetPoint(Vector3 pt, int index)
    {
        if (index < 0 || index >= numPoints)
            return;

        points[index] = pt;
    }

    public void AddPoints(Vector3[] pts)
    {
        for( int i = 0; i < pts.Length; ++i )
        {
            AddPoint(pts[i]);
        }
    }

    void Start()
    {
        Init();
    }

    public void UpdateVerticesRange( int pointIndexStart, int pointCount )
    {
        // don't do anything until there are three points
        if (numPoints < 3)
            return;

        // indices
        //pointIndexStart = Mathf.Clamp(pointIndexStart, 0, numPoints - 1);
        //int pointIndexEnd = Mathf.Min( numPoints-1, pointIndexStart + pointCount);

        var bounds = mesh.bounds;

        //print(pointIndexStart);

        for (int i = 0; i < pointCount; i += 1)
        {
            int indexBaseCurr = (i + numPoints + pointIndexStart) % numPoints;

            // indices
            int pointIndexPrev = (indexBaseCurr - 1 + numPoints) % numPoints;
            int pointIndexCurr = indexBaseCurr;
            int pointIndexNext = (indexBaseCurr + 1) % numPoints;

            // pos
            var pointPosPrev = points[pointIndexPrev];
            var pointPosCurr = points[pointIndexCurr];
            var pointPosNext = points[pointIndexNext];
            Vector4 pointPosNextV4 = new Vector4(pointPosNext.x, pointPosNext.y, pointPosNext.z, lineThickness);

            // offset
            int vertOffsetCurr = pointIndexCurr * 2;

            // set verts
            vertices[vertOffsetCurr + 0] = pointPosCurr;
            vertices[vertOffsetCurr + 1] = pointPosCurr;
            verticesPrev[vertOffsetCurr + 0] = pointPosPrev;
            verticesPrev[vertOffsetCurr + 1] = pointPosPrev;
            verticesNext[vertOffsetCurr + 0] = pointPosNextV4;
            verticesNext[vertOffsetCurr + 1] = pointPosNextV4;

            // tris
            int triOffset = pointIndexCurr * 6;
            tris[ triOffset + 0 ] = (vertOffsetCurr + 0) % vertices.Length;
            tris[ triOffset + 1 ] = (vertOffsetCurr + 2) % vertices.Length;
            tris[ triOffset + 2 ] = (vertOffsetCurr + 3) % vertices.Length;
            tris[ triOffset + 3 ] = (vertOffsetCurr + 0) % vertices.Length;
            tris[ triOffset + 4 ] = (vertOffsetCurr + 3) % vertices.Length;
            tris[ triOffset + 5 ] = (vertOffsetCurr + 1) % vertices.Length;

            // add point to bounds
            bounds.Encapsulate(pointPosCurr);
        }

        /*
        // handle first previous point (because has no previous)
        var firstPointPrev = points[0] * 2.0f - points[1];
        //if (numPoints == maxNumPoints) firstPointPrev = points[numPoints - 1]; // wrap around?
        verticesPrev[0] = firstPointPrev;
        verticesPrev[1] = firstPointPrev;
        
        // handle last point's next vert (because has no next)
        var lastPointNext = points[numPoints - 1] * 2 - points[numPoints - 2];
        //if (numPoints == maxNumPoints) lastPointNext = points[0]; // wrap around?
        verticesNext[(numPoints - 1) * 2 + 0] = lastPointNext;
        verticesNext[(numPoints - 1) * 2 + 1] = lastPointNext;*/

        /*
        // last triangle set should be zerod out
        if (numPoints < maxNumPoints)
        {
            int triOffsetFinal = (numPoints - 1) * 6;
            tris[triOffsetFinal + 0] = 0;
            tris[triOffsetFinal + 1] = 0;
            tris[triOffsetFinal + 2] = 0;
            tris[triOffsetFinal + 3] = 0;
            tris[triOffsetFinal + 4] = 0;
            tris[triOffsetFinal + 5] = 0;
        }*/

        mesh.bounds = bounds;
        SendToGpu();
    }

    public void BreakLineAt( int ptIndex )
    {
        ptIndex = (ptIndex + numPoints) % numPoints;

        int triOffset = ptIndex * 6;

        tris[(triOffset + 0) % tris.Length] = 0;
        tris[(triOffset + 1) % tris.Length] = 0;
        tris[(triOffset + 2) % tris.Length] = 0;
        tris[(triOffset + 3) % tris.Length] = 0;
        tris[(triOffset + 4) % tris.Length] = 0;
        tris[(triOffset + 5) % tris.Length] = 0;
    }

    public void SendToGpu()
    {
        // set mesh properties
        mesh.vertices = vertices;
        mesh.normals = verticesPrev;
        mesh.tangents = verticesNext;
        mesh.triangles = tris;
    }

    // call update only if points chaneg
    public void UpdateVerticesAll()
    {
        mesh.bounds = new Bounds(Vector3.zero, Vector3.zero);
        UpdateVerticesRange(0, numPoints);
    }

    void InitMesh()
    {
        var meshFilter = GetComponent<MeshFilter>();
        Mesh newMesh = null;

        if (meshFilter.sharedMesh)
        {
            newMesh = meshFilter.sharedMesh;
        }
        else {
            newMesh = new Mesh();
            meshFilter.sharedMesh = newMesh;
        }

        int numVerts = maxNumPoints * 2;

        // init verts
        vertices = new Vector3[numVerts];
        verticesPrev = new Vector3[numVerts];
        verticesNext = new Vector4[numVerts];
        tris = new int[(maxNumPoints) * 6];
        uvs = new Vector2[numVerts];

        // verts
        for (int i = 0; i < numVerts; i += 1)
        {
            vertices[i] = Vector3.zero;
            verticesPrev[i] = Vector3.zero;
            verticesNext[i] = Vector4.zero;
        }

        // faces
        for (int i = 0; i < tris.Length; ++i)
        {
            tris[i] = 0;
        }

        // uvs
        for (int i = 0; i < maxNumPoints; i += 1)
        {
            uvs[i * 2 + 0] = new Vector2(i, -1.0f);
            uvs[i * 2 + 1] = new Vector2(i, +1.0f);
        }

        newMesh.vertices = vertices;
        newMesh.normals = verticesPrev;
        newMesh.tangents = verticesNext;
        newMesh.triangles = tris;
        newMesh.uv = uvs;
        newMesh.MarkDynamic();

        mesh = meshFilter.sharedMesh;
    }
}
