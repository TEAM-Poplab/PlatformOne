using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Normal.Realtime;
using Bolt;
using Ludiq;

public class RealtimeHandConfidence : RealtimeComponent<RealtimeHandConfidenceModel>
{
    private OVRHand.Hand hand = OVRHand.Hand.None;

    private void Start()
    {
        GameObject.Find("NormcoreManager").GetComponent<Realtime>().didConnectToRoom += RealtimeHandConfidence_didConnectToRoom;
    }

    private void RealtimeHandConfidence_didConnectToRoom(Realtime realtime)
    {
        model.clientID = realtime.clientID;
    }

    protected override void OnRealtimeModelReplaced(RealtimeHandConfidenceModel previousModel, RealtimeHandConfidenceModel currentModel)
    {
        if (previousModel != null)
        {
            // Unregister from events
            previousModel.trackingConfidenceDidChange -= CurrentModel_trackingConfidenceDidChange;
        }

        if (currentModel != null)
        {
            //If this is a model that has no data set on it, populate it.
            if (currentModel.isFreshModel)
            {
                currentModel.trackingConfidence = 1;
            }

            // Register for events
            currentModel.trackingConfidenceDidChange += CurrentModel_trackingConfidenceDidChange;
        }
    }

    private void CurrentModel_trackingConfidenceDidChange(RealtimeHandConfidenceModel model, int value)
    {
        if (hand == OVRHand.Hand.HandLeft)
        {
            Debug.LogError("Changing confidence level, left hand event");
            CustomEvent.Trigger(gameObject, "OnConfidenceChange", value == 0 ? OVRHand.TrackingConfidence.Low : OVRHand.TrackingConfidence.High);
        } else if (hand == OVRHand.Hand.HandRight)
        {
            Debug.LogError("Changing confidence level, right hand event");
            CustomEvent.Trigger(gameObject, "OnConfidenceChange", value == 0 ? OVRHand.TrackingConfidence.Low : OVRHand.TrackingConfidence.High);
        }
        
    }

    public void SetConfidenceLevel(OVRHand.TrackingConfidence value, OVRHand.Hand hand)
    {
        Debug.LogError("Changing confidence level, hand: " + hand);
        if (this.hand == OVRHand.Hand.None)
        {
            this.hand = hand;
            model.handType = hand == OVRHand.Hand.HandLeft ? 0 : 1;
        }
        
        if (value == OVRHand.TrackingConfidence.Low)
        {
            model.trackingConfidence = 0;
        } else
        {
            model.trackingConfidence = 1;
        }
    }

    public void SetClientID(int value)
    {
        model.clientID = value;
    }

    public OVRHand.Hand GetHandType() => model.handType == 0 ? OVRHand.Hand.HandLeft : OVRHand.Hand.HandRight;

    public int GetClientID() => model.clientID;

    public OVRHand.TrackingConfidence GetConfidence() => model.trackingConfidence == 0 ? OVRHand.TrackingConfidence.Low : OVRHand.TrackingConfidence.High;
}
