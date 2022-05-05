using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Normal.Realtime;

public class AudioSync : RealtimeComponent<AudioSyncModel>
{
    public bool UseAudio = true;
    private Realtime core;
    private RealtimeAvatarVoice avatarVoice;
    private UIManagerForUserMenuMRTKWithoutButtonsOsaka uiManager;
    private float savedDBValue;

    private void Awake()
    {
        core = GameObject.Find("NormcoreManager").GetComponent<Realtime>();
    }

    // Start is called before the first frame update
    void Start()
    {
        uiManager = GameObject.Find("UIManager").GetComponent<UIManagerForUserMenuMRTKWithoutButtonsOsaka>();
        if (core.clientID == 0)
        {
            uiManager.SetAudioButton();

        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void AudioDidChange(AudioSyncModel model, bool useAudio)
    {
        UseAudio = useAudio;
        GetComponent<RealtimeAvatarVoice>().mute = !useAudio;
    }

    private void DBValueDidChange(AudioSyncModel model, float dbValue)
    {

    }

    protected override void OnRealtimeModelReplaced(AudioSyncModel previousModel, AudioSyncModel currentModel)
    {
        if (previousModel != null)
        {
            // Unregister from events
            previousModel.useAudioDidChange -= AudioDidChange;
            previousModel.dbValueDidChange -= DBValueDidChange;
        }

        if (currentModel != null)
        {
            // If this is a model that has no data set on it, populate it with the current mesh renderer color.
            if (currentModel.isFreshModel)
            {
                currentModel.useAudio = true;
                currentModel.dbValue = -42f;
            }

            GetComponent<RealtimeAvatarVoice>().mute = model.useAudio;

            // Register for events so we'll know if the color changes later
            currentModel.useAudioDidChange += AudioDidChange;
            currentModel.dbValueDidChange += DBValueDidChange;
        }
    }

    public void SetAudio(bool audio)
    {
        // Set the floor on the model
        // This will fire the floorDidChange event on the model
        //model.useAudio = audio;
        if (core.clientID == 0)
        {
            this.RequestOwnership();
            if (audio)
            {
                model.dbValue = savedDBValue;
                model.useAudio = audio;
            } else
            {
                savedDBValue = GetComponent<RealtimeAvatarVoice>().voiceVolume;
                model.dbValue = 0.01f;
                model.useAudio = audio;
            }
        }

    }
}
