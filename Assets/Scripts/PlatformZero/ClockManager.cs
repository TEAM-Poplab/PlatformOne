/************************************************************************************
* 
* Class Purpose: the class controls any in/out interaction with the clock object
*
************************************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;

public class ClockManager : MonoBehaviour
{
    #region Public properties
    [SerializeField]
    [Tooltip("The hours hand of the clock gameobject")]
    private GameObject hoursHand;

    [SerializeField]
    [Tooltip("The minutes hand of the clock gameobject")]
    private GameObject minutesHand;
    #endregion

    #region Private fields and properties
    private CustomLightManager lmScript;

    private float currentHoursHandRotationAngle;
    private float previousHoursHandRotationAngle;
    private float currentTimeInDecimals;

    private float _timer = 60; //Timer used to add the angle offset for auto rotation of sun/moon;
    #endregion

    #region Accessors for private fields
    public float CurrentTimeInDecimals
    {
        get
        {
            return currentTimeInDecimals;
        }
    }

    public float CurrentHoursHandRotationAngle
    {
        get
        {
            return currentHoursHandRotationAngle;
        }
    }

    public float PreviousHoursHandRotationAngle
    {
        get
        {
            return previousHoursHandRotationAngle;
        }
        
        set
        {
            previousHoursHandRotationAngle = value;
        } 
    }
    #endregion

    #region Unity Engine methods
    private void Awake()
    {
        lmScript = GetComponent<CustomLightManager>();
    }
    // Start is called before the first frame update
    void Start()
    {
        ClockManagerOnStart();
        GameManager.Instance.OnDayLightSet.AddListener(ClockManagerOnStart);
    }

    // Update is called once per frame
    void Update()
    {
        int hours;
        int minutes;

        if(lmScript.useCustomLight)
        {
            //This part adds the angle that the hours hand does in a 60-second time interval to add a realistic time flow in the scene
            if (_timer <= 0)
            {
                hoursHand.transform.localEulerAngles = new Vector3(hoursHand.transform.localEulerAngles.x, hoursHand.transform.localEulerAngles.y, hoursHand.transform.localEulerAngles.z+0.25f);
                _timer = 60;
            }

            currentHoursHandRotationAngle = hoursHand.transform.localEulerAngles.z;
            //Debug.Log("Sine: " + Mathf.Sin(currentHoursHandRotationAngle*(Mathf.PI/180)) + ", cosine: " + Mathf.Cos(currentHoursHandRotationAngle * (Mathf.PI / 180)));

            currentTimeInDecimals = timeFromRotation(currentHoursHandRotationAngle);
            lmScript.FloatToHoursMinutes(currentTimeInDecimals, out hours, out minutes);

            RotateMinuteHand(minutes);

            _timer -= Time.deltaTime;
            //print(_timer);
        }
    }
    #endregion

    #region Class custom methods
    /*
     * It rotates the hour hand according to a value
     * @param {int} angle - The hour value the hand must orient to in the watchface
     */
    public void RotateHourHand(int angle)
    {
        hoursHand.transform.localEulerAngles = new Vector3(hoursHand.transform.rotation.x, hoursHand.transform.rotation.y+180, angle * (360/12));
    }

    /*
     * It rotates the minutes hand according to a value
     * @param {int} angle - The minute value the hand must orient to in the watchface
     */
    public void RotateMinuteHand(int angle)
    {
        minutesHand.transform.localEulerAngles = new Vector3(0, 0, angle * (360 / 60));
    }

    /*
     * It rotates both the hour and the minute hands according to a value
     * @param {int} hour - The hour value the hand must orient to in the watchface
     * @param {int} minute - The minute value the hand must orient to in the watchface
     */
    public void RotateClockHands(int hour, int minute)
    {
        hoursHand.transform.localEulerAngles = new Vector3(hoursHand.transform.rotation.x, hoursHand.transform.rotation.y, hour * (360 / 12) + (0.5f*minute));
        minutesHand.transform.localEulerAngles = new Vector3(minutesHand.transform.rotation.x, minutesHand.transform.rotation.y + 180, minute * (360 / 60));
    }

    /*
     * It calculate the hour hand value from an angle, in the range [0, 12], with decimals (base 10)
     * @param {float} value - The angle value
     * @out {float} value - The hour hand position in the 12-hour system
     */
    public float timeFromRotation(float value)
    {
        return value * 12f / 360f;
    }

    public void ClockManagerOnStart()
    {
        if (lmScript.useCustomLight)
        {
            RotateClockHands(DateTime.Now.Hour % 12, DateTime.Now.Minute);
            currentHoursHandRotationAngle = hoursHand.transform.rotation.eulerAngles.z;
            previousHoursHandRotationAngle = currentHoursHandRotationAngle;
            GameManager.Instance.OnDayLightSet.RemoveListener(ClockManagerOnStart);
        }
    }
    #endregion
}
