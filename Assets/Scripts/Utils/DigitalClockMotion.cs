using Microsoft.MixedReality.Toolkit.Utilities.Solvers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Bolt;
using Ludiq;

public class DigitalClockMotion : MonoBehaviour
{
    private Transform clockSlotInDock;
    private RadialView rv;
    private GameObject increaseButton;
    private GameObject decreaseButton;

    bool pressedStatus = false;

    // Start is called before the first frame update
    void Start()
    {
        clockSlotInDock = GameObject.Find("Dock/ClockSlot").transform;
        rv = GetComponent<RadialView>();

        increaseButton = transform.GetChild(2).gameObject;
        decreaseButton = transform.GetChild(1).gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        pressedStatus = (bool) Variables.Object(increaseButton).Get("Pressed") | (bool) Variables.Object(decreaseButton).Get("Pressed");

        if (UIManagerForUserMenuMRTKWithoutButtons.Instance.IsDockActive)
        {
            if (gameObject.activeSelf)
            {
                rv.enabled = false;
                transform.position = clockSlotInDock.position;
                transform.rotation = clockSlotInDock.rotation;
            } else
            {
                rv.enabled = true;
            }
        } else
        {
            if (gameObject.activeSelf && !pressedStatus)
            {
                rv.enabled = true;
            }
        }
    }
}
