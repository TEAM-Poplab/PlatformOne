/************************************************************************************
* 
* Class Purpose: it's a controller for any scene element related to the linear MappingValue
*
************************************************************************************/

using Microsoft.MixedReality.Toolkit.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class ValuesAnimationController : MonoBehaviour
{
    #region Properties
    //[Tooltip("The GameObject containing the track which will be scaled during the pinch slider interaction")]
    //public GameObject track;

    [Tooltip("Limit min slider value to 0.05 in order to avoid limit case for geometries")]
    public bool limitMinValue = false;
    #endregion

    #region Fields
    private GHLoader ghloaderScript;    //The GHLoaderScript to access to the Grasshopper's input variables array
    private float linearValue = 1;
    //public Triggers trigger;    //The trigger area where the handle which controls the LinearMappingValue is set to
    #endregion

    #region Unity events
    // Start is called before the first frame update
    void Start()
    {
        ghloaderScript = GetComponent<GHLoader>();
    }
    #endregion

    #region Public custom methods for events
    //Identify handle's value changes in order to change the GHLoader in values (and send them to the server for a new rendering) and any animation changes
    public void OnSliderUpdatedValue(SliderEventData eventData)
    {
        linearValue = eventData.NewValue;

        if (limitMinValue && linearValue < 0.05)
        {
            linearValue = 0.05f;
        }

        ghloaderScript.ghInputs[0].value = (float)linearValue * 90;
        ghloaderScript.SendGHData();
        //track.transform.localScale = new Vector3(1f, 1f - eventData.NewValue + 0.2f, 1f);
    }
    #endregion
}