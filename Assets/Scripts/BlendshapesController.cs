using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.UI;

[RequireComponent(typeof(SkinnedMeshRenderer))]
public class BlendshapesController : MonoBehaviour
{
    private SkinnedMeshRenderer smr;
    // Start is called before the first frame update
    void Start()
    {
        smr = GetComponent<SkinnedMeshRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnValueChange(SliderEventData eventData)
    {
        smr.SetBlendShapeWeight(0, Mathf.Lerp(0, 100, eventData.NewValue));
    }
}
