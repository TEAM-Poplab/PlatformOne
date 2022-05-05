/************************************************************************************
* 
* Class Purpose: the class manages the control and changes of the skybox according to
*           the hour set by the analog clock
*
************************************************************************************/

using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class SkyboxManagerForOsaka : MonoBehaviour
{
    public Material daySkyboxMaterial;
    public Material nightSkyboxMaterial;
    public Material realityOffSkyboxMaterial;

    public float offset = 1.2f;

    private Material _skyboxBaseMaterial;
    private CustomLightManagerForOsaka lmScript;

    private float dusk;
    private float dawn;

    private void Awake()
    {
        _skyboxBaseMaterial = RenderSettings.skybox;
        lmScript = GetComponent<CustomLightManagerForOsaka>();
    }

    // Start is called before the first frame update
    void Start()
    {
        if (lmScript.useCustomLight)
        {
            if (lmScript.TimeOfDay >= lmScript.duskTime && lmScript.TimeOfDay < lmScript.dawnTime)
            {
                SetSkybox(RenderSettings.skybox, nightSkyboxMaterial);
            }
            else
            {
                SetSkybox(RenderSettings.skybox, daySkyboxMaterial);
            }
        }
        else
        {
            SetSkybox(RenderSettings.skybox, realityOffSkyboxMaterial);
        }

        dusk = lmScript.duskTime;
        dawn = lmScript.dawnTime;

        //GameManager.Instance.OnNightLightSet.AddListener(ResetSkybox);
        //GameManager.Instance.OnDayLightSet.AddListener(SetCurrentBaseSkybox);
    }

    // Update is called once per frame
    void Update()
    {
        if (lmScript.useCustomLight)
        {
            if (lmScript.TimeOfDay <= (dusk + offset) && lmScript.TimeOfDay >= (dusk - offset))
            {
                ChangeSkyboxSettingsByLerp(daySkyboxMaterial, nightSkyboxMaterial, lmScript.TimeOfDay, dusk - offset, dusk + offset);
            }
            else if (lmScript.TimeOfDay <= (dawn + offset) && lmScript.TimeOfDay >= (dawn - offset))
            {
                ChangeSkyboxSettingsByLerp(daySkyboxMaterial, nightSkyboxMaterial, lmScript.TimeOfDay, dawn + offset, dawn - offset);
            }
            else if (lmScript.TimeOfDay > dawn + offset && lmScript.TimeOfDay < dusk - offset)
            {
                SetSkybox(RenderSettings.skybox, daySkyboxMaterial);
            }
            else
            {
                SetSkybox(RenderSettings.skybox, nightSkyboxMaterial);
            }
        }
    }

    /* Copies the values from a skybox B material into a skybox A material
     * @param {Material} skyboxA - The skybox to set values to
     * @param {Material} skyboxB - The skybox to copy values from
     */
    private void SetSkybox(Material skyboxA, Material skyboxB)
    {
        skyboxA.SetFloat("_SunSize", skyboxB.GetFloat("_SunSize"));
        skyboxA.SetFloat("_SunSizeConvergence", skyboxB.GetFloat("_SunSizeConvergence"));
        skyboxA.SetFloat("_AtmosphereThickness", skyboxB.GetFloat("_AtmosphereThickness"));
        skyboxA.SetColor("_SkyTint", skyboxB.GetColor("_SkyTint"));
        skyboxA.SetColor("_GroundColor", skyboxB.GetColor("_GroundColor"));
        skyboxA.SetFloat("_Exposure", skyboxB.GetFloat("_Exposure"));
    }

    /* Interpolates the values of two skyboxes for a smooth transition
     * @param {Material} skyboxA - The first skybox whose values are set as minimum for the lerp interval
     * @param {Material} skyboxB - The second skybox whose values are set as maximum for the lerp interval
     * @param {float} currentTime - The current time to use to find out the interpolant value used in the transition time range
     * @param {float} referenceTimeA - The time value used as minimum value in the transition time range
     * @param {float} referenceTimeB - The time value used as maximum value in the transition time range
     */
    private void ChangeSkyboxSettingsByLerp(Material skyboxA, Material skyboxB, float currentTime, float referenceTimeA, float referenceTimeB)
    {
        RenderSettings.skybox.SetFloat("_SunSize", Mathf.Lerp(skyboxA.GetFloat("_SunSize"), skyboxB.GetFloat("_SunSize"), Mathf.InverseLerp(referenceTimeA, referenceTimeB, currentTime)));
        RenderSettings.skybox.SetFloat("_SunSizeConvergence", Mathf.Lerp(skyboxA.GetFloat("_SunSizeConvergence"), skyboxB.GetFloat("_SunSizeConvergence"), Mathf.InverseLerp(referenceTimeA, referenceTimeB, currentTime)));
        RenderSettings.skybox.SetFloat("_AtmosphereThickness", Mathf.Lerp(skyboxA.GetFloat("_AtmosphereThickness"), skyboxB.GetFloat("_AtmosphereThickness"), Mathf.InverseLerp(referenceTimeA, referenceTimeB, currentTime)));
        RenderSettings.skybox.SetColor("_SkyTint", Color.Lerp(skyboxA.GetColor("_SkyTint"), skyboxB.GetColor("_SkyTint"), Mathf.InverseLerp(referenceTimeA, referenceTimeB, currentTime)));
        RenderSettings.skybox.SetColor("_GroundColor", Color.Lerp(skyboxA.GetColor("_GroundColor"), skyboxB.GetColor("_GroundColor"), Mathf.InverseLerp(referenceTimeA, referenceTimeB, currentTime)));
        RenderSettings.skybox.SetFloat("_Exposure", Mathf.Lerp(skyboxA.GetFloat("_Exposure"), skyboxB.GetFloat("_Exposure"), Mathf.InverseLerp(referenceTimeA, referenceTimeB, currentTime)));
    }

    private void ResetSkybox()
    {
        SetSkybox(RenderSettings.skybox, realityOffSkyboxMaterial);
    }

    private void SetCurrentBaseSkybox()
    {
        RenderSettings.skybox = _skyboxBaseMaterial;
    }
}