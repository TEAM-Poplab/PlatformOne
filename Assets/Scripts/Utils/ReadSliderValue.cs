using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Microsoft.MixedReality.Toolkit.UI;

public class ReadSliderValue : MonoBehaviour
{
    private TMP_Text field;

    // Start is called before the first frame update
    void Awake()
    {
        field = GetComponent<TMP_Text>();
    }

    public void OnValueUpdate(SliderEventData eventData)
    {
        field.text = eventData.NewValue.ToString("0.00");
    }

    public void OnLabelHidden()
    {
        GetComponent<MeshRenderer>().enabled = false;
    }

    public void OnLabelActive()
    {
        GetComponent<MeshRenderer>().enabled = true;
    }
}
