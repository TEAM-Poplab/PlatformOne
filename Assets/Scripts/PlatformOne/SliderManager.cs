using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;
using TMPro;

public class SliderManager : Singleton<SliderManager>
{
    [SerializeField][Tooltip("Prefab for the slider, properly set with any necessary component. The pref should include che tooltip")]
    protected GameObject sliderPrefab;

    [SerializeField]
    [Tooltip("Placeholder transform where place the newly instantiated slider")]
    protected Transform sliderPlaceholderPosition;

    PlatformDockPosition platformDock;
    PlatformPlaceholderMeshSequence platformPlaceholderMS;
    GameObject currentSlider = null;

    // Start is called before the first frame update
    void Start()
    {
        platformDock = GameObject.Find("PlatformDockPosition").GetComponent<PlatformDockPosition>();
        platformPlaceholderMS = GameObject.Find("ObjectPlaceholderMS").GetComponent<PlatformPlaceholderMeshSequence>();

        platformDock.onObjectPlatformedMS.AddListener(OnObjectMSPlatformed);
        platformDock.onObjectUnplatformedMS.AddListener(OnObjectMSUnplatformed);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    protected virtual void OnObjectMSPlatformed(GameObject platformedObject)
    {
        currentSlider = Instantiate(sliderPrefab, sliderPlaceholderPosition);
        currentSlider.transform.GetChild(0).gameObject.GetComponent<ToolTipConnector>().Target = platformDock.PlatformedObject;
        currentSlider.transform.GetChild(0).GetChild(1).GetChild(0).gameObject.GetComponent<PinchSlider>().OnValueUpdated.AddListener(platformedObject.GetComponent<MeshSequenceControllerImproved>().OnValueChange);
        StartCoroutine(WaitToConnectSlider());
        SetLabelVisibility(UIManagerForUserMenuMRTKWithoutButtons.Instance.AreLabelsActive);
        GeometryStatusSaver.Instance.SaveSliderObject(platformedObject.transform.parent.gameObject, currentSlider);
    }

    protected virtual void OnObjectMSUnplatformed()
    {
        Destroy(currentSlider);
        currentSlider = null;
    }

    protected virtual IEnumerator WaitToConnectSlider()
    {
        yield return new WaitForSeconds(1.1f);
        currentSlider.transform.GetChild(0).GetChild(1).GetChild(0).gameObject.GetComponent<PinchSlider>().OnValueUpdated.AddListener(platformPlaceholderMS.InstantiatedMainObject.GetComponent<MeshSequenceControllerImproved>().OnValueChange);
    }

    public virtual void OnDockHidden()
    {
        if (currentSlider != null)
            sliderPlaceholderPosition.GetComponent<RadialView>().enabled = false;
    }

    public virtual void OnDockShowed()
    {
        if (currentSlider != null)
            sliderPlaceholderPosition.GetComponent<RadialView>().enabled = true;
    }

    /// <summary>
    /// Set the slider labels visibility
    /// </summary>
    /// <param name="flag"> The flag which toggle the visibility. true = labels are shown. false = labels are hidden</param>
    public void SetLabelVisibility(bool flag)
    {
        if (currentSlider!= null)
        {
            switch (flag)
            {
                case true:
                    currentSlider.transform.GetChild(0).GetChild(3).gameObject.GetComponent<MeshRenderer>().enabled = true;
                    currentSlider.transform.GetChild(0).GetChild(1).GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetChild(3).gameObject.GetComponent<ReadSliderValue>().OnLabelActive();
                    break;
                case false:
                    currentSlider.transform.GetChild(0).GetChild(3).gameObject.GetComponent<MeshRenderer>().enabled = false;
                    currentSlider.transform.GetChild(0).GetChild(1).GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetChild(3).gameObject.GetComponent<ReadSliderValue>().OnLabelHidden();
                    break;
            }
        } 
    }

    public virtual void DisableCurrentSlider()
    {
        if (currentSlider != null)
        {
            currentSlider.SetActive(false);
            currentSlider = null;
        }
    }

    public virtual void EnableCurrentSlider()
    {
        if (currentSlider == null)
        {
            currentSlider = GeometryStatusSaver.Instance.GetSliderObject(platformDock.PlatformedObject.transform.parent.gameObject);
            currentSlider.SetActive(true);
        }
    }
}
