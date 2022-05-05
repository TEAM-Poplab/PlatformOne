/************************************************************************************
* 
* Class Purpose: control the fog settings parameters for day/night transitions
*
************************************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class FogManager : MonoBehaviour
{
    [Header("Animation Curves for Fog Density")]
   /* [Tooltip("Set the animation curve for the fog density during the day-to-night animation")]
    public AnimationCurve dayToNightFogDensityAnimation;

    [Tooltip("Set the animation curve for the fog density during the night-to-day animation")]
    public AnimationCurve nightToDayFogDensityAnimation;*/

    [Tooltip("Set the animation curve for the fog color blending during the day. 0 is dawn time, 1 is sunset time. A value of 0 is night fog color, a value of 1 is day fog color. Used only when custom light is ON")]
    public AnimationCurve fogColorBlendingAnimation;

    private bool _onDayToNight = false;

    private bool _onNightToDay = false;

    private float _timer = 0;

    private Color _originalFogColor;

    private CustomLightManager lmScript;

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
        lmScript = GetComponent<CustomLightManager>();
        FogManagerOnStart();
        GameManager.Instance.OnDayLightSet.AddListener(FogManagerOnStart);
        GameManager.Instance.OnNightLightSet.AddListener(FogManagerOnStart);
    }

    // Update is called once per frame
    void Update()
    {
        //if (!lmScript.useCustomLight)
        //{
        //    if (_onDayToNight)
        //    {
        //        _timer += Time.deltaTime;
        //        RenderSettings.fogColor = Color.Lerp(fogChangeColor, Color.black, _timer);
        //        //RenderSettings.fogDensity = dayToNightFogDensityAnimation.Evaluate(_timer);
        //        if (_timer > 1)
        //        {
        //            _onDayToNight = false;
        //            _timer = 0;
        //            GameObject.Find("Water").SetActive(false);
        //        }
        //    }
        //    else if (_onNightToDay)
        //    {
        //        _timer += Time.deltaTime;
        //        RenderSettings.fogColor = Color.Lerp(Color.black, fogChangeColor, _timer);
        //        //RenderSettings.fogDensity = nightToDayFogDensityAnimation.Evaluate(_timer);
        //        if (_timer > 1)
        //        {
        //            _onNightToDay = false;
        //            _timer = 0;
        //        }
        //    }
        //    //RenderSettings.fogColor = Color.black;
        //} else
        //{
        //    RenderSettings.fogColor = Color.Lerp(Color.black, fogChangeColor, dayFogDensityAnimation.Evaluate(Mathf.InverseLerp(lmScript.dawnTime, lmScript.duskTime, lmScript.TimeOfDay)));
        //}

        if (lmScript.useCustomLight)
        {
            RenderSettings.fogColor = Color.Lerp(fogColorOnNight, fogColorOnDay, fogColorBlendingAnimation.Evaluate(Mathf.InverseLerp(lmScript.dawnTime, lmScript.duskTime, lmScript.TimeOfDay)));
        }

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
