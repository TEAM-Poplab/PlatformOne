using GH_IO.Serialization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AreaLightController : MonoBehaviour
{
    [SerializeField]
    private GameObject mesh;

    private bool isActive = false;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if(mesh.GetComponent<Triggers>().enabled)
        {
            isActive = mesh.GetComponent<Triggers>().isActive;
        } else if(mesh.GetComponent<NewTriggers>().enabled)
        {
            isActive = mesh.GetComponent<NewTriggers>().IsActive;
        }

        gameObject.SetActive(isActive);
    }
}
