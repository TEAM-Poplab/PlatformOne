using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;
using Microsoft.MixedReality.Toolkit.UI.BoundsControl;
using Microsoft.MixedReality.Toolkit.Experimental.UI;
using Microsoft.MixedReality.Toolkit.Utilities;
using Bolt;
using Ludiq;

/// <inheritdoc/>
/// <remarks>This class extension supports the projection of a mesh sequence object</remarks>
public class PlatformPlaceholderMeshSequence : PlatformPlaceholder
{
    protected GameObject currentFrame = null;

    protected override void Awake()
    {
        platformDock.onObjectPlatformedMS.AddListener(NewObjectInPlatform);
        platformDock.onObjectPlatformedMSAfterFreeze.AddListener(NewObjectInPlatformAfterUnfreeze);
        platformDock.onObjectUnplatformedMS.AddListener(RemoveObjectInPlatform);
        GameManager.Instance.OnDayLightSet.AddListener(ChangeMaterialOnRealityChange);
        GameManager.Instance.OnNightLightSet.AddListener(ChangeMaterialOnRealityChange);
    }

    protected override void Update()
    {
        base.Update();
    }

    /// <inheritdoc/>
    /// <remarks>This overrides the single mesh method in order to support mesh sequence objects</remarks>
    protected override void InstantiateObject(GameObject geometry)
    {
        _instantiatedObjectOrigin = new GameObject();
        _instantiatedObjectOrigin.transform.parent = transform;
        _instantiatedObjectOrigin.name = "GeometryOrigin";
        _instantiatedObjectOrigin.transform.SetAsFirstSibling();
        _instantiatedMainObject = Instantiate(geometry, _instantiatedObjectOrigin.transform);

        _instantiatedObjectOrigin.transform.localPosition = geometry.GetComponent<PlatformableMeshSequence>().SavedOriginTransform.position;
        _instantiatedObjectOrigin.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
        _instantiatedObjectOrigin.transform.localRotation = Quaternion.identity;

        _instantiatedMainObject.transform.localPosition = geometry.GetComponent<PlatformableMeshSequence>().SavedCentroidTransformWhenPivoted.position;
        _instantiatedMainObject.transform.localScale = geometry.GetComponent<PlatformableMeshSequence>().SavedCentroidTransformWhenPivoted.scale;
        _instantiatedMainObject.transform.localRotation = geometry.GetComponent<PlatformableMeshSequence>().SavedCentroidTransformWhenPivoted.rotation;

        _instantiatedMainObject.GetComponent<Dockable>().enabled = false;
        _instantiatedMainObject.GetComponent<PlatformableMeshSequence>().typeOfGeometry = Platformable.GeometryType.Sequence;
        _instantiatedMainObject.GetComponent<PlatformableMeshSequence>().enabled = false;
        _instantiatedMainObject.GetComponent<Collider>().enabled = false;
        _instantiatedMainObject.GetComponent<CustomBoundsControl>().enabled = false;
        _instantiatedMainObject.GetComponent<ValueLabelsVisualizer>().enabled = false;
        _instantiatedMainObject.GetComponent<MeshSequenceControllerImproved>().ActivateFrame(1);
        //_instantiatedMainObject.GetComponent<MeshSequenceControllerImproved>().Setup();
        _instantiatedMainObject.GetComponent<MeshSequenceControllerImproved>().ChangeMaterialToMeshes(GameManager.Instance.LightStatus == GameManager.GameLight.DAY ? realityOnMaterial : realityOffMaterial);
        _instantiatedMainObject.GetComponent<MeshSequenceControllerImproved>().ChangeShadowCastingModeToMeshes(GameManager.Instance.LightStatus == GameManager.GameLight.DAY ? true : false);
        //_instantiatedMainObject.transform.GetChild(0).transform.localRotation = _instantiatedMainObject.GetComponent<Dockable>().OriginalRotation;
        _instantiatedMainObject.GetComponent<MeshSequenceControllerImproved>().GetCurrentFrame().GetComponent<Animator>().enabled = true;
        _instantiatedMainObject.GetComponent<MeshSequenceControllerImproved>().GetCurrentFrame().GetComponent<Animator>().SetTrigger("Swap");

        //Outline for instantiated object first frame
        //_instantiatedMainObject.GetComponent<MeshSequenceControllerImproved>().GetCurrentFrame().AddComponent<MeshOutlineHierarchy>();
        _instantiatedMainObject.GetComponent<MeshSequenceControllerImproved>().GetCurrentFrame().AddComponent<Outline>();
        _instantiatedMainObject.GetComponent<MeshSequenceControllerImproved>().GetCurrentFrame().GetComponent<Outline>().OutlineColor = new Color(1, 0.808f, 0.322f, 1);
        _instantiatedMainObject.GetComponent<MeshSequenceControllerImproved>().GetCurrentFrame().GetComponent<Outline>().OutlineWidth = 2f;
        _instantiatedMainObject.GetComponent<MeshSequenceControllerImproved>().GetCurrentFrame().GetComponent<Outline>().enabled = true;
    }

    /// <inheritdoc/>
    /// <remarks>This overrides the single mesh method in order to support mesh sequence objects</remarks>
    protected override void InstantiateObjectAfterUnfreeze(GameObject geometry)
    {
        _instantiatedObjectOrigin = GeometryStatusSaver.Instance.GetProjectedGeometry(geometry.transform.parent.gameObject);
        _instantiatedMainObject = _instantiatedObjectOrigin.transform.GetChild(0).gameObject;
        Debug.LogError("_instantiatedObjectOrigin after unfreeze: " + _instantiatedObjectOrigin.name);
        _instantiatedObjectOrigin.transform.SetAsFirstSibling();
        _instantiatedObjectOrigin.SetActive(true);

        SliderManager.Instance.EnableCurrentSlider();
    }

    /// <inheritdoc/>
    /// <remarks>This overrides the single mesh method in order to support mesh sequence objects</remarks>
    protected override void ChangeMaterialOnRealityChange()
    {
        if (_instantiatedMainObject != null)
        {
            _instantiatedMainObject.GetComponent<MeshSequenceControllerImproved>().ChangeMaterialToMeshes(GameManager.Instance.LightStatus == GameManager.GameLight.DAY ? realityOnMaterial : realityOffMaterial);
            _instantiatedMainObject.GetComponent<MeshSequenceControllerImproved>().ChangeShadowCastingModeToMeshes(GameManager.Instance.LightStatus == GameManager.GameLight.DAY ? true : false);
        }
    }

    /// <inheritdoc/>
    /// <remarks>This overrides the single mesh method in order to support mesh sequence objects</remarks>
    protected override void RemoveObjectInPlatform()
    {
        if (_instantiatedObjectOrigin != null)
        {
            Animator anim = _instantiatedMainObject.GetComponent<MeshSequenceControllerImproved>().GetCurrentFrame().GetComponent<Animator>();
            anim.enabled = true;
            anim.SetTrigger("FadeOut");
            StartCoroutine(DestroyObjectAfterAnimationEnded(1f));
            currentFrame = null;
        }

    }

    /// <inheritdoc/>
    /// <remarks>This overrides the single mesh method in order to support mesh sequence objects</remarks>
    protected override void NewObjectInPlatform(GameObject geometry)
    {
        if (_instantiatedObjectOrigin != null)
        {
            Animator anim = _instantiatedMainObject.GetComponent<MeshSequenceControllerImproved>().GetCurrentFrame().GetComponent<Animator>();
            anim.enabled = true;
            anim.SetTrigger("FadeOut");
            StartCoroutine(DestroyAndCreateObjectAfterAnimationEnded(1f, geometry));
        }
        else
        {
            InstantiateObject(geometry);
            instantiatedObjectStatus = InstantStatus.INSTANTIATING;
        }
    }

    /// <summary>
    /// Listener for the event called when an object is unfreeze and it's replaced onto the projection dock
    /// </summary>
    /// <param name="geometry">The geometry (centroid) dragged onto the projection dock. Custom event parameter, according to its interface</param>
    protected override void NewObjectInPlatformAfterUnfreeze(GameObject geometry)
    {
        if (_instantiatedObjectOrigin != null)
        {
            Animator anim = _instantiatedMainObject.GetComponent<MeshSequenceControllerImproved>().GetCurrentFrame().GetComponent<Animator>();
            anim.enabled = true;
            anim.SetTrigger("FadeOut");
            StartCoroutine(DestroyAndCreateObjectAfterAnimationEndedFrozen(1f, geometry));
        }
        else
        {
            Debug.LogError("NewObjectInPlatformAfterUnfreeze. Going to renable the object");
            InstantiateObjectAfterUnfreeze(geometry);
            //instantiatedObjectStatus = InstantStatus.UNFROZEN;
            instantiatedObjectStatus = InstantStatus.INSTANTIATED;
            Debug.LogError("DONE! Status: INSTANTIATED");
        }
    }

    /// <inheritdoc/>
    /// <remarks>This overrides the single mesh method in order to support mesh sequence objects</remarks>
    protected override IEnumerator DestroyObjectAfterAnimationEnded(float seconds)
    {
        Destroy(_instantiatedMainObject.GetComponent<MeshSequenceControllerImproved>().GetCurrentFrame().GetComponent<Outline>());
        Material[] mat = { _instantiatedMainObject.GetComponent<MeshSequenceControllerImproved>().GetCurrentFrame().transform.GetComponentInChildren<MeshRenderer>().materials[0] };
        _instantiatedMainObject.GetComponent<MeshSequenceControllerImproved>().GetCurrentFrame().transform.GetComponentInChildren<MeshRenderer>().materials = mat;

        yield return new WaitForSeconds(seconds);
        Destroy(_instantiatedObjectOrigin);
        _instantiatedObjectOrigin = null;
        _instantiatedMainObject = null;
    }

    /// <inheritdoc/>
    /// <remarks>This overrides the single mesh method in order to support mesh sequence objects</remarks>
    protected override IEnumerator DestroyAndCreateObjectAfterAnimationEnded(float seconds, GameObject geometry)
    {
        Destroy(_instantiatedMainObject.GetComponent<MeshSequenceControllerImproved>().GetCurrentFrame().GetComponent<Outline>());
        Material[] mat = { _instantiatedMainObject.GetComponent<MeshSequenceControllerImproved>().GetCurrentFrame().transform.GetComponentInChildren<MeshRenderer>().materials[0] };
        _instantiatedMainObject.GetComponent<MeshSequenceControllerImproved>().GetCurrentFrame().transform.GetComponentInChildren<MeshRenderer>().materials = mat;

        yield return new WaitForSeconds(seconds);
        Destroy(_instantiatedObjectOrigin);
        _instantiatedObjectOrigin = null;
        _instantiatedMainObject = null;
        InstantiateObject(geometry);
        instantiatedObjectStatus = InstantStatus.INSTANTIATING;
    }

    /// <inheritdoc/>
    /// <remarks>This overrides the single mesh method in order to support mesh sequence objects</remarks>
    protected IEnumerator DestroyAndCreateObjectAfterAnimationEndedFrozen(float seconds, GameObject geometry)
    {
        Destroy(_instantiatedMainObject.GetComponent<MeshSequenceControllerImproved>().GetCurrentFrame().GetComponent<Outline>());
        Material[] mat = { _instantiatedMainObject.GetComponent<MeshSequenceControllerImproved>().GetCurrentFrame().transform.GetComponentInChildren<MeshRenderer>().materials[0] };
        _instantiatedMainObject.GetComponent<MeshSequenceControllerImproved>().GetCurrentFrame().transform.GetComponentInChildren<MeshRenderer>().materials = mat;

        yield return new WaitForSeconds(seconds);
        Destroy(_instantiatedObjectOrigin);
        _instantiatedObjectOrigin = null;
        _instantiatedMainObject = null;
        InstantiateObjectAfterUnfreeze(geometry);
        instantiatedObjectStatus = InstantStatus.INSTANTIATED;
    }
}
