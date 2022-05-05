using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Normal.Realtime;

public class DoorSync : RealtimeComponent<DoorSyncModel>
{
    public int Players;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void IsOpenedDidChange(DoorSyncModel model, bool isOpened)
    {
        //if (isOpened)
        //{
        //    GetComponent<DoorSensor>().SetAnimatorTriggerValue(isOpened);
        //} else if (!isOpened && model.playersInTrigger == 0)
        //{
        //    GetComponent<DoorSensor>().SetAnimatorTriggerValue(isOpened);
        //}
        GetComponent<DoorSensor>().SetAnimatorTriggerValue(isOpened);

    }

    protected override void OnRealtimeModelReplaced(DoorSyncModel previousModel, DoorSyncModel currentModel)
    {
        if (previousModel != null)
        {
            // Unregister from events
            previousModel.isOpenedDidChange -= IsOpenedDidChange;
        }

        if (currentModel != null)
        {
            // If this is a model that has no data set on it, populate it with the current mesh renderer color.
            if (currentModel.isFreshModel)
            {
                currentModel.isOpened = false; //spawn point
                currentModel.playersInTrigger = 0;
            }

            Players = model.playersInTrigger;
            // Register for events so we'll know if the color changes later
            currentModel.isOpenedDidChange += IsOpenedDidChange;
        }
    }

    public void SetIsOpened(bool isOpened)
    {
        if (model.isOpened == isOpened)
        {
            return;
        } else
        {
            model.isOpened = isOpened;
        }
    }

    public void IncreasePlayersCount()
    {
        model.playersInTrigger++;
        Players = model.playersInTrigger;
    }

    public void DecreasePlayersCount()
    {
        model.playersInTrigger--;
        Players = model.playersInTrigger;
    }
}
