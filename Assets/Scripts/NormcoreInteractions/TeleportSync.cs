using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Normal.Realtime;
using Bolt;
using Ludiq;

public class TeleportSync : RealtimeComponent<TeleportSyncModel>
{
    private Realtime core;
    private GameObject coreManager;
    private GameObject TFXManager;

    private void Awake()
    {
        coreManager = GameObject.Find("NormcoreManager");
        core = coreManager.GetComponent<Realtime>();
        TFXManager = GameObject.Find("TeleportFXManager");
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void TeleportInDidChange(TeleportSyncModel model, bool teleportIn)
    {
        
    }

    private void TeleportOutDidChange(TeleportSyncModel model, bool teleportOut)
    {
        if (teleportOut)
        {
            CustomEvent.Trigger(TFXManager, "TeleportOutFX", gameObject);
            model.teleportOut = false;
        }
    }

    protected override void OnRealtimeModelReplaced(TeleportSyncModel previousModel, TeleportSyncModel currentModel)
    {
        if (previousModel != null)
        {
            // Unregister from events
            previousModel.teleportInDidChange -= TeleportInDidChange;
            previousModel.teleportOutDidChange -= TeleportOutDidChange;
        }

        if (currentModel != null)
        {
            // If this is a model that has no data set on it, populate it with the current mesh renderer color.
            if (currentModel.isFreshModel)
            {
                currentModel.teleportIn = false; //spawn point
                currentModel.teleportOut = false;
            }

            // Register for events so we'll know if the color changes later
            currentModel.teleportInDidChange += TeleportInDidChange;
            currentModel.teleportOutDidChange += TeleportOutDidChange;
        }
    }

    public void SetTeleportIn(bool val)
    {
        model.teleportIn = val;
    }

    public void SetTeleportOut(bool val)
    {
        model.teleportOut = val;
    }
}
