using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Bolt;
using Ludiq;
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;

public class ScaleHandleBehaviour : MonoBehaviour
{
    private GameObject target;
    private List<GameObject> scaleHandlesList;
    private List<GameObject> rotateHandlesList;
    private List<GameObject> translateHandleList;
    private CustomBoundsControl boundsControl;
    private Color originalMaterialColor;
    public Color interactioMaterialColor = new Color(205, 38, 83, 51);
    private Vector3 originalScale;

    private void Update()
    {
    }

    public void Init(GameObject target, CustomBoundsControl script)
    {
        this.target = target;
        boundsControl = script;
        scaleHandlesList = boundsControl.ScaleHandlesList;
        rotateHandlesList = boundsControl.RotateHandlesList;
        translateHandleList = boundsControl.TranslateHandleList;
    }

    public void Activation()
    {
        originalScale = transform.localScale;
        transform.localScale = new Vector3(transform.localScale.x*1.5f, transform.localScale.y*1.5f, transform.localScale.z*1.5f);

        //Hide unused handles
        foreach (GameObject handle in scaleHandlesList)
        {
            if (handle.name != gameObject.name)
            {
                handle.SetActive(false);
            }
        }

        foreach (GameObject handle in rotateHandlesList)
        {
            handle.SetActive(false);
        }

        foreach (GameObject handle in translateHandleList)
        {
            handle.SetActive(false);
        }

        originalMaterialColor = GetComponent<MeshRenderer>().material.GetColor("_Color");
        GetComponent<MeshRenderer>().material.SetColor("_Color", interactioMaterialColor);
    }

    public void Deactivation()
    {
        transform.localScale = originalScale;

        foreach (GameObject handle in scaleHandlesList)
        {
            if (handle.name != gameObject.name)
            {
                handle.SetActive(true);
            }
        }

        foreach (GameObject handle in rotateHandlesList)
        {
            handle.SetActive(true);
        }

        foreach (GameObject handle in translateHandleList)
        {
            handle.SetActive(true);
        }

        GetComponent<MeshRenderer>().material.SetColor("_Color", originalMaterialColor);
    }
}
