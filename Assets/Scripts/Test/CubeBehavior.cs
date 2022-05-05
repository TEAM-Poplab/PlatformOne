using Microsoft.MixedReality.Toolkit.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeBehavior : MonoBehaviour
{
    [SerializeField]
    private GameObject cylinder;
    // Start is called before the first frame update
    void Start()
    {
        transform.localScale = new Vector3(1f, 1f, 1f);
        gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnValueChange(SliderEventData eventData)
    {
        transform.localScale = new Vector3(eventData.NewValue, eventData.NewValue, eventData.NewValue);
        //cylinder.transform.localScale = new Vector3(1f, 1f - eventData.NewValue + 0.2f, 1f);
    }
}
