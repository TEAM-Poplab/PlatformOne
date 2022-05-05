using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Teleport;
using Bolt;
using Ludiq;

public class TPEventsListener : MonoBehaviour, IMixedRealityTeleportHandler
{
    public void OnTeleportCanceled(TeleportEventData eventData)
    {
    }

    public void OnTeleportCompleted(TeleportEventData eventData)
    {
        CustomEvent.Trigger(gameObject, "ChangePosition");
    }

    public void OnTeleportRequest(TeleportEventData eventData)
    {
        CustomEvent.Trigger(gameObject, "SearchTeleport");
    }

    public void OnTeleportStarted(TeleportEventData eventData)
    {
    }

    void OnEnable()
    {
        CoreServices.TeleportSystem.RegisterHandler<IMixedRealityTeleportHandler>(this);
    }

    void OnDisable()
    {
        CoreServices.TeleportSystem.UnregisterHandler<IMixedRealityTeleportHandler>(this);
    }
}
