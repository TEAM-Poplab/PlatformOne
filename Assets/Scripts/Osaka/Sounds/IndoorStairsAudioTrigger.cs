using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class IndoorStairsAudioTrigger : MonoBehaviour
{
    //[Tooltip("The AudioMixer Group for indoor stairs settings")] public AudioMixerGroup stairsGroup;
    [Tooltip("The indoor stairs ambience sound volume")] public float volume = 0f;
    private float prevVolume = 0f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            if (AudioManager.Instance.SceneAudioMixer.GetFloat("IndoorStairsVolume", out prevVolume))
            {
                AudioManager.Instance.SceneAudioMixer.SetFloat("IndoorStairsVolume", volume);
            }  
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            AudioManager.Instance.SceneAudioMixer.SetFloat("IndoorStairsVolume", prevVolume);
        }
    }
}
