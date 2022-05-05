using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class IndoorPerimetricTrigger : MonoBehaviour
{
    [Tooltip("The AudioMixer Snapshot for indoor settings near perimetric walls")] public AudioMixerSnapshot indoorPerimetricBase;
    [Tooltip("The transition time between snapshots")] public float transitionTime = 0.25f;

    private AudioMixerSnapshot previousSnapshot;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            if (AudioManager.Instance.CurrentSnapshot != indoorPerimetricBase)
            {
                indoorPerimetricBase.TransitionTo(transitionTime);
                previousSnapshot = AudioManager.Instance.CurrentSnapshot;
                AudioManager.Instance.CurrentSnapshot = indoorPerimetricBase;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            previousSnapshot.TransitionTo(transitionTime);
            AudioManager.Instance.CurrentSnapshot = previousSnapshot;
        }
    }
}
