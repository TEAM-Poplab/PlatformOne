using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Experimental.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshSequenceControllerImproved : MeshSequenceController
{
    public GameObject GetCurrentFrame() => _currentFrame;
    public GameObject GetFrameWhenDocked() => _frames[defaultIndex-1];
    public Material GetCurrentFramesMaterial() => _currentFrame.transform.GetChild(0).GetComponent<MeshRenderer>().material;
    public Material GetFrameWhenDeockedMaterial() => _frames[defaultIndex-1].transform.GetChild(0).GetComponent<MeshRenderer>().material;

    [HideInInspector] public int defaultIndex = 0;

    // Start is called before the first frame update
    protected override void Start()
    {

    }

    // Update is called once per frame
    protected override void Update()
    {
        
    }

    public override void Setup()
    {
        //Debug.Log("Setting up...");
        Transform meshSequenceObject = transform.GetChild(0).GetChild(0);

        foreach (Transform child in meshSequenceObject)
        {
            _frames.Add(child.gameObject);
            _subframes.Add(child.GetChild(0).gameObject);

            //child.GetChild(0).gameObject.GetComponent<MeshRenderer>().material = lightOffMaterial;
            child.gameObject.SetActive(false);
        }

        _currentFrame = _frames[0];
        _currentFrame.gameObject.SetActive(true);

        //Debug.Log("Frames Count: " + _frames.Count + "by: "+ gameObject.name);
        //foreach (GameObject obj in _frames)
        //{
        //    Debug.Log(obj.name);
        //}
    }

    public virtual void SetupAfterFreeze()
    {
        Transform meshSequenceObject = transform.GetChild(0).GetChild(0);

        foreach (Transform child in meshSequenceObject)
        {
            _frames.Add(child.gameObject);
            _subframes.Add(child.GetChild(0).gameObject);

            if (child.gameObject.activeSelf)
            {
                _currentFrame = child.gameObject;
            }
        }
        Destroy(_currentFrame.GetComponent<Animator>());
    }

    /// <summary>
    /// Called whenever the Mesh Sequence Object is put back into <see cref="Dock"/> 
    /// </summary>
    /// <remarks>Reset the mesh to first frame</remarks>
    public virtual void ResetForDock()
    {
        ActivateFrame(defaultIndex);
    }

    /// <summary>
    /// Enables the selected frame and desable any other one
    /// </summary>
    /// <param name="frame">The ordinal frame number of the frame to set active</param>
    public virtual void ActivateFrame(int frame)
    {
        if (frame < 1)
        {
            Debug.LogWarning("Wrong index for active frame!");
            defaultIndex = 1;
            return;
        }

        for (int i = 0; i < _frames.Count; i++)
        {
            if (i == frame - 1)
            {
                _frames[i].SetActive(true);
            }
            else
            {
                _frames[i].SetActive(false);
            }
        }

        //Done only the first time the method is called, and we call it for the first time during setup
        if (defaultIndex == 0)
        {
            defaultIndex = frame;
        }

        if (GetComponent<RealtimeMeshSequenceController>())
        {
            GetComponent<RealtimeMeshSequenceController>().SetIndex(frame);
        }
    }

    public virtual void ActivateAnimator(bool flag)
    {
        foreach (GameObject child in _frames)
        {
            child.GetComponent<Animator>().enabled = flag;
        }
    }

    /*
     * Called when the scene light is set to on
     */
    public virtual void LightOnChangeMaterial(Material mat)
    {
        foreach (GameObject child in _subframes)
        {
            child.GetComponent<MeshRenderer>().material = mat;
        }
    }

    /*
     * Called when the scene light is set to off
     */
    public virtual void LightOffChangeMaterial(Material mat)
    {
        foreach (GameObject child in _subframes)
        {
            child.GetComponent<MeshRenderer>().material = mat;
        }
    }

    public virtual void ChangeMaterialToMeshes(Material mat)
    {
        foreach (GameObject child in _subframes)
        {
            child.GetComponent<MeshRenderer>().material = mat;
        }
    }

    public virtual void ChangeShadowCastingModeToMeshes(bool flag)
    {
        foreach (GameObject child in _subframes)
        {
            child.GetComponent<MeshRenderer>().shadowCastingMode = flag == true? UnityEngine.Rendering.ShadowCastingMode.On : UnityEngine.Rendering.ShadowCastingMode.Off;
        }
    }
}
