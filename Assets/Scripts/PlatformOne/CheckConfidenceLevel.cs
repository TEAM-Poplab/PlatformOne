using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Normal.Realtime;

[RequireComponent(typeof(OVRHand))]
public class CheckConfidenceLevel : MonoBehaviour
{
    RealtimeAvatarManager realtimeAM;
    OVRHand.TrackingConfidence currentConfidence;
    OVRHand.TrackingConfidence previousConfidence;

    bool connected = false;

    // Start is called before the first frame update
    void Start()
    {
        realtimeAM = GameObject.Find("NormcoreManager").GetComponent<RealtimeAvatarManager>();
        currentConfidence = GetComponent<OVRHand>().HandConfidence;
        previousConfidence = currentConfidence;

        GameObject.Find("NormcoreManager").GetComponent<Realtime>().didConnectToRoom += RealtimeHandConfidence_didConnectToRoom;
        GameObject.Find("NormcoreManager").GetComponent<Realtime>().didDisconnectFromRoom += RealtimeHandConfidence_didDisconnectFromRoom;
    }

    private void RealtimeHandConfidence_didDisconnectFromRoom(Realtime realtime)
    {
        connected = false;
    }

    private void RealtimeHandConfidence_didConnectToRoom(Realtime realtime)
    {
        connected = true;
    }

    void Update()
    {
        if (connected)
        {
            if (previousConfidence != currentConfidence)
            {
                currentConfidence = GetComponent<OVRHand>().HandConfidence;
                if (GetComponent<OVRHand>().GetHandType() == OVRHand.Hand.HandLeft)
                {
                    realtimeAM.localAvatar.leftHand.GetChild(0).GetChild(1).GetComponent<RealtimeHandConfidence>().SetConfidenceLevel(currentConfidence, GetComponent<OVRHand>().GetHandType());
                } else
                {
                    realtimeAM.localAvatar.rightHand.GetChild(0).GetChild(1).GetComponent<RealtimeHandConfidence>().SetConfidenceLevel(currentConfidence, GetComponent<OVRHand>().GetHandType());
                }
                
                previousConfidence = currentConfidence;
            }

            currentConfidence = GetComponent<OVRHand>().HandConfidence;
        }
    }
}
