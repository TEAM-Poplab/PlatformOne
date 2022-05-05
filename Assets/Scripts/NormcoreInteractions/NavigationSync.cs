using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Normal.Realtime;

public class NavigationSync : RealtimeComponent<NavigationSyncModel>
{
    public Transform[] floors;
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

    private void FloorDidChange(NavigationSyncModel model, int floor)
    {
        if (core.clientID != 0)
        {
            GetComponent<UIManagerForUserMenuMRTKWithoutButtonsOsaka>().Teleport(floors[floor]);
        }
    }

    private void PositionDidChange(NavigationSyncModel model, Vector3 position)
    {
        if (core.clientID != 0)
        {
            GameObject tmpLocation = new GameObject();
            tmpLocation.transform.position = position;
            GetComponent<UIManagerForUserMenuMRTKWithoutButtonsOsaka>().Teleport(tmpLocation.transform, tmpLocation);
        }
    }

    protected override void OnRealtimeModelReplaced(NavigationSyncModel previousModel, NavigationSyncModel currentModel)
    {
        if (previousModel != null)
        {
            // Unregister from events
            previousModel.floorDidChange -= FloorDidChange;
            previousModel.positionDidChange -= PositionDidChange;
        }

        if (currentModel != null)
        {
            // If this is a model that has no data set on it, populate it with the current mesh renderer color.
            if (currentModel.isFreshModel)
            {
                currentModel.floor = 5; //spawn point
                //currentModel.position = coreManager.GetComponent<RealtimeAvatarManager>().localAvatar.gameObject.transform.position;
            }

            // Register for events so we'll know if the color changes later
            currentModel.floorDidChange += FloorDidChange;
            currentModel.positionDidChange += PositionDidChange;
        }
    }

    public void SetFloor(int floor)
    {
        // Set the floor on the model
        // This will fire the floorDidChange event on the model
        if (core.clientID == 0)
        {
            model.floor = floor;
        }
    }

    public void SetPosition()
    {
        if (core.clientID == 0)
        {
            model.position = coreManager.GetComponent<RealtimeAvatarManager>().localAvatar.gameObject.transform.position;
        }
    }
}
