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

    /// <summary>
    /// Properly set the fog color according to light position on start
    /// </summary>
    public void FogManagerOnStart()
    {
        if (!lmScript.useCustomLight)
        {
            RenderSettings.fogColor = _originalFogColor;
        }
        else
        {
            RenderSettings.fogColor = Color.Lerp(fogColorOnNight, fogColorOnDay, fogColorBlendingAnimation.Evaluate(Mathf.InverseLerp(lmScript.dawnTime, lmScript.duskTime, lmScript.TimeOfDay)));
        }
    }
}
