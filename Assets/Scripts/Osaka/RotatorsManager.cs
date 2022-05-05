/************************************************************************************
* 
* Class Purpose: manager for every blade rotation and event management for them.
*           To use in combination with Rotator.cs on each blade
*
************************************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotatorsManager : MonoBehaviour
{
    [Header("Options")]
    [Tooltip("When enabled, the blades will start as if they started rotating time ago")]
    [HideInInspector]
    public bool forwardedInitialization = false;

    [Header("Blades - First set")]
    [Tooltip("Left side blades, from smaller to bigger")]
    public List<Rotator> leftSet = new List<Rotator>();

    [Header("Blades - Second set")]
    [Tooltip("Right side blades, from smaller to bigger")]
    public List<Rotator> rightSet = new List<Rotator>();

    [Header("Delay values - First set")]
    [Tooltip("Delays in seconds after that a blade (left side of the pavilion) starts its own rotation, from smaller to bigger")]
    public List<float> delaysLeftSet = new List<float>();

    [Header("Delay values - Second set")]
    [Tooltip("Delays in seconds after that a blade (right side of the pavilion) starts its own rotation, from smaller to bigger")]
    public List<float> delaysRightSet = new List<float>();

    [Header("Semiperiod values - First set")]
    [Tooltip("Semiperiod in seconds in which a blade (left side of the pavilion) covers half of its full degree rotation, from smaller to bigger")]
    public List<float> semiperiodLeftSet = new List<float>();

    [Header("Delay values - Second set")]
    [Tooltip("Semiperiod in seconds in which a blade (right side of the pavilion) covers half of its full degree rotation, from smaller to bigger")]
    public List<float> semiperiodRightSet = new List<float>();

    /*
     * Initialization of variables for each Rotator and assigment to the corresponding blade
     */
    private void Awake()
    {
        if (leftSet.Count == delaysLeftSet.Count && leftSet.Count == semiperiodLeftSet.Count)
        {
            //Init values for each ring
            int i = 0;
            float delaySum = 0;
            foreach (Rotator rt in leftSet)
            {
                delaySum += delaysLeftSet[i];
                rt.Delay = delaySum;
                rt.Semiperiod = semiperiodLeftSet[i];
                rt.ForwardedInitialization = forwardedInitialization;
                i++;
            }
        } else
        {
            Debug.LogWarning("All left side blades values must have been assigned and be in equal number!");
        }

        if (rightSet.Count == delaysRightSet.Count && rightSet.Count == semiperiodRightSet.Count)
        {
            //Init values for each ring
            int i = 0;
            float delaySum = 0;
            foreach (Rotator rt in rightSet)
            {
                delaySum += delaysRightSet[i];
                rt.Delay = delaySum;
                rt.Semiperiod = semiperiodRightSet[i];
                rt.ForwardedInitialization = forwardedInitialization;
                i++;
            }
        }
        else
        {
            Debug.LogWarning("All right side blades values must have been assigned and be in equal number!");
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
