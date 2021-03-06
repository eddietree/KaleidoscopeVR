﻿using UnityEngine;
using System.Collections;
using System;

public class SegmentMeshGenerator : MonoBehaviour
{
    public int numRotations = 8;
    private int numPointsAdded = 0;
    private int pointAddedIndex = 0;

    MeshLine meshLine;

    // draw lines
    Vector3 drawPosPrev = Vector3.zero;
    Vector3 drawVelAccum = Vector3.zero;

    public bool autoDraw = false;
    public float autoDrawSpeed = 1.0f;
    public float autoDrawRadius = 0.5f;

    void Start ()
    {
        // mesh line
        meshLine = gameObject.AddComponent<MeshLine>();
        meshLine.Init();

        //InitDebugPoints();
        GenerateRotations();

        if (autoDraw)
        {
            StartCoroutine(DrawInfiniteLoop());
        }
    }
	
	void Update ()
    {
        //if ( Input.GetKey(KeyCode.Space) )
        if ( !autoDraw && Input.GetMouseButton(0) )
            HandleDrawLine();

        if (Input.GetKey(KeyCode.C))
            meshLine.Clear();
	}

    IEnumerator DrawInfiniteLoop()
    {
        while(true)
        {
            float time = Time.time * autoDrawSpeed;
            float radius = autoDrawRadius;
            Vector3 point = new Vector3(Mathf.Cos(time*1.5f) * radius, Mathf.Sin(time*2.0f) * radius, -1.0f);
            TryAddPoint(point);

            yield return null;
        }
    }

    void HandleDrawLine()
    {
        Plane plane = new Plane(new Vector3(0.0f, 0.0f, -1.0f), 0.0f);

        var camera = Camera.main;
        var mousePos = Input.mousePosition;

        var ray = camera.ScreenPointToRay(mousePos);
        //var ray = new Ray( camera.transform.position, camera.transform.forward );

        float enter = 0.0f;

        if ( plane.Raycast( ray, out enter ) )
        {
            var interesctionPt = ray.origin + ray.direction * enter;
            TryAddPoint(interesctionPt);
        }
    }

    void TryAddPoint( Vector3 pt )
    {
        // calc velocity
        Vector3 drawPosVel = (pt - drawPosPrev) / Time.deltaTime;
        drawVelAccum = Vector3.Lerp(drawVelAccum, drawPosVel, 0.1f);
        drawPosPrev = pt;

        if (meshLine.numPoints > 2)
        {
            var points = meshLine.points;
            var lastPt = points[meshLine.numPoints - 1];

            if (Vector3.Distance(lastPt, pt) < 0.01f)
            {
                return;
            }

            points[meshLine.numPoints - 1] = (points[meshLine.numPoints - 2] + pt) * 0.5f;
        }

        var speed = drawVelAccum.magnitude;
        //Debug.Log(speed);
        var velMin = 0.0f;
        var velMax = 20.0f;
        var lineThicknessMin = 0.1f;
        var lineThicknessMax = 5.0f;

        var smoothstep = Mathf.Clamp01( (drawVelAccum.magnitude - velMin) / (velMax-velMin) );

        meshLine.lineThickness = Mathf.Lerp( lineThicknessMin, lineThicknessMax, 1.0f-smoothstep);

        //var vecTocam = (Camera.main.transform.position - pt).normalized;
        //pt += vecTocam * Time.time * 0.1f;

        // already maxed out
        if (numPointsAdded >= MeshLine.maxNumPoints)
        {
            int pointIndex = numPointsAdded % MeshLine.maxNumPoints;
            meshLine.SetPoint(pt, pointIndex);
            meshLine.UpdateVerticesRange(pointIndex - 1, 2);

            meshLine.BreakLineAt(pointIndex);
            meshLine.SendToGpu();
        }
        else
        {
            // add point
            meshLine.AddPoint(pt);

            //  need to update last two because edges
            meshLine.UpdateVerticesRange(meshLine.numPoints - 2, 2);
        }

        ++numPointsAdded;
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
