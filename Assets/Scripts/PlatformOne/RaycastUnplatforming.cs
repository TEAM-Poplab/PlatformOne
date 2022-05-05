using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Bolt;
using Ludiq;

public class RaycastUnplatforming : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        RaycastHit hit;
        if ((bool) Variables.Application.Get("IsHolding") && GetComponent<Platformable>().CanUnplatform) {
            var pinchingPointTransform = (GameObject) Variables.Application.Get("PinchingFinger");
            if (Physics.Raycast(pinchingPointTransform.transform.position, pinchingPointTransform.transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity))
            {
                Debug.DrawRay(pinchingPointTransform.transform.position, pinchingPointTransform.transform.TransformDirection(Vector3.forward), Color.cyan);
            }
        }
    }
}
