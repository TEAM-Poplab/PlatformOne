/************************************************************************************
* 
* Class Purpose: the class controls any interaction with the mesh sequence
*
************************************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities;

public class MeshSequenceController : MonoBehaviour
{
    #region Public properties
    public Material lightOnMaterial;
    public Material lightOffMaterial;
    public int ActiveFrameIndex
    {
        get;
        protected set;
    }

    #endregion

    #region Private properties and fields
    [HideInInspector] public List<GameObject> _frames = new List<GameObject>();
    [HideInInspector] public List<GameObject> _subframes = new List<GameObject>();
    [HideInInspector] public GameObject _currentFrame;
    #endregion

    #region Unity Engine methods
    // Start is called before the first frame update
    protected virtual void Start()
    {
        Setup();
        GameManager.Instance.OnDayLightSet.AddListener(LightOnChangeMaterial);
        GameManager.Instance.OnNightLightSet.AddListener(LightOffChangeMaterial);
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        //Calling these kind of methods everyframe is very expensive, but for the moment is the fastest solution to implement materal change
        //      in a new approach which uses enabling/desabling a sequence of meshes
        if (GameManager.Instance.LightStatus == GameManager.GameLight.DAY)
        {
            LightOnChangeMaterial();
        }
        else
        {
            LightOffChangeMaterial();
        }
    }
    #endregion

    #region Custom methods

    /*
     * Setup the scene initializing properly the list for children and theri childer of the mesh sequence container
     */
    public virtual void Setup()
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
    public virtual void OnValueChange(SliderEventData eventData)
    {
        _currentFrame.gameObject.SetActive(false);
        ActiveFrameIndex = Mathf.FloorToInt((_frames.Count - 1) * eventData.NewValue);
        Debug.Log($"{gameObject.name} frame index {ActiveFrameIndex}");
        _currentFrame = _frames[ActiveFrameIndex];
        _currentFrame.gameObject.SetActive(true);

        if (transform.parent.parent.name == "ObjectPlaceholderMS")
        {
            if (_currentFrame.GetComponent<Outline>() == null)
            {
                _currentFrame.AddComponent<Outline>();
                _currentFrame.GetComponent<Outline>().OutlineColor = new Color(1, 0.808f, 0.322f, 1);
                _currentFrame.GetComponent<Outline>().OutlineWidth = 2f;
                _currentFrame.GetComponent<Outline>().enabled = true;
            }
        }
    }

    /*
     * Called when the scene light is set to on
     */
    public virtual void LightOnChangeMaterial()
    {
        foreach(GameObject child in _subframes)
        {
            child.GetComponent<MeshRenderer>().material = lightOnMaterial;
        }     
    }

    /*
     * Called when the scene light is set to off
     */
    public virtual void LightOffChangeMaterial()
    {
        foreach (GameObject child in _subframes)
        {
            child.GetComponent<MeshRenderer>().material = lightOffMaterial;
        }
    }
    #endregion
}
