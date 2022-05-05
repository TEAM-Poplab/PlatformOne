using Microsoft.MixedReality.Toolkit.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VertexExtrusionScript : MonoBehaviour
{
    //[SerializeField]
    //private GameObject cylinder;
    // Start is called before the first frame update
    void Start()
    {
        //transform.localScale = new Vector3(1f, 1f, 1f);
        //gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnSliderUpdate(SliderEventData eventData)
    {
        GetComponent<MeshRenderer>().material.SetFloat("_VertexExtrusionValue", 1.01f - eventData.NewValue);
        //cylinder.transform.localScale = new Vector3(1f, 1f - eventData.NewValue + 0.2f, 1f);
    }
}
