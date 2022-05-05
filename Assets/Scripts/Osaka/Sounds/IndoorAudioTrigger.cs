using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class IndoorAudioTrigger : MonoBehaviour
{
    [Tooltip("The AudioMixer Snapshot for indoor settings")] public AudioMixerSnapshot indoorSnapshot;
    [Tooltip("The transition time between snapshots")] public float transitionTime = 1f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            if (AudioManager.Instance.CurrentSnapshot != indoorSnapshot)
            {
                indoorSnapshot.TransitionTo(transitionTime);
                AudioManager.Instance.CurrentSnapshot = indoorSnapshot;
            }
        }
    }
}
