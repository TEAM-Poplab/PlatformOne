/************************************************************************************
* 
* Class Purpose: it overrides the OVRGrabber class in order to implement grabbing based on joints and physics
*
************************************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OVRCustomGrabber : OVRGrabber
{
    protected override void MoveGrabbedObject(Vector3 pos, Quaternion rot, bool forceTeleport = false)
    {
        if (m_grabbedObj == null)
        {
            return;
        }

        Rigidbody grabbedRigidbody = m_grabbedObj.grabbedRigidbody;
        Vector3 grabbablePosition = pos.x * m_grabbedObjectPosOff;

        if (forceTeleport)
        {
            grabbedRigidbody.transform.position = grabbablePosition;
        }
        else
        {
            grabbedRigidbody.MovePosition(grabbablePosition);
        }


    }

    protected override void GrabBegin()
    {
        float closestMagSq = float.MaxValue;
        OVRGrabbable closestGrabbable = null;
        Collider closestGrabbableCollider = null;

        // Iterate grab candidates and find the closest grabbable candidate
        foreach (OVRGrabbable grabbable in m_grabCandidates.Keys)
        {
            bool canGrab = !(grabbable.isGrabbed && !grabbable.allowOffhandGrab);
            if (!canGrab)
            {
                continue;
            }

            for (int j = 0; j < grabbable.grabPoints.Length; ++j)
            {
                Collider grabbableCollider = grabbable.grabPoints[j];
                // Store the closest grabbable
                Vector3 closestPointOnBounds = grabbableCollider.ClosestPointOnBounds(m_gripTransform.position);
                float grabbableMagSq = (m_gripTransform.position - closestPointOnBounds).sqrMagnitude;
                if (grabbableMagSq < closestMagSq)
                {
                    closestMagSq = grabbableMagSq;
                    closestGrabbable = grabbable;
                    closestGrabbableCollider = grabbableCollider;
                }
            }
        }

        // Disable grab volumes to prevent overlaps
        GrabVolumeEnable(false);

        if (closestGrabbable != null)
        {
            m_grabbedObj = closestGrabbable;
            m_grabbedObj.GrabBegin(this, closestGrabbableCollider);

            m_lastPos = transform.position;
            m_lastRot = transform.rotation;

            //Create a fixed joint from the hand to the holding body => Physics interaction starts
            Rigidbody handRigidbody = GetComponent<Rigidbody>();
            handRigidbody.isKinematic = true;
            FixedJoint handJoint = this.gameObject.AddComponent(typeof(FixedJoint)) as FixedJoint;
            handJoint.connectedBody = m_grabbedObj.GetComponent<Rigidbody>();

            // Set up offsets for grabbed object desired position relative to hand.
            //Vector3 relPos = transform.position - m_grabbedObj.transform.position;
            //relPos = Quaternion.Inverse(transform.rotation) * relPos;
            //transform.position = relPos; 

            // Note: force teleport on grab, to avoid high-speed travel to dest which hits a lot of other objects at high
            // speed and sends them flying. The grabbed object may still teleport inside of other objects, but fixing that
            // is beyond the scope of this demo.
            MoveGrabbedObject(m_lastPos, m_lastRot, true);
            SetPlayerIgnoreCollision(m_grabbedObj.gameObject, true);
        }
    }

    protected void GrabEnd()
    {
        if (m_grabbedObj != null)
        {
            //Delete the fixed joint from the hand to the holding object => Physical interaction ends
            Destroy(this.gameObject.GetComponent<FixedJoint>());

            OVRPose localPose = new OVRPose { position = OVRInput.GetLocalControllerPosition(m_controller), orientation = OVRInput.GetLocalControllerRotation(m_controller) };
            OVRPose offsetPose = new OVRPose { position = m_anchorOffsetPosition, orientation = m_anchorOffsetRotation };
            localPose = localPose * offsetPose;

            OVRPose trackingSpace = transform.ToOVRPose() * localPose.Inverse();
            Vector3 linearVelocity = trackingSpace.orientation * OVRInput.GetLocalControllerVelocity(m_controller);
            Vector3 angularVelocity = trackingSpace.orientation * OVRInput.GetLocalControllerAngularVelocity(m_controller);

            GrabbableRelease(linearVelocity, angularVelocity);
        }

        // Re-enable grab volumes to allow overlap events
        GrabVolumeEnable(true);   
    }
}
