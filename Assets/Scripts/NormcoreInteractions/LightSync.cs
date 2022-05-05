using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Normal.Realtime;

public class LightSync : RealtimeComponent<LightSyncModel>
{
    private Realtime core;

    private void Awake()
    {
        core = GameObject.Find("NormcoreManager").GetComponent<Realtime>();
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void ForceLightSyncDidChange(LightSyncModel model, bool forceLightSync)
    {
        if (forceLightSync)
        {
            GetComponent<CustomLightManagerForOsaka>().ResetSunRotation();
            if (core.clientID == model.leastConnectedClientID)
            {
                model.forceLightSync = false;
            }
        }
    }

    private void LeastConnectedClientIDDidChange(LightSyncModel model, int leastConnectedClientID)
    {
        if (core.clientID == leastConnectedClientID)
        {
            this.RequestOwnership();
        }
    }

    protected override void OnRealtimeModelReplaced(LightSyncModel previousModel, LightSyncModel currentModel)
    {
        if (previousModel != null)
        {
            // Unregister from events
            previousModel.forceLightSyncDidChange -= ForceLightSyncDidChange;
            previousModel.leastConnectedClientIDDidChange -= LeastConnectedClientIDDidChange;
        }

        if (currentModel != null)
        {
            // If this is a model that has no data set on it, populate it with the current mesh renderer color.
            if (currentModel.isFreshModel)
            {
                currentModel.forceLightSync = false;
                currentModel.leastConnectedClientID = core.clientID;
            }

            // Register for events so we'll know if the color changes later
            currentModel.forceLightSyncDidChange += ForceLightSyncDidChange;
            currentModel.leastConnectedClientIDDidChange += LeastConnectedClientIDDidChange;
        }
    }

    public void SetForceLightSync(bool isTrue)
    {
        model.forceLightSync = isTrue;
    }

    public void SetLeastConnectedClientID(int id)
    {
        model.leastConnectedClientID = id;
    }

    public int GetLeastConnectedClientID()
    {
        return model.leastConnectedClientID;
    }
}
