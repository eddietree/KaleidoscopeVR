using UnityEngine;
using System.Collections;
using System;

public class SegmentMeshGenerator : MonoBehaviour
{
    public int numRotations = 8;

    MeshLine meshLine;

    void Start ()
    {
        // mesh line
        meshLine = gameObject.AddComponent<MeshLine>();
        meshLine.Init();

        InitDebugPoints();
        GenerateRotations();
    }
	
	void Update ()
    {
        HandleDrawLine();
	}

    void HandleDrawLine()
    {
        Plane plane = new Plane(new Vector3(0.0f, 0.0f, -1.0f), 0.0f);

        var camera = Camera.main;
        var camPos = camera.transform.position;
        var camDir = camera.transform.forward;

        var ray = new Ray(camPos, camDir);
        float enter = 0.0f;
        if ( plane.Raycast( ray, out enter ) )
        {
            var interesctionPt = camPos + camDir * enter;

            meshLine.AddPoint(interesctionPt);

            //meshLine.AddPoint(new Vector3( meshLine.numPoints, Mathf.Sin(meshLine.numPoints), 0.0f ));
            meshLine.UpdateVerticesRange(meshLine.numPoints-2, 1);
            //meshLine.UpdateVerticesAll();
        }
        //plane.Raycast()
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

            meshLine.AddPoint(new Vector3(x, y, z));
        }

        meshLine.UpdateVerticesAll();
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

        //GetComponent<MeshRenderer>().enabled = false;
    }

}
