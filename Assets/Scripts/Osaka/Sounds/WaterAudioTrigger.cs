using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class WaterAudioTrigger : MonoBehaviour
{
    [Tooltip("The AudioMixer Snapshot for water settings")] public AudioMixerSnapshot waterSnapshot;
    [Tooltip("The transition time between snapshots")] public float transitionTime = 1f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            waterSnapshot.TransitionTo(transitionTime);
            AudioManager.Instance.CurrentSnapshot = waterSnapshot;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            AudioManager.Instance.OutdoorBaseSnapshot.TransitionTo(transitionTime);
            AudioManager.Instance.CurrentSnapshot = AudioManager.Instance.OutdoorBaseSnapshot;
        }
    }

    ////If the player use the teleport system, collisions won't bge detected, so we use OnTrigger stay with control to ensure the player is inside the trigger area
    //private void OnTriggerStay(Collider other)
    //{
    //    if (other.tag == "Player")
    //    {
    //        Debug.LogWarning("OnTriggerStay");
    //        if (AudioManager.Instance.CurrentSnapshot == baseSnapshot)
    //        {
    //            waterSnapshot.TransitionTo(0.5f);
    //        }
    //    }
    //}
}
