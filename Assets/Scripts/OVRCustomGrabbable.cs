/************************************************************************************
* 
* Class Purpose: it overiddes the OVRGrabbable class
*
************************************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OVRCustomGrabbable : OVRGrabbable
{
    override public void GrabBegin(OVRGrabber hand, Collider grabPoint)
    {
        m_grabbedBy = hand;
        m_grabbedCollider = grabPoint;
        //gameObject.GetComponent<Rigidbody>().isKinematic = true;
    }

    override public void GrabEnd(Vector3 linearVelocity, Vector3 angularVelocity)
    {
        Rigidbody rb = gameObject.GetComponent<Rigidbody>();
        //rb.isKinematic = m_grabbedKinematic;
        //rb.velocity = linearVelocity;
        rb.angularVelocity = angularVelocity;
        m_grabbedBy = null;
        m_grabbedCollider = null;
    }
}
