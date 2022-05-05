using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorSensor : MonoBehaviour
{
    [SerializeField]
    Animator doorGameObjectAnimatorComponent;

    private int _playersInSensorRange = 0;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player" || other.tag == "NormcorePlayer")
        {
            //SetAnimatorTriggerValue(true);
            _playersInSensorRange++;
            
        }

        if(_playersInSensorRange > 0)
        {
            GetComponent<DoorSync>().SetIsOpened(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player" || other.tag == "NormcorePlayer")
        {
            //SetAnimatorTriggerValue(false);
            _playersInSensorRange--;
        }

        if (_playersInSensorRange == 0)
        {
            GetComponent<DoorSync>().SetIsOpened(false);
        }
    }

    public void SetAnimatorTriggerValue(bool isTriggered)
    {
        doorGameObjectAnimatorComponent.SetBool("IsTriggered", isTriggered);
    }
}
