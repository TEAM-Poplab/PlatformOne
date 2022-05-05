/************************************************************************************
* 
* Class Purpose: the class controls any event related to time control in the scene
*       and let any other module related to it to being able to communicated with
*       one other
*
************************************************************************************/

//using Microsoft.MixedReality.Toolkit.Experimental.UI.BoundsControlTypes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CustomLightManager : MonoBehaviour
{
    #region Properties
    //[Tooltip("Set if whether or not use the custom light system. MUST be set before runtime")]
    //[HideInInspector]
    public bool useCustomLight = false;

    [Header("General Settings")]
    [Tooltip("Custom sunset time used to determine if it's still day or not")]
    public float duskTime = 18.9f;

    [Tooltip("Custom dawn time used to determine if it's still night or not")]
    public float dawnTime = 4.3f;

    public float offsetDawn = 1f;
    public float offsetDusk = 1f;

    [Tooltip("Value to control how fast the sun goes, so it changes the ratio of actual minute and it corresponding value in the world")]
    public float autoSpeedMultiplier = 1;

    [Tooltip("The value of a virtual world second for a real world second")]
    public int worldSecondsRatio = 1;

    [Header("Animation parameters")]
    [Tooltip("Animation curve for sunlight intensity animation. 0 is dawn time and 1 is sunset time")]
    [SerializeField]
    private AnimationCurve _sunIntensityAnimation;

    [Tooltip("Animation curve for sunlight intensity animation. 0 is sunset time and 1 is dawn time")]
    [SerializeField]
    private AnimationCurve _moonIntensityAnimation;
    
    [Tooltip("Animation curve for Enviroment Light intensity multiplier animation. 0 is 0.00, 0.5 is 12 and 1 is 24.00")]
    [SerializeField]
    private AnimationCurve _environmentIntensityAnimation;

    [Header("Light settings")]
    [Tooltip("The color of the custom directional light when it's at dawn/sunset time")]
    [SerializeField]
    private Color sunColorAtSunset;

    [Tooltip("Animation curve for sunlight color animation. 0 is sunset time and 1 is dawn time. A value of 1 is the original light color, a value of 0 is the color set above")]
    [SerializeField]
    private AnimationCurve _sunColorAnimation;

    //[Range(0, 24)]
    //[Tooltip("Inspector slider to control the scene time")]
    [HideInInspector]
    public float TimeOfDay;

    [HideInInspector]
    public Period currentPeriod;

    public Light CustomDirectionalLight
    {
        get => _customDirectionalLight;
        private set => _customDirectionalLight = value;
    }

    public enum Period
    {
        AM,
        PM
    }
    #endregion

    #region Private fields and variables
    private Light _customDirectionalLight;
    private Light _customNightDirectionalLight;
    private Quaternion baseCustomDirectionalLightRotation;
    private Color _customDirectionalLightColor;
    private GameObject _sunObject;
    private Color _sunOriginalColor;
    private List<GameObject> _haloList = new List<GameObject>();
    private Color _haloOriginalColor;
    private Quaternion savedRotAngle;


    private float prevTimeOfDay;
    private Period previousPeriod;
    private float cumulativeDeltaTime;
    private float stepTimeOfDay;

    private ClockManager cmScript;
    private DigitalClockManager dcmScript;

    private const float TIME_UNIT_FOR_ANGLE = 360f / 24f;
    private float angleForRealTimeUnit = 360f / 86400f;     //Base value: 1 second in real world = 1 second in virtual world
    #endregion

    #region Unity Engine methods
    private void Awake()
    {
        TimeOfDay = HoursMinutesToFloat(DateTime.Now.Hour, DateTime.Now.Minute);
        currentPeriod = DeterminePeriodFromTime(DateTime.Now.Hour);
    }

    // Start is called before the first frame update
    void Start()
    {
        //DontDestroyOnLoad(gameObject);
        _customDirectionalLight = GameObject.Find("Custom Directional Light").GetComponent<Light>();
        _customNightDirectionalLight = GameObject.Find("Custom Directional Light/Night Custom Directional Light").GetComponent<Light>();
        _sunObject = GameObject.Find("Custom Directional Light/Sun");
        
        foreach(Transform child in _sunObject.transform)
        {
            _haloList.Add(child.gameObject);
        }

        _haloOriginalColor = _haloList[0].GetComponent<SpriteRenderer>().color;

        baseCustomDirectionalLightRotation = _customDirectionalLight.transform.rotation;
        _customDirectionalLightColor = _customDirectionalLight.color;
        _sunOriginalColor = _sunObject.GetComponent<MeshRenderer>().material.color;
        cmScript = GetComponent<ClockManager>();
        dcmScript = GetComponent<DigitalClockManager>();

        TimeOfDay = DateTime.Now.Hour + DateTime.Now.Minute * (60 / 100) / 100;

        //CustomLightOnStart();
        _customDirectionalLight.gameObject.SetActive(false);
        GameManager.Instance.OnDayLightSet.AddListener(CustomLightOnStart);
        GameManager.Instance.OnNightLightSet.AddListener(ResumeCustomLight);
    }

    // Update is called once per frame
    void Update()
    {
        if (useCustomLight)
        {
            if (autoSpeedMultiplier != 0)
            {
                RotateSunByAngle();
            }// else
            //{
            //    RotateSunByAngle2();
            //}

            //Calculate current time using angles
            var angle = Vector3.SignedAngle(Vector3.left, _customDirectionalLight.transform.GetChild(2).position, Vector3.back);
            if (angle >= 0 && angle < 180)
            {
                var t = Mathf.InverseLerp(0, 180f, angle);
                TimeOfDay = Mathf.Lerp(dawnTime, duskTime, t);
            }
            else if (angle < -90)
            {
                var t = Mathf.InverseLerp(90, 180, Mathf.Abs(angle));
                TimeOfDay = Mathf.Lerp(24, duskTime, t);
            }
            else
            {
                var t = Mathf.InverseLerp(0, 90, Mathf.Abs(angle));
                TimeOfDay = Mathf.Lerp(dawnTime, 0, t);
            }

            _customDirectionalLight.intensity = _sunIntensityAnimation.Evaluate(Mathf.InverseLerp(dawnTime, duskTime, TimeOfDay));
            if (TimeOfDay > duskTime)
            {
                _customNightDirectionalLight.intensity = _moonIntensityAnimation.Evaluate(Mathf.InverseLerp(duskTime, 24, TimeOfDay) * 0.5f);
            }
            else if (TimeOfDay < dawnTime)
            {
                _customNightDirectionalLight.intensity = _moonIntensityAnimation.Evaluate(Mathf.InverseLerp(0, dawnTime, TimeOfDay) * 0.5f + 0.5f);
            }
            RenderSettings.ambientIntensity = _environmentIntensityAnimation.Evaluate(Mathf.InverseLerp(0, 24, TimeOfDay));

            if (prevTimeOfDay < duskTime + offsetDusk && TimeOfDay >= duskTime + offsetDusk)
            {
                RenderSettings.sun = _customNightDirectionalLight;
            }
            else if (prevTimeOfDay >= dawnTime + offsetDawn && TimeOfDay < dawnTime + offsetDawn)
            {
                RenderSettings.sun = _customNightDirectionalLight;
            }
            else if (prevTimeOfDay >= duskTime + offsetDusk && TimeOfDay < duskTime + offsetDusk)
            {
                RenderSettings.sun = _customDirectionalLight;
            }
            else if (prevTimeOfDay < dawnTime + offsetDawn && TimeOfDay >= dawnTime + offsetDawn)
            {
                RenderSettings.sun = _customDirectionalLight;
            }

            _customDirectionalLight.color = Color.Lerp(sunColorAtSunset, _customDirectionalLightColor, _sunColorAnimation.Evaluate(Mathf.InverseLerp(dawnTime, duskTime, TimeOfDay)));
            _sunObject.GetComponent<MeshRenderer>().material.color = Color.Lerp(sunColorAtSunset, _sunOriginalColor, _sunColorAnimation.Evaluate(Mathf.InverseLerp(dawnTime, duskTime, TimeOfDay)));

            foreach (GameObject sprite in _haloList)
            {
                sprite.GetComponent<SpriteRenderer>().color = Color.Lerp(sunColorAtSunset, _haloOriginalColor, _sunColorAnimation.Evaluate(Mathf.InverseLerp(dawnTime, duskTime, TimeOfDay)));
            }

            prevTimeOfDay = TimeOfDay;
            Debug.LogWarning($"{TimeOfDay} {Mathf.FloorToInt(TimeOfDay)} {Mathf.FloorToInt(Mathf.Lerp(0, 60, TimeOfDay - Mathf.Floor(TimeOfDay)))}");
        }

        /******* ORIGINAL SCRIPT *************
        if (useCustomLight && TimeOfDay != prevTimeOfDay)
        {
            RotateSun(TimeOfDay);

            
            //if (Mathf.Sin(cmScript.CurrentHoursHandRotationAngle * (Mathf.PI/180)) < 0f && Mathf.Sin(cmScript.PreviousHoursHandRotationAngle * (Mathf.PI / 180)) >= 0f && Mathf.Cos(cmScript.PreviousHoursHandRotationAngle * (Mathf.PI / 180)) >= 0.5f)
            //{
            //    switch (previousPeriod)
            //    {
            //        case Period.PM:
            //            currentPeriod = Period.AM;
            //            break;
            //        case Period.AM:
            //            currentPeriod = Period.PM;
            //            break;
            //    }
            //    previousPeriod = currentPeriod;
            //} else if (Mathf.Sin(cmScript.CurrentHoursHandRotationAngle * (Mathf.PI / 180)) >= 0f && Mathf.Sin(cmScript.PreviousHoursHandRotationAngle * (Mathf.PI / 180)) < 0 && Mathf.Cos(cmScript.PreviousHoursHandRotationAngle * (Mathf.PI / 180)) >= 0.5f)
            //{
            //    switch (previousPeriod)
            //    {
            //        case Period.AM:
            //            currentPeriod = Period.PM;
            //            break;
            //        case Period.PM:
            //            currentPeriod = Period.AM;
            //            break;
            //    }
            //    previousPeriod = currentPeriod;
            //}
            

            _customDirectionalLight.intensity = _sunIntensityAnimation.Evaluate(Mathf.InverseLerp(dawnTime, duskTime, TimeOfDay));
            if (TimeOfDay>duskTime)
            {
                _customNightDirectionalLight.intensity = _moonIntensityAnimation.Evaluate(Mathf.InverseLerp(duskTime, 24, TimeOfDay)*0.5f);
            } else if (TimeOfDay<dawnTime)
            {
                _customNightDirectionalLight.intensity = _moonIntensityAnimation.Evaluate(Mathf.InverseLerp(0, dawnTime, TimeOfDay) * 0.5f + 0.5f);
            }
            RenderSettings.ambientIntensity = _environmentIntensityAnimation.Evaluate(Mathf.InverseLerp(0, 24, TimeOfDay));

            if (prevTimeOfDay<duskTime+offsetDusk && TimeOfDay>= duskTime+ offsetDusk)
            {
                RenderSettings.sun = _customNightDirectionalLight;
            } else if (prevTimeOfDay >= dawnTime+offsetDawn && TimeOfDay<dawnTime+ offsetDawn)
            {
                RenderSettings.sun = _customNightDirectionalLight;
            } else if (prevTimeOfDay>= duskTime+ offsetDusk && TimeOfDay < duskTime+ offsetDusk)
            {
                RenderSettings.sun = _customDirectionalLight;
            } else if (prevTimeOfDay<dawnTime+ offsetDawn && TimeOfDay>=dawnTime+ offsetDawn)
            {
                RenderSettings.sun = _customDirectionalLight;
            }

            _customDirectionalLight.color = Color.Lerp(sunColorAtSunset, _customDirectionalLightColor, _sunColorAnimation.Evaluate(Mathf.InverseLerp(dawnTime, duskTime, TimeOfDay)));
            _sunObject.GetComponent<MeshRenderer>().material.color = Color.Lerp(sunColorAtSunset, _sunOriginalColor, _sunColorAnimation.Evaluate(Mathf.InverseLerp(dawnTime, duskTime, TimeOfDay)));
            
            foreach(GameObject sprite in _haloList)
            {
                sprite.GetComponent<SpriteRenderer>().color = Color.Lerp(sunColorAtSunset, _haloOriginalColor, _sunColorAnimation.Evaluate(Mathf.InverseLerp(dawnTime, duskTime, TimeOfDay)));
            }

            prevTimeOfDay = TimeOfDay;
            //cmScript.PreviousHoursHandRotationAngle = cmScript.CurrentHoursHandRotationAngle;
        }

        //TimeOfDay = currentPeriod == Period.AM ? cmScript.CurrentTimeInDecimals : cmScript.CurrentTimeInDecimals + 12f;
        currentPeriod = dcmScript.CurrentPeriod;
        TimeOfDay = dcmScript.CurrentTime;
        ********** END OF ORIGINAL SCRIPT **********/
    }
    #endregion

    #region Custom class methods and functions
    /*
     * Calculate the linear interpolation of a value in the range of the 24-hour cycle.
     * @param {float} value - The interpolant parameter within the range [0, 24]
     * @out {float} normalizedValue - the linear parameter which produces value in the linear range
     */
    private float InverseLerpWithHours(float value)
    {
        float normalizedValue = 0;

        switch (value)
        {
            case float f when (f <= 12f):
                normalizedValue = Mathf.InverseLerp(0f, 12f, value) * (-2) + 1f;
                break;
            case float f when (f > 12f):
                normalizedValue = Mathf.InverseLerp(12f, 24f, value) * 2 - 1f;
                break;
        }

        return normalizedValue;
    }

    /*
     * Rotate the custom light according to the hour
     * @param {float} angle - The hour (~ the angle) according to rotate the light
     */
    private void RotateSun(float angle)
    {
        _customDirectionalLight.transform.rotation = Quaternion.Euler(baseCustomDirectionalLightRotation.eulerAngles.x - TIME_UNIT_FOR_ANGLE * angle, baseCustomDirectionalLightRotation.eulerAngles.y, baseCustomDirectionalLightRotation.eulerAngles.z);
    }

    /// <summary>
    /// Rotate the custom light according to the angle correspoding to time unit. Same versione as <see cref="RotateSun(float)"/>, but more direct angle value driven
    /// </summary>
    private void RotateSunByAngle()
    {
        _customDirectionalLight.transform.Rotate(new Vector3(- (angleForRealTimeUnit) * Time.deltaTime * autoSpeedMultiplier, 0, 0));

    }

    private void RotateSunByAngle2()
    {
        _customDirectionalLight.transform.Rotate(new Vector3(-(angleForRealTimeUnit * worldSecondsRatio) * Time.deltaTime, 0, 0));

    }

    public void RotateSunByAngleFromSeconds(int specifiedSeconds)
    {
        _customDirectionalLight.transform.Rotate(new Vector3(-(angleForRealTimeUnit * specifiedSeconds), 0, 0));

    }

    /*
     * Convert the couple (hours, minutes) into a float number to be used for the light rotation
     * @param {int} hours - The hour value
     * @param {int} minutes - The minute value
     * @out {float} - The generated float value
     */
    public float HoursMinutesToFloat(int hours, int minutes)
    {
        float fminutes = (float)minutes * 100f / 6000f;
        return (float)hours + fminutes;
    }

    /*
     * Convert the float value used for the light rotation into the pair (hours, minutes)
     * @param {float} value - The float value to convert
     * @out {int} hours - The hours output generated from the value
     * @out {int} minutes - The minutes output generated from the value
     */
    public void FloatToHoursMinutes(float value, out int hours, out int minutes)
    {
        hours = (int)Mathf.Floor(value);
        int decimals = (int)Mathf.Floor(((value - hours) * 100));
        minutes = (int)Mathf.Floor((decimals * 60) / 100);
    }

    /*
     * Calculate if the current day period is AM or PM
     * @param {int} hour - The hour value [0, 24] according to determine the period
     * @out {Period} - The Period value corresponding to the haour passed as input
     */
    public Period DeterminePeriodFromTime(int hour)
    {
        switch (hour / 12)
        {
            case 0:
                return Period.AM;
            case 1:
                return Period.PM;
            case 2:
                return Period.AM;
            default:
                return Period.AM;
        }
    }

    public void CustomLightOnStart()
    {
        if (useCustomLight)
        {
            _customDirectionalLight.gameObject.SetActive(true);
            RenderSettings.sun = _customDirectionalLight;
            RotateSunByAngleFromSeconds(DateTime.Now.Hour*3600 + DateTime.Now.Minute*60);
            //_customDirectionalLight.transform.eulerAngles = new Vector3(270 - angleForRealTimeUnit * TimeOfDay * 3600, -90, 0);

            savedRotAngle = _customDirectionalLight.transform.rotation;

            _customDirectionalLight.intensity = _sunIntensityAnimation.Evaluate(Mathf.InverseLerp(dawnTime, duskTime, TimeOfDay));
            if (TimeOfDay > duskTime)
            {
                _customNightDirectionalLight.intensity = _moonIntensityAnimation.Evaluate(Mathf.InverseLerp(duskTime, 24, TimeOfDay) * 0.5f);
            }
            else if (TimeOfDay < dawnTime)
            {
                _customNightDirectionalLight.intensity = _moonIntensityAnimation.Evaluate(Mathf.InverseLerp(0, dawnTime, TimeOfDay) * 0.5f + 0.5f);
            }
            RenderSettings.ambientIntensity = _environmentIntensityAnimation.Evaluate(Mathf.InverseLerp(0, 24, TimeOfDay));

            if (TimeOfDay >= duskTime || TimeOfDay < dawnTime)
            {
                RenderSettings.sun = _customNightDirectionalLight;
            }
        }

        prevTimeOfDay = TimeOfDay;

        GameManager.Instance.OnDayLightSet.RemoveListener(CustomLightOnStart);
        GameManager.Instance.OnDayLightSet.AddListener(ResumeCustomLight);
    }

    public void ResumeCustomLight()
    {
        if (useCustomLight)
        {
            _customDirectionalLight.gameObject.SetActive(true);
            RenderSettings.sun = _customDirectionalLight;
            _customDirectionalLight.transform.rotation = savedRotAngle;
        } else
        {
            _customDirectionalLight.gameObject.SetActive(false);
            savedRotAngle = _customDirectionalLight.transform.rotation;
        }
    }
    #endregion
}
