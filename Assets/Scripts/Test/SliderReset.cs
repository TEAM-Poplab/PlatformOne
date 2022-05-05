using Microsoft.MixedReality.Toolkit.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SliderReset : MonoBehaviour
{
    [SerializeField]
    private PinchSlider slider;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnDisable()
    {
        slider.SliderValue = 0;
    }
}
