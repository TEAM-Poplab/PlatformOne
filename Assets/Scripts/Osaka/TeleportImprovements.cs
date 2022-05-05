using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Teleport;
using prvncher.MixedReality.Toolkit.Input.Teleport;
using Microsoft.MixedReality.Toolkit.Input;

public class TeleportImprovements : MonoBehaviour, IMixedRealityTeleportHandler
{
    public LayerMask invalidLayers;
    CustomTeleportPointer teleportPointer;
    WaitForSeconds timer = new WaitForSeconds(0.5f);

    private void Start()
    {
        teleportPointer = GetComponent<CustomTeleportPointer>();
    }


    public void OnTeleportCanceled(TeleportEventData eventData)
    {
        Debug.Log("Teleport Cancelled");
        StopAllCoroutines();
    }

    public void OnTeleportCompleted(TeleportEventData eventData)
    {
        Debug.Log("Teleport Completed");
    }

    //public void OnTeleportRequest(TeleportEventData eventData)
    //{
    //    CustomTeleportPointer.TeleportPointerHitResult hitResult = new CustomTeleportPointer.TeleportPointerHitResult();
    //    LayerMask[] maskList = new LayerMask[invalidLayers];

    //    float rayStartDistance = 0;
    //    for (int i = 0; i < teleportPointer.Rays.Length; i++)
    //    {
    //        if (CoreServices.InputSystem.RaycastProvider.Raycast(teleportPointer.Rays[i], maskList, true, out MixedRealityRaycastHit hitInfos))
    //        {
    //            hitResult.Set(hitInfos, teleportPointer.Rays[i], i, rayStartDistance + hitInfos.distance, true);
    //            break;
    //        }
    //        rayStartDistance += teleportPointer.Rays[i].Length;
    //    }
    //    if (hitResult.hitObject != null)
    //    {
    //        for (int i = 0; i < teleportPointer.LineRenderers.Length; i++)
    //        {
    //            teleportPointer.LineRenderers[i].LineColor = teleportPointer.LineColorInvalid;
    //        }
    //        //CoreServices.TeleportSystem?.RaiseTeleportCanceled(eventData.Pointer, eventData.HotSpot);
    //    }
    //}

    public void OnTeleportRequest(TeleportEventData eventData)
    {
        //StartCoroutine(CheckForWalls());
        for (int i = 0; i < teleportPointer.LineRenderers.Length; i++)
        {
            teleportPointer.LineRenderers[i].LineColor = teleportPointer.LineColorInvalid;
        }

    }

    public void OnTeleportStarted(TeleportEventData eventData)
    {
        Debug.Log("Teleport Started");
    }

    void OnEnable()
    {
        // This is the critical call that registers this class for events. Without this
        // class's IMixedRealityTeleportHandler interface will not be called.
        CoreServices.TeleportSystem.RegisterHandler<IMixedRealityTeleportHandler>(this);
    }

    void OnDisable()
    {
        // Unregistering when disabled is important, otherwise this class will continue
        // to receive teleportation events.
        CoreServices.TeleportSystem.UnregisterHandler<IMixedRealityTeleportHandler>(this);
    }

    IEnumerator CheckForWalls()
    {
        if (teleportPointer.TeleportHotSpot.GameObjectReference.tag == "Wall")
        {
            for (int i = 0; i < teleportPointer.LineRenderers.Length; i++)
            {
                teleportPointer.LineRenderers[i].LineColor = teleportPointer.LineColorInvalid;
            }
        }

        yield return timer;
    }
}
