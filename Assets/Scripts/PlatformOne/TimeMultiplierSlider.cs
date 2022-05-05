using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.UI;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(DigitalClockManager))]
public class TimeMultiplierSlider : MonoBehaviour
{
    [SerializeField] private GameObject slider;

    private int sliderMultiplier = 0;

    private DigitalClockManager dcmComponent;
    private CustomLightManager clmComponent;

    public int SliderMultiplier
    {
        get => sliderMultiplier;
        private set => sliderMultiplier = value;
    }

    // Start is called before the first frame update
    void Awake()
    {
        dcmComponent = GetComponent<DigitalClockManager>();
        clmComponent = GetComponent<CustomLightManager>();
        CheckSliderValue(slider.GetComponent<PinchSlider>().SliderValue);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnSliderValueChange(SliderEventData data)
    {
        if (data.NewValue >= 0 && data.NewValue < 0.25f)
        {
            sliderMultiplier = 0;
            dcmComponent.StopAutoIncrease();
            dcmComponent.StopBlinkingTime();
            if (SceneManager.GetActiveScene().name == "PlatformOne")
            {
                UIManagerForUserMenuMRTKWithoutButtons.Instance.ShowClockButtons();
            } else
            {
                UIManager.Instance.ShowClockButtons();
            }
        } else if (data.NewValue >= 0.25f && data.NewValue < 0.5f) {
            sliderMultiplier = 1;
            if (!dcmComponent.AutoIncrease)
            {
                dcmComponent.StartAutoIncrease();
                dcmComponent.ResumeBlinkingTime();
                if (SceneManager.GetActiveScene().name == "PlatformOne")
                {
                    UIManagerForUserMenuMRTKWithoutButtons.Instance.HideClockButtons();
                }
                else
                {
                    UIManager.Instance.HideClockButtons();
                }
            }
        } else if (data.NewValue >= 0.5f && data.NewValue < 0.75f)
        {
            sliderMultiplier = 200;
            if (!dcmComponent.AutoIncrease)
            {
                dcmComponent.StartAutoIncrease();
                dcmComponent.ResumeBlinkingTime();
                if (SceneManager.GetActiveScene().name == "PlatformOne")
                {
                    UIManagerForUserMenuMRTKWithoutButtons.Instance.HideClockButtons();
                }
                else
                {
                    UIManager.Instance.HideClockButtons();
                }
            }
        } else
        {
            sliderMultiplier = 1000;
            if (!dcmComponent.AutoIncrease)
            {
                dcmComponent.StartAutoIncrease();
                dcmComponent.ResumeBlinkingTime();
                if (SceneManager.GetActiveScene().name == "PlatformOne")
                {
                    UIManagerForUserMenuMRTKWithoutButtons.Instance.HideClockButtons();
                }
                else
                {
                    UIManager.Instance.HideClockButtons();
                }
            }
        }
        clmComponent.autoSpeedMultiplier = sliderMultiplier;
    }

    private void CheckSliderValue(float val)
    {
        if (val >= 0 && val < 0.25f)
        {
            sliderMultiplier = 0;
            dcmComponent.StopAutoIncrease();
            if (SceneManager.GetActiveScene().name == "PlatformOne")
            {
                UIManagerForUserMenuMRTKWithoutButtons.Instance.ShowClockButtons();
            }
            else
            {
                UIManager.Instance.ShowClockButtons();
            }
        }
        else if (val >= 0.25f && val < 0.5f)
        {
            sliderMultiplier = 1;
            if (!dcmComponent.AutoIncrease)
            {
                dcmComponent.StartAutoIncrease();
                if (SceneManager.GetActiveScene().name == "PlatformOne")
                {
                    UIManagerForUserMenuMRTKWithoutButtons.Instance.HideClockButtons();
                }
                else
                {
                    UIManager.Instance.HideClockButtons();
                }
            }
        }
        else if (val >= 0.5f && val < 0.75f)
        {
            sliderMultiplier = 20;
            if (!dcmComponent.AutoIncrease)
            {
                dcmComponent.StartAutoIncrease();
                if (SceneManager.GetActiveScene().name == "PlatformOne")
                {
                    UIManagerForUserMenuMRTKWithoutButtons.Instance.HideClockButtons();
                }
                else
                {
                    UIManager.Instance.HideClockButtons();
                }
            }
        }
        else
        {
            sliderMultiplier = 100;
            if (!dcmComponent.AutoIncrease)
            {
                dcmComponent.StartAutoIncrease();
                if (SceneManager.GetActiveScene().name == "PlatformOne")
                {
                    UIManagerForUserMenuMRTKWithoutButtons.Instance.HideClockButtons();
                }
                else
                {
                    UIManager.Instance.HideClockButtons();
                }
            }
        }
    }
}
