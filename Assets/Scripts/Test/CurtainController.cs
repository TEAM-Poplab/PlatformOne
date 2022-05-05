using Microsoft.MixedReality.Toolkit.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurtainController : MonoBehaviour
{
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
        GetComponent<Animator>().SetFloat("TimeController", eventData.NewValue);
    }
}
