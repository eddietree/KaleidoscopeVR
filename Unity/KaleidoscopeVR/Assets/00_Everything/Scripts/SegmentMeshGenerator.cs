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

        GetComponent<MeshRenderer>().enabled = false;
    }

}
