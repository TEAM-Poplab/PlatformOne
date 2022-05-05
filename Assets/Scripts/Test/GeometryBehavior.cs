using Microsoft.MixedReality.Toolkit.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeometryBehavior : MonoBehaviour
{
    [SerializeField]
    private GameObject[] faces;

    [SerializeField]
    private GameObject cylinder;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnValueChange(SliderEventData eventData)
    {
        foreach(GameObject face in faces)
        {
            face.transform.localRotation = Quaternion.Euler(-90, 0, eventData.NewValue * 65 + 5);
        }

        cylinder.transform.localScale = new Vector3(1f, 1f - eventData.NewValue + 0.2f, 1f);
    }
}
