using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities;

public class LineRenderControler : MonoBehaviour
{
    // Start is called before the first frame update
    Transform lineEndPoint;
    LineRenderer lr;

    [SerializeField]
    int numOfPoints;

    [SerializeField]
    float timeInterval;

    CubeController cubeInstance; 
    void Start()
    {
        lineEndPoint = gameObject.transform.GetChild(0);
        lr = GetComponent<LineRenderer>();
        cubeInstance = GetComponent<CubeController>();
    }

    public void CalculateProjectilePath()
    {
        List<Vector3> points = HelperFunctions.CalculateProjectilePath(numOfPoints, timeInterval, cubeInstance.InitialVelocity,
            cubeInstance.InitialPosition);
        lr.positionCount = numOfPoints;
        lr.SetPositions(points.ToArray());
        lr.enabled = true;
    }

    public void SetLineRenVisibility(bool isEnabled)
    {
        lr.enabled = isEnabled;
    }

}
