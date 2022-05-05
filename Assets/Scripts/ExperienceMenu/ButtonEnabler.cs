using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.UI;

public class ButtonEnabler : MonoBehaviour
{
    private Interactable _button;

    // Start is called before the first frame update
    void Start()
    {
        _button = GameObject.Find("3DPushButton").GetComponent<Interactable>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            _button.IsEnabled = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            _button.IsEnabled = false;
        }
    }
}
