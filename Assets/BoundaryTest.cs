using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoundaryTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //OVRBoundary.BoundaryTestResult test =  OVRManager.boundary.TestNode(OVRBoundary.Node.Head, OVRBoundary.BoundaryType.PlayArea);

        //Debug.LogWarning(test.IsTriggering);
        //Debug.LogWarning(test.ClosestDistance);
        //Debug.LogWarning(test.ClosestPoint);
        //Debug.LogWarning(test.ClosestPointNormal);
        if (OVRManager.boundary.GetConfigured())
        {
            Vector3[] geometry = OVRManager.boundary.GetGeometry(OVRBoundary.BoundaryType.PlayArea);

            foreach (Vector3 vec in geometry)
            {
                Debug.Log(vec);
            }
        } else
        {
            Debug.LogError("Boundary not configured. Error in Oculus Kit");
        }
        
    }
}
