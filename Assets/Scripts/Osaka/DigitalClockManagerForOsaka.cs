/************************************************************************************
* 
* Class Purpose: the class handles any interaction with the digital clock system,
*           including input (time changing) and output (correct digital clock display)
*
************************************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using System.Runtime.CompilerServices;
using Bolt;
using Ludiq;

public class DigitalClockManagerForOsaka : MonoBehaviour
{
    #region Properties and public fields
    [Tooltip("The TMP object in scene where output the time string to.")]
    [HideInInspector]
    public TMP_Text TMPComponent;

    [Tooltip("The TMP object in scene where output the column string to. It refers to second in the digital clock display (\":\")")]
    [HideInInspector]
    public TMP_Text TMPColumnComponent;

    [Tooltip("The step in minutes for the increase and decrease of minutes in the clock interface. If 0, time won't be increased.")]
    [Min(0)]
    public int timeStep = 1;

    [Tooltip("The lenght of a full day cycle, in minutes. If = 1440 (= 24h), the cycle will be realtime. If < 1440, the cycle will be faster. If > 1440, the cycle will be slower")]
    [Min(1)]
    public int dayLength = 1;

    [Tooltip("The current step will be multiplied by the multiplier value every second the button is being pressed")]
    [Min(1)]
    [HideInInspector]
    public int longPressMultiplier = 7;

    [Tooltip("The increase button object")]
    [HideInInspector]
    public GameObject increaseButton;

    [Tooltip("The decrease button object")]
    [HideInInspector]
    public GameObject decreaseButton;

    [Header("Audio Sources")]
    [Tooltip("Digital Clock audio source")]
    [HideInInspector]
    public AudioSource clockAudioSource;

    [Tooltip("Increase button audio clip")]
    [HideInInspector]
    public AudioClip increaseAudioClip;

    [Tooltip("Decrease button audio clip")]
    [HideInInspector]
    public AudioClip decreaseAudioClip;


    public float CurrentTime
    {
        get => _timeOfDay;

        private set
        {
            _timeOfDay = value;
        }
    }

    public CustomLightManagerForOsaka.Period CurrentPeriod
    {
        get => _period;

        private set
        {
            _period = value;
        }
    }

    public float increaseExpMult
    {
        get => expMult1;
    }

    public float decreaseExpMult
    {
        get => expMult2;
    }
    #endregion

    #region Fields
    private CustomLightManagerForOsaka lmScript;

    private int _hours;
    private int _minutes;
    private float _secondsTimer = 0f;
    private float _timeOfDay;
    private CustomLightManagerForOsaka.Period _period;
    private bool _isLongIncreasePressed = false;
    private bool _isLongDecreasePressed = false;
    private float _increaseButtonTimer = 0f;
    private float _decreaseButtonTimer = 0;
    private float expMult1 = 1f;
    private float expMult2 = 1f;
    [HideInInspector] public float minutesPerCycle;
    #endregion

    #region Unity Engine methods
    private void Awake()
    {
        //Osaka is in Japan and the time zone there is UTC+9

        lmScript = GetComponent<CustomLightManagerForOsaka>();
        _hours = DateTime.UtcNow.Hour + 9;
        _minutes = DateTime.UtcNow.Minute;
        minutesPerCycle = 1440f / (float) dayLength;
    }
    // Start is called before the first frame update
    void Start()
    {
        //GameManager.Instance.OnDayLightSet.AddListener(ClockManagerOnStart);
        //StartCoroutine(DisplayBlinkingTime());

    }

    // Update is called once per frame
    void Update()
    {
        if (lmScript.useCustomLight)
        {
            //Automatic time increase according to real seconds passing
            if (_secondsTimer >= 1f)
            {
                _minutes += Mathf.FloorToInt(minutesPerCycle) / 60;
                _secondsTimer = 0f;
            }

            //Checking for clockwise and unclockwise cycles. Why putting it here? Minutes are changed in many points, so before visualizing and memorizing actual time, we should check it and set them
            ClockwiseCycleCheck();
            UnclockwiseCycleCheck();

            _period = _hours >= 12 ? CustomLightManagerForOsaka.Period.PM : CustomLightManagerForOsaka.Period.AM;

            //Formatting output string and memorizing actual time to be read by other scripts
            //timeDisplayFormatter();

            //We started using coroutines to check for button press and time increasing, but the system was not solid enought causing spurious values to be detected.
            //So we tried to use the old and not elegant Update() for every check. Not performant, but it works better and fortunately calculus aren't so cycle expensive
            //if((bool)Variables.Object(increaseButton).Get("Pressed"))
            //{
            //    if (_increaseButtonTimer > 1.1f)
            //    {
            //        IncreaseTime(Mathf.FloorToInt(expMult1));
            //        timeDisplayFormatter();
            //        expMult1 += 0.035f;

            //        if (!clockAudioSource.isPlaying)
            //        {
            //            clockAudioSource.Play();
            //        }
            //    } else
            //    {
            //        _increaseButtonTimer += Time.deltaTime;
            //    }
            //} else
            //{
            //    _increaseButtonTimer = 0;
            //    expMult1 = 1f;
            //}

            //if ((bool)Variables.Object(decreaseButton).Get("Pressed"))
            //{
            //    if (_decreaseButtonTimer > 1.1f)
            //    {
            //        DecreaseTime(Mathf.FloorToInt(expMult2));
            //        timeDisplayFormatter();
            //        expMult2 += 0.035f;

            //        if (!clockAudioSource.isPlaying)
            //        {
            //            clockAudioSource.Play();
            //        }
            //    }
            //    else
            //    {
            //        _decreaseButtonTimer += Time.deltaTime;
            //    }
            //}
            //else
            //{
            //    _decreaseButtonTimer = 0;
            //    expMult2 = 1f;
            //}

            //Formatting output string and memorizing actual time to be read by other scripts
            _timeOfDay = lmScript.HoursMinutesToFloat(_hours, _minutes);

            _secondsTimer += Time.deltaTime;
        }
    }
    #endregion

    #region Init method
    private void ClockManagerOnStart()
    {
        if (lmScript.useCustomLight)
        {
            _hours = DateTime.Now.Hour;
            _minutes = DateTime.Now.Minute;
            GameManager.Instance.OnDayLightSet.RemoveListener(ClockManagerOnStart);
        }
    }
    #endregion

    #region Display methods
    /*
     * The method display the time in the format "hh : mm"
     * No parameters need because the current values of hours and minutes are taken
     */
    private void timeDisplayFormatter()
    {
        ClockwiseCycleCheck();
        UnclockwiseCycleCheck();
        TMPComponent.text = DigitFormatter(_hours) + "   " + DigitFormatter(_minutes);
    }

    /*
     * The method display the time in the format "hh : mm"
     * @param {int} hours - The hours value to display
     * @param {int} minutes - The minutes value to display
     */
    private void timeDisplayFormatter(int hours, int minutes)
    {
        TMPComponent.text =  DigitFormatter(hours) + "   " + DigitFormatter(minutes);
    }

    /*
     * The method display the time in the format "hh : mm", with the column symbol blinking every second
     * @param {int} hours - The hours value to display
     * @param {int} minutes - The minutes value to display
     */
    IEnumerator DisplayBlinkingTime(int hours, int minutes)
    {
        TMPComponent.text = DigitFormatter(hours) + " : " + DigitFormatter(minutes);
        yield return new WaitForSeconds(0.5f);
        TMPComponent.text = DigitFormatter(hours) + "   " + DigitFormatter(minutes);
        yield return new WaitForSeconds(0.5f);
    }

    /*
     * The method display the time in the format "hh : mm", with the column symbol blinking every second
     * No parameters need because the current values of hours and minutes are taken
     */
    IEnumerator DisplayBlinkingTime()
    {
        while (Application.isPlaying)
        {
            //ClockwiseCycleCheck();
            //UnclockwiseCycleCheck();
            //TMPComponent.text = DigitFormatter(_hours) + " : " + DigitFormatter(_minutes);
            TMPColumnComponent.text = ":";
            yield return new WaitForSeconds(0.5f);
            //ClockwiseCycleCheck();
            //UnclockwiseCycleCheck();
            //TMPComponent.text = DigitFormatter(_hours) + "   " + DigitFormatter(_minutes);
            TMPColumnComponent.text = "";
            yield return new WaitForSeconds(0.5f);
        }
    }

    /*
     * Utils method to correctly display the digit in a text field. If value is lower than 10, it's always formatted to be double digit (e.g: 9 -> 09)
     * @param {int} val - The digit to format
     * @out {string} - The number val in string type correctly formatted
     */
    private string DigitFormatter(int val)
    {
        if (val <= 9)
        {
            return "0" + val.ToString();
        }
        else
        {
            return val.ToString();
        }
    }

    //The method checks minute and hour values clockwise to check for cycle and increasing values if necessary
    private void ClockwiseCycleCheck()
    {
        if (_minutes >= 60)
        {
            _minutes -= 60;
            _hours++;
        }

        if (_hours >= 24)
        {
            _hours -= 24;
        }
    }

    //The method checks minute and hour values unclockwise to check for cycle and decreasing values if necessary
    private void UnclockwiseCycleCheck()
    {
        if (_minutes < 0)
        {
            _minutes += 60;
            _hours--;
        }

        if (_hours < 0)
        {
            _hours += 24;
        }
    }
    #endregion

    #region Public time modifier methods
    // Increase the minutes field by one
    public void IncreaseTime()
    {
        _minutes++;

        ClockwiseCycleCheck();
    }

    // Decrease the minutes field by one
    public void DecreaseTime()
    {
        _minutes--;

        UnclockwiseCycleCheck();
    }

    /* Increase the minutes field by the predefined step value multiplied by a multiplier for fast time increase
     * @param {int} multiplier - The multiplier value
     */
    public void IncreaseTime(int multiplier)
    {
        _minutes += timeStep*multiplier;

        ClockwiseCycleCheck();

        if (_isLongIncreasePressed && !clockAudioSource.isPlaying)
        {
            clockAudioSource.Play();
        }
    }

    /* Decrease the minutes field by the predefined step value multiplied by a multiplier for fast time decrease
     * @param {int} multiplier - The multiplier value
     */
    public void DecreaseTime(int multiplier)
    {
        _minutes -= timeStep*multiplier;

        UnclockwiseCycleCheck();

        if (_isLongDecreasePressed && !clockAudioSource.isPlaying)
        {
            clockAudioSource.Play();
        }
    }

    /* Increase the minutes field by the chosen step value multiplied by a multiplier for fast time increase
     * @param {int} step - The base step value to add to minutes
     * @param {int} multiplier - The multiplier value
     */
    public void IncreaseTime(int step, int multiplier)
    {
        _minutes += step * multiplier;

        ClockwiseCycleCheck();
    }

    /* Decrease the minutes field by the chosen step value multiplied by a multiplier for fast time decrease
     * @param {int} step - The base step value to subtrac to minutes
     * @param {int} multiplier - The multiplier value
     */
    public void DecreaseTime(int step, int multiplier)
    {
        _minutes -= step * multiplier;

        UnclockwiseCycleCheck();
    }

    // Prerequisite: the interaction occurs with a button object.
    // Method called when the increase button has just been pressed: it calls all propers coroutine to start checking the single or long pression
    public void SetLongIncreasePressTrue()
    {
        clockAudioSource.clip = increaseAudioClip;
        //_isLongIncreasePressed = true;
        _minutes = FindNearestFiveClockwise(_minutes);
        //StartCoroutine(OnButtonPressIncrease());
    }

    // Prerequisite: the interaction occurs with a button object.
    // Method called when the increase button has just been released: it calls all propers coroutine to stop checking the single or long pression
    public void SetLongIncreasePressFalse()
    {
        //StopCoroutine(OnButtonPressIncrease());
        //_isLongIncreasePressed = false;
        
        if(clockAudioSource.isPlaying)
        {
            clockAudioSource.Stop();
        }
    }

    // Prerequisite: the interaction occurs with a button object.
    // Method called when the decrease button has just been pressed: it calls all propers coroutine to start checking the single or long pression
    public void SetLongDecreasePressTrue()
    {
        clockAudioSource.clip = decreaseAudioClip;
        
        if (_minutes == 0)
        {
            DecreaseTime();
        }

        _minutes = FindNearestFiveUnclockwise(_minutes);

        //_isLongDecreasePressed = true;
        //StartCoroutine(OnButtonPressDecrease());
    }

    // Prerequisite: the interaction occurs with a button object.
    // Method called when the decrease button has just been released: it calls all propers coroutine to stop checking the single or long pression
    public void SetLongDecreasePressFalse()
    {
        //StopCoroutine(OnButtonPressDecrease());
        //_isLongDecreasePressed = false;

        if (clockAudioSource.isPlaying)
        {
            clockAudioSource.Stop();
        }
    }
    #endregion

    #region UI buttons methods
    // Prerequisite: the interaction occurs with a button object.
    // Coroutine: Method called when the increase button is being pressed: it executes the increasing methods depending on whether the button is pushed once or longer
    IEnumerator OnButtonPressIncrease()
    {
        _minutes = FindNearestQuarterClockwise(_minutes);
        yield return new WaitForSeconds(.5f);

        float expMult = 0.5f;

        while (_isLongIncreasePressed)
        {
            yield return new WaitForSeconds(.5f / expMult);
            IncreaseTime(1);
            timeDisplayFormatter();
            expMult += 0.4f;
        }
    }

    // Prerequisite: the interaction occurs with a button object.
    // Coroutine: Method called when the decrease button is being pressed: it executes the increasing methods depending on whether the button is pushed once or longer
    IEnumerator OnButtonPressDecrease()
    {
        if (_minutes == 0)
        {
            DecreaseTime();
        }

        _minutes = FindNearestQuarterUnclockwise(_minutes);
        yield return new WaitForSeconds(.5f);

        float expMult = 1f;

        while (_isLongDecreasePressed)
        {
            DecreaseTime(1);
            timeDisplayFormatter();
            expMult += 0.4f;
            yield return new WaitForSeconds(.5f / expMult);
        }
    }
    #endregion

    #region Util methods
    /* The method find the closest clockwise time quarter value to the input value
     * @param {int} minute - The value according to find the nearest quarter value clockwise
     * @out {int} - The closest quarter value if found, the minute value passed as input otherwise
     */
    private int FindNearestQuarterClockwise(int minute)
    {
        int[] quarterList = { 15, 30, 45, 60 };
        
        for (int i = 0; i < quarterList.Length; i++)
        {
            if (minute < quarterList[i])
            {
                return quarterList[i];
            }
        }

        return minute;
    }

    /* The method find the closest unclockwise time quarter value to the input value
     * @param {int} minute - The value according to find the nearest quarter value unclockwise
     * @out {int} - The closest quarter value if found, the minute value passed as input otherwise
     */
    private int FindNearestQuarterUnclockwise(int minute)
    {

        int[] quarterList = { 0, 15, 30, 45 };

        for (int i = quarterList.Length-1; i >= 0; i--)
        {
            if (minute > quarterList[i])
            {
                return quarterList[i];
            }
        }

        return minute;
    }

    /* The method find the closest clockwise time five value to the input value
     * @param {int} minute - The value according to find the nearest quarter value clockwise
     * @out {int} - The closest five value if found, the minute value passed as input otherwise
     */
    private int FindNearestFiveClockwise(int minute)
    {
        int[] fiveList = { 5, 10, 15, 20, 25, 30, 35, 40, 45, 50, 55, 60 };

        for (int i = 0; i < fiveList.Length; i++)
        {
            if (minute < fiveList[i])
            {
                return fiveList[i];
            }
        }

        return minute;
    }

    /* The method find the closest unclockwise time five value to the input value
     * @param {int} minute - The value according to find the nearest quarter value unclockwise
     * @out {int} - The closest five value if found, the minute value passed as input otherwise
     */
    private int FindNearestFiveUnclockwise(int minute)
    {

        int[] fiveList = { 0, 5, 10, 15, 20, 25, 30, 35, 40, 45, 50, 55 };

        for (int i = fiveList.Length - 1; i >= 0; i--)
        {
            if (minute > fiveList[i])
            {
                return fiveList[i];
            }
        }

        return minute;
    }
    #endregion
}
