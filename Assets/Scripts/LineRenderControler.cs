using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities;

//[RequireComponent(typeof(EdgeCollider2D))]
public class LineRenderControler : MonoBehaviour
{
    // Start is called before the first frame update
    Transform lineEndPoint;
    LineRenderer lr;
    //EdgeCollider2D egdeCol;


    [SerializeField]
    int numOfPoints;

    [SerializeField]
    float timeInterval;
    List<Vector3> points = new List<Vector3>();
    Vector3 cubeStartingPosition;
    CubeController cubeInstance; 
    void Start()
    {
        lineEndPoint = gameObject.transform.GetChild(0);
        lr = GetComponent<LineRenderer>();
        //egdeCol = GetComponent<EdgeCollider2D>();

        cubeInstance = GetComponent<CubeController>();
        cubeStartingPosition = gameObject.transform.position;
        //HelperFunctions.Log("Current Position: " + cubeStartingPosition);
    }

    private void Update()
    {
        //SetEdgeCollider();
        

    }

    public void CalculateProjectilePath()
    {
        points = HelperFunctions.CalculateProjectilePath(numOfPoints, timeInterval, cubeInstance.InitialVelocity,
            cubeInstance.CurrentPosition);
        //HelperFunctions.Log("First Point" + points[0]);
        lr.positionCount = numOfPoints;
        lr.SetPositions(points.ToArray());
        lr.enabled = true;
    }

    public void SetLineRenVisibility(bool isEnabled)
    {
        lr.enabled = isEnabled;
    }

    /*void SetEdgeCollider()
    {
        List<Vector2> edges = new List<Vector2>();

        for(int i = 0; i < lr.positionCount; i++)
        {
            Vector3 lrPoint = lr.GetPosition(i);
            edges.Add(new Vector2(lrPoint.x, lrPoint.y));
        }

        egdeCol.SetPoints(edges);
    }*/

}
