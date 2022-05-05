using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Normal.Realtime;

public class SyncReality : RealtimeComponent<SyncRealityModel>
{
    private Realtime core;
    private GameObject coreManager;

    private void Awake()
    {
        coreManager = GameObject.Find("NormcoreManager");
        core = coreManager.GetComponent<Realtime>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void RealityDidChange(SyncRealityModel model, bool val)
    {
        if (core.clientID != 0)
        {
            UIManagerForUserMenuMRTKWithoutButtons.Instance.RealityHandler();
        }
    }

    protected override void OnRealtimeModelReplaced(SyncRealityModel previousModel, SyncRealityModel currentModel)
    {
        if (previousModel != null)
        {
            // Unregister from events
            previousModel.isRealityOnDidChange -= RealityDidChange;
        }

        if (currentModel != null)
        {
            // If this is a model that has no data set on it, populate it with the current mesh renderer color.
            if (currentModel.isFreshModel)
            {
                currentModel.isRealityOn = false;
                //currentModel.position = coreManager.GetComponent<RealtimeAvatarManager>().localAvatar.gameObject.transform.position;
            }

            // Register for events so we'll know if the color changes later
            currentModel.isRealityOnDidChange += RealityDidChange;
        }
    }

    public void SetRealityOn()
    {
        if (model.isRealityOn)
        {
            model.isRealityOn = false;
        } else
        {
            model.isRealityOn = true;
        }
    }
}
