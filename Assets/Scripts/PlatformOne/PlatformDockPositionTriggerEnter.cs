using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit;
using Ludiq;
using Bolt;

//This class is for the trigger that check when and object in the platform dock is grabbed out the projection area
public class PlatformDockPositionTriggerEnter : MonoBehaviour
{
    private void Awake()
    {
        // Ensure this collider can be used as a trigger by having
        // a RigidBody attached to it.
        var rigidBody = gameObject.EnsureComponent<Rigidbody>();
        rigidBody.isKinematic = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Platformable>() != null)
        {
            Variables.Object(other.gameObject).Set("IsInPlatformDockTrigger", true);
        }
    }
}