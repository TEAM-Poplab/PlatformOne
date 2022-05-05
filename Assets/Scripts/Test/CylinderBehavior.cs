using Microsoft.MixedReality.Toolkit.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CylinderBehavior : MonoBehaviour
{
    [SerializeField]
    private GameObject cylinder;

    // Start is called before the first frame update
    void Start()
    {
        transform.localScale = new Vector3(transform.localScale.x, 0.01f, transform.localScale.z);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnValueChange(SliderEventData eventData)
    {
        transform.localScale = new Vector3(transform.localScale.x, 1.01f-eventData.NewValue, transform.localScale.z);
        //cylinder.transform.localScale = new Vector3(1f, 1f - eventData.NewValue + 0.2f, 1f);
    }
}
