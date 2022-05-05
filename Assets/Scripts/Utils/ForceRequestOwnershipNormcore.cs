using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Bolt;
using Ludiq;
using Normal.Realtime;

public class ForceRequestOwnershipNormcore : MonoBehaviour
{
    #region Ownership management for fingers
    public void RequestFingersOwnership()
    {
        //if (transform.parent.GetComponent<RealtimeAvatar>().isOwnedLocallyInHierarchy)
        //{
            ArrayList fingers = (AotList) Variables.Graph(GraphReference.New(GetComponent<FlowMachine>(), true)).Get("LocalFingers");
            foreach(Transform finger in fingers)
            {
                finger.gameObject.GetComponent<RealtimeTransform>().RequestOwnership();
            }
        //}
    }
    #endregion

    #region Ownership management for grabbing
    /// <summary>
    /// Request the ownership of the grabbed object. Controls about ownership are done here
    /// </summary>
    public void RequestGrabOwnership()
    {
        GameObject grabbed = (GameObject)Variables.Application.Get("ActiveGrabbedObject");
        if (grabbed.GetComponent<RealtimeTransform>().isUnownedInHierarchy)
        {
            grabbed.GetComponent<RealtimeTransform>().RequestOwnership();
        }
    }

    public void ClearGrabOwnership()
    {
        gameObject.GetComponent<RealtimeTransform>().ClearOwnership();
    }

    /// <summary>
    /// Pass the ownership of the grabbed object to the hand calling this method
    /// </summary>
    public void PassGrabOwnership()
    {
        GameObject grabbed = (GameObject)Variables.Application.Get("ActiveGrabbedObject");
        if (grabbed.GetComponent<RealtimeTransform>().ownerIDSelf != -1)
        {
            grabbed.GetComponent<RealtimeTransform>().RequestOwnership();
        }
    }

    /// <summary>
    /// Request the ownership of the grabbed object. Controls about ownership are not done.
    /// This allows to exchange grabbed objects betweens clients
    /// </summary>
    public void RequestGrabOwnershipNoOwnerCheck()
    {
        GameObject grabbed = (GameObject)Variables.Application.Get("ActiveGrabbedObject");
        grabbed.GetComponent<RealtimeTransform>().RequestOwnership();
    }
    #endregion

    #region Ownership management for tracking space
    public void RequestTrackingSpaceOwnership()
    {
        gameObject.GetComponent<RealtimeTransform>().RequestOwnership();
    }
    #endregion

    #region Ownership checking methods
    /// <summary>
    /// Check if the owner of the object is the same as the scene owner
    /// </summary>
    /// <param name="objectToCheck"></param>
    /// <returns>True if owner of the object and local owner are the same</returns>
    public bool CheckIfSameLocalOwner(GameObject objectToCheck)
    {
        int id = GameObject.Find("NormcoreManager").GetComponent<Realtime>().clientID;
        return gameObject.GetComponent<RealtimeTransform>().ownerIDSelf == id;
    }

    /// <summary>
    /// Check if the the object the method is called on has no owner
    /// </summary>
    /// <returns>True if the object has no owner, false otherwise</returns>
    public bool CheckIfHasNoOwner()
    {
        return gameObject.GetComponent<RealtimeTransform>().ownerIDSelf == -1;
    }
    #endregion
}
