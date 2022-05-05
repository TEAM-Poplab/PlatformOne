/************************************************************************************
* 
* Class Purpose: control the fog settings parameters for day/night transitions
*
************************************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class FogManagerForOsaka : MonoBehaviour
{
    [Header("Animation Curves for Fog Density")]
   /* [Tooltip("Set the animation curve for the fog density during the day-to-night animation")]
    public AnimationCurve dayToNightFogDensityAnimation;

    [Tooltip("Set the animation curve for the fog density during the night-to-day animation")]
    public AnimationCurve nightToDayFogDensityAnimation;*/

    [Tooltip("Set the animation curve for the fog color blending during the day. 0 is dawn time, 1 is sunset time. A value of 0 is night fog color, a value of 1 is day fog color. Used only when custom light is ON")]
    public AnimationCurve fogColorBlendingAnimation;

    private float TimeOfDay;
    private float prevTimeOfDay;
    private float stepTimeOfDay;
    private float cumulativeDeltaTime;

    private bool _onDayToNight = false;

    private bool _onNightToDay = false;

    private float _timer = 0;

    private Color _originalFogColor;

    private CustomLightManagerForOsaka lmScript;
    private DigitalClockManagerForOsaka dcmScript;

    [Header("Fog Color on light change")]
    public Color fogColorOnDay;
    public Color fogColorOnNight;

    // Start is called beforethe first frame update
    void Start()
    {
        /*dayToNightFogDensityAnimation.preWrapMode = WrapMode.Once;
        dayToNightFogDensityAnimation.postWrapMode = WrapMode.Once;
        nightToDayFogDensityAnimation.preWrapMode = WrapMode.Once;
        nightToDayFogDensityAnimation.postWrapMode = WrapMode.Once;*/
        fogColorBlendingAnimation.preWrapMode = WrapMode.Once;
        fogColorBlendingAnimation.postWrapMode = WrapMode.Once;
        _originalFogColor = RenderSettings.fogColor;
        lmScript = GetComponent<CustomLightManagerForOsaka>();
        dcmScript = GetComponent<DigitalClockManagerForOsaka>();
        TimeOfDay = lmScript.TimeOfDay;
        FogManagerOnStart();
        //GameManagerOsaka.Instance.OnDayLightSet.AddListener(FogManagerOnStart);
        //GameManagerOsaka.Instance.OnNightLightSet.AddListener(FogManagerOnStart);
    }

    // Update is called once per frame
    void Update()
    {
        //if (TimeOfDay == prevTimeOfDay)
        //{
        //    stepTimeOfDay = Mathf.Lerp(TimeOfDay, TimeOfDay + ((Mathf.FloorToInt(dcmScript.minutesPerCycle) / 60) * 100f / 6000f), cumulativeDeltaTime);
        //    RenderSettings.fogColor = Color.Lerp(fogColorOnNight, fogColorOnDay, fogColorBlendingAnimation.Evaluate(Mathf.InverseLerp(lmScript.dawnTime, lmScript.duskTime, stepTimeOfDay)));
        //    cumulativeDeltaTime += Time.deltaTime;
        //} else
        //{
        //    cumulativeDeltaTime = 0;
        //    prevTimeOfDay = TimeOfDay;
        //}

        //TimeOfDay = dcmScript.CurrentTime;

        RenderSettings.fogColor = Color.Lerp(fogColorOnNight, fogColorOnDay, fogColorBlendingAnimation.Evaluate(Mathf.InverseLerp(lmScript.dawnTime, lmScript.duskTime, lmScript.TimeOfDay)));
    }

    public void IsDayToNight()
    {
        _onDayToNight = true;
    }

    public void IsNightToDay()
    {
        _onNightToDay = true;
    }

    public void FogManagerOnStart()
    {
        if (!lmScript.useCustomLight)
        {
            //GameManager.Instance.OnDayLightSet.AddListener(IsNightToDay);
            //GameManager.Instance.OnNightLightSet.AddListener(IsDayToNight);
            RenderSettings.fogColor = _originalFogColor;
        }
        else
        {
            //GameManager.Instance.OnDayLightSet.RemoveListener(IsNightToDay);
            //GameManager.Instance.OnNightLightSet.RemoveListener(IsDayToNight);
            RenderSettings.fogColor = Color.Lerp(fogColorOnNight, fogColorOnDay, fogColorBlendingAnimation.Evaluate(Mathf.InverseLerp(lmScript.dawnTime, lmScript.duskTime, lmScript.TimeOfDay)));
        }
    }
}
