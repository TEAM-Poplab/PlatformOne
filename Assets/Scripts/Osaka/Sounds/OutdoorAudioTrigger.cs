using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class OutdoorAudioTrigger : MonoBehaviour
{
    [Tooltip("The AudioMixer Snapshot for outdoor settings")] public AudioMixerSnapshot outdoorSnapshot;
    [Tooltip("The transition time between snapshots")] public float transitionTime = 0.5f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            if (AudioManager.Instance.CurrentSnapshot != outdoorSnapshot)
            {
                outdoorSnapshot.TransitionTo(transitionTime);
                AudioManager.Instance.CurrentSnapshot = outdoorSnapshot;
            }
        }
    }
}
