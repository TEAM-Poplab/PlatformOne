/************************************************************************************
* 
* Class Purpose: the class controls any interaction with the mesh sequence in Platform One scene
*
************************************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Experimental.UI;

[RequireComponent(typeof(Dockable))]
public class GeometryInteraction : MonoBehaviour
{
    #region Public properties
    public Material lightOnMaterial;
    public Material lightOffMaterial;
    #endregion

    #region Private properties and fields
    private List<GameObject> _frames = new List<GameObject>();
    private List<GameObject> _subframes = new List<GameObject>();
    private GameObject _currentFrame;
    private Coroutine e = null;
    #endregion

    #region Unity Engine methods
    // Start is called before the first frame update
    void Start()
    {
        Setup();
        GameManager.Instance.OnDayLightSet.AddListener(LightOnChangeMaterial);
        GameManager.Instance.OnNightLightSet.AddListener(LightOffChangeMaterial);
    }

    // Update is called once per frame
    void Update()
    {
        //Calling these kind of methods everyframe is very expensive, but for the moment is the fastest solution to implement materal change
        //      in a new approach which uses enabling/desabling a sequence of meshes
        //if (GameManager.Instance.LightStatus == GameManager.GameLight.DAY)
        //{
        //    LightOnChangeMaterial();
        //}
        //else
        //{
        //    LightOffChangeMaterial();
        //}

        if (GetComponent<Dockable>().CanUndock && e == null)
        {
            e = StartCoroutine(PlayAnimation());
        } else if (GetComponent<Dockable>().CanDock && e != null)
        {
            StopCoroutine(e);
            e = null;
        }
    }
    #endregion

    #region Custom methods

    /*
     * Setup the scene initializing properly the list for children and theri childer of the mesh sequence container
     */
    private void Setup()
    {
        foreach (Transform child in transform)
        {
            _frames.Add(child.gameObject);
            _subframes.Add(child.GetChild(0).gameObject);
            child.GetChild(0).gameObject.GetComponent<MeshRenderer>().material = lightOffMaterial;
            child.gameObject.SetActive(false);
        }

        _currentFrame = _frames[0];
        _currentFrame.gameObject.SetActive(true);
    }

    /*
     * It's called whenever the slider change value
     * @param {SliderEventData} eventData - The Slider data passed by the event
     */
    public void OnValueChange(SliderEventData eventData)
    {
        _currentFrame.gameObject.SetActive(false);
        _currentFrame = _frames[Mathf.FloorToInt((_frames.Count - 1) * eventData.NewValue)];
        _currentFrame.gameObject.SetActive(true);
    }

    /*
     * Called when the scene light is set to on
     */
    public void LightOnChangeMaterial()
    {
        foreach (GameObject child in _subframes)
        {
            child.GetComponent<MeshRenderer>().material = lightOnMaterial;
        }
    }

    /*
     * Called when the scene light is set to off
     */
    public void LightOffChangeMaterial()
    {
        foreach (GameObject child in _subframes)
        {
            child.GetComponent<MeshRenderer>().material = lightOffMaterial;
        }
    }

    IEnumerator PlayAnimation()
    {
        float index = 0f;
        bool playReverse = false;

        while(true)
        {
            _currentFrame.gameObject.SetActive(false);
            _currentFrame = _frames[Mathf.FloorToInt((_frames.Count - 1) * index)];
            _currentFrame.gameObject.SetActive(true);
            
            if (index == 1f)
            {
                playReverse = true;
            } else if (index == 0f)
            {
                playReverse = false;
            }

            switch(playReverse)
            {
                case true:
                    index -= 0.05f;
                    break;
                case false:
                    index += 0.05f;
                    break;
            }

            yield return new WaitForSeconds(.1f);
        }  
    }
    #endregion
}
