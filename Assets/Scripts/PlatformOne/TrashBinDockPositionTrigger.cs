using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit;
using Ludiq;
using Bolt;
using UnityEngine.Events;

//This class is for the trigger that check when and object in the platform dock is grabbed out the projection area
public class TrashBinDockPositionTrigger : MonoBehaviour
{
    [Tooltip("Called when the elimination process starts")] public UnityEvent onDeleteEvent = new UnityEvent();
    [Tooltip("Called when the elimination process is almost done, right before the destruction of selected object")] public UnityEvent onDeleteAndDestroyEvent = new UnityEvent();

    public AudioClip swooshSoundFx;
    public AudioClip emptyTrashSoundFx;

    public Color colorWhenInteracted;
    private Color originalColor;
    private Material handleMaterial;

    private void Awake()
    {
        // Ensure this collider can be used as a trigger by having
        // a RigidBody attached to it.
        var rigidBody = gameObject.EnsureComponent<Rigidbody>();
        rigidBody.isKinematic = true;

        //onDeleteEvent.AddListener(PlaySound);
        onDeleteAndDestroyEvent.AddListener(() => handleMaterial.color = originalColor);
    }

    private void Start()
    {
        handleMaterial = transform.parent.GetChild(0).GetChild(1).GetComponent<MeshRenderer>().material;
        originalColor = handleMaterial.color;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<Platformable>() != null || other.GetComponent<PlatformableMeshSequence>() != null)
        {
            Variables.Object(other.gameObject).Set("IsInTrashBinDockTrigger", false);
            handleMaterial.color = originalColor;
            //other.GetComponent<Platformable>().trashBin = null;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Platformable>() != null || other.GetComponent<PlatformableMeshSequence>() != null)
        {
            Variables.Object(other.gameObject).Set("IsInTrashBinDockTrigger", true);
            handleMaterial.color = colorWhenInteracted;
            //other.GetComponent<Platformable>().trashBin = this;
        }
    }

    public void PlaySound()
    {
        if (GetComponent<AudioSource>().isPlaying)
        {
            GetComponent<AudioSource>().Stop();
        }
        GetComponent<AudioSource>().Play();
    }
}
