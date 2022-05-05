using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Bolt;
using Ludiq;

public class HandTrackingLostDetector : MonoBehaviour
{
    private GameObject leftHand = null;
    private GameObject rightHand = null;
    private SkinnedMeshRenderer leftHandMeshRender;
    private SkinnedMeshRenderer rightHandMeshRender;

    private GameObject currentGrabbingHand;

    // Start is called before the first frame update
    void Start()
    {
        leftHand = GameObject.Find("MixedRealityPlayspace/MRTK-Quest_OVRCameraRig(Clone)/TrackingSpace/LeftHandAnchor/OVRHandPrefab_Left");
        leftHandMeshRender = leftHand.GetComponent<SkinnedMeshRenderer>();
        rightHand = GameObject.Find("MixedRealityPlayspace/MRTK-Quest_OVRCameraRig(Clone)/TrackingSpace/RightHandAnchor/OVRHandPrefab_Right");
        rightHandMeshRender = rightHand.GetComponent<SkinnedMeshRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        currentGrabbingHand = (GameObject)Variables.Application.Get("GrabbingHand");

        if (!rightHandMeshRender.enabled)
        {
            if (currentGrabbingHand == rightHand)
            {
                ForceUngrab();
            }
        }

        if (!leftHandMeshRender.enabled)
        {
            if (currentGrabbingHand == leftHand)
            {
                ForceUngrab();
            }
        }


    }

    private void ForceUngrab()
    {
        CustomEvent.Trigger((GameObject)Variables.Application.Get("ActiveGrabbedObject"), "GrabOff");
        CustomEvent.Trigger((GameObject)Variables.Application.Get("GameManager"), "GrabOutRange", currentGrabbingHand);
        CustomEvent.Trigger((GameObject)Variables.Application.Get("GameManager"), "ForcedGrabOutRange");
        Variables.Application.Set("InGrabRange", false);
        Variables.Application.Set("HandInRange", null);
        Variables.Application.Set("ActiveGrabbedObject", null);
        Variables.Application.Set("GrabbingHand", null);
        Variables.Application.Set("Grabbing", false);
    }
}
