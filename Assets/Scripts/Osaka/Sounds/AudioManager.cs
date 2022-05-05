using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : Singleton<AudioManager>
{
    [SerializeField] private AudioMixer sceneAudioMixer;
    [SerializeField] private AudioMixerSnapshot outdoorBaseSnapshot;

    private AudioMixerSnapshot currentSnapshot;

    public AudioMixer SceneAudioMixer
    {
        get => sceneAudioMixer;
        private set => sceneAudioMixer = value;
    }

    public AudioMixerSnapshot OutdoorBaseSnapshot
    {
        get => outdoorBaseSnapshot;
        private set => outdoorBaseSnapshot = value;
    }

    public AudioMixerSnapshot CurrentSnapshot
    {
        get => currentSnapshot;
        set => currentSnapshot = value;
    }
    // Start is called before the first frame update
    void Start()
    {
        currentSnapshot = outdoorBaseSnapshot;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
