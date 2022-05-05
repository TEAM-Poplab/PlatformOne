using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;
using Microsoft.MixedReality.Toolkit.UI.BoundsControl;
using Microsoft.MixedReality.Toolkit.Experimental.UI;
using Bolt;
using Ludiq;
using UnityEngine.Events;

/// <summary>
/// The class <c>PlatformPlaceholder</c> is responsible for the projection of any object which interacts with the <see cref="PlatformDockPosition"/>.
/// It creates the life-size copy of the object and handles any interaction sync from the original object
/// </summary>
public class PlatformPlaceholder : MonoBehaviour
{
    public PlatformDockPosition platformDock;
    public GameObject toggleButton;

    public Material realityOnMaterial;
    public Material realityOffMaterial;

    public GameObject _instantiatedObjectOrigin = null;
    protected GameObject _instantiatedMainObject = null;

    protected InstantStatus instantiatedObjectStatus = InstantStatus.NOT_INSTANTIATED;

    protected float lerpTime = 0.1f;

    protected bool enableLiveChanges = false;

    //Variables for live changes
    protected float scaleFactor = 1f;
    protected Quaternion rotationAngle = Quaternion.identity;
    protected Quaternion startRotation = Quaternion.identity;

    protected GeometryStatusSaver.SavedTransform trasformBeforeFreeze = new GeometryStatusSaver.SavedTransform();

    public GameObject InstantiatedObject
    {
        get => _instantiatedObjectOrigin;
        protected set => _instantiatedObjectOrigin = value;
    }

    public GameObject InstantiatedMainObject
    {
        get => _instantiatedMainObject;
        protected set => _instantiatedMainObject = value;
    }

    public bool EnableLiveChanges
    {
        get => enableLiveChanges;
        set => enableLiveChanges = value;
    }

    public float ScaleFactor
    {
        set => scaleFactor = value;
    }

    public Quaternion RotationAngle
    {
        set => rotationAngle = value;
    }

    public InstantStatus InstantiatedObjectStatus
    {
        get => InstantiatedObjectStatus;
    }
    public enum InstantStatus
    {
        INSTANTIATING,
        INSTANTIATED,
        NOT_INSTANTIATED,
        UNFROZEN
    }

    /// <summary>
    /// Subscribes to events
    /// </summary>
    protected virtual void Awake()
    {
        platformDock.onObjectPlatformed.AddListener(NewObjectInPlatform);
        platformDock.onObjectPlatformedAfterFreeze.AddListener(NewObjectInPlatformAfterUnfreeze);
        platformDock.onObjectUnplatformed.AddListener(RemoveObjectInPlatform);
        GameManager.Instance.OnDayLightSet.AddListener(ChangeMaterialOnRealityChange);
        GameManager.Instance.OnNightLightSet.AddListener(ChangeMaterialOnRealityChange);
    }

    /// <summary>
    /// Animation when the projected object is created and destroyed
    /// </summary>
    protected virtual void Update()
    {
        if (instantiatedObjectStatus == InstantStatus.INSTANTIATING)
        {
            _instantiatedObjectOrigin.transform.localScale = Solver.SmoothTo(_instantiatedObjectOrigin.transform.localScale, Vector3.one, Time.deltaTime, lerpTime);

            //if (_instantiatedObject.GetComponent<CustomBoundsControl>().HandlesContainer != null)
            //{
            //    _instantiatedObject.GetComponent<CustomBoundsControl>().DestroyBounds();
            //}

            if (Mathf.Abs(_instantiatedObjectOrigin.transform.localScale.x / Vector3.one.x - 1.0f) < 0.01f)
            {
                instantiatedObjectStatus = InstantStatus.INSTANTIATED;
                _instantiatedObjectOrigin.transform.localScale = Vector3.one;
                _instantiatedObjectOrigin.transform.localPosition = platformDock.PlatformedObject.transform.parent.localPosition;
                //_instantiatedObjectOrigin.transform.localRotation = platformDock.PlatformedObject.transform.parent.localRotation;

                startRotation = _instantiatedMainObject.transform.rotation;
            }
        } else if (instantiatedObjectStatus == InstantStatus.INSTANTIATED && _instantiatedObjectOrigin != null && enableLiveChanges)
        {
            //_instantiatedMainObject.transform.localPosition = platformDock.PlatformedObject.transform.localPosition;
            //_instantiatedMainObject.transform.rotation = platformDock.PlatformedObject.transform.rotation;
            //_instantiatedMainObject.transform.localScale = platformDock.PlatformedObject.transform.localScale;
            //_instantiatedObjectOrigin.transform.localPosition = platformDock.PlatformedObject.transform.parent.localPosition;
            _instantiatedObjectOrigin.transform.rotation = platformDock.PlatformedObject.transform.parent.rotation;
            _instantiatedObjectOrigin.transform.localScale = platformDock.PlatformedObject.transform.parent.localScale;
        } else if (instantiatedObjectStatus == InstantStatus.UNFROZEN)
        {
            _instantiatedObjectOrigin.transform.localScale = Solver.SmoothTo(_instantiatedObjectOrigin.transform.localScale, trasformBeforeFreeze.scale, Time.deltaTime, lerpTime);
            if (Mathf.Abs(_instantiatedObjectOrigin.transform.localScale.x / trasformBeforeFreeze.scale.x - 1.0f) < 0.01f)
            {
                instantiatedObjectStatus = InstantStatus.INSTANTIATED;
                _instantiatedObjectOrigin.transform.localScale = trasformBeforeFreeze.scale;
                _instantiatedObjectOrigin.transform.localPosition = trasformBeforeFreeze.position;
                startRotation = _instantiatedMainObject.transform.rotation;
            }
        }

        //toggleRotation = ((bool)Variables.Object(toggleButton).Get("Status"));
    }

    /// <summary>
    /// Listener for the event called when a new object is dragged onto the projection dock
    /// </summary>
    /// <param name="geometry">The geometry (centroid) dragged onto the projection dock. Custom event parameter, according to its interface</param>
    protected virtual void NewObjectInPlatform(GameObject geometry)
    {
        if (_instantiatedObjectOrigin != null)
        {
            Animator anim = _instantiatedMainObject.GetComponent<Animator>();
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
    /// <param name="geometry">The geometry dragged onto the projection dock. Custom event parameter, according to its interface</param>
    protected virtual void NewObjectInPlatformAfterUnfreeze(GameObject geometry)
    {
        if (_instantiatedObjectOrigin != null)
        {
            Animator anim = _instantiatedMainObject.GetComponent<Animator>();
            anim.SetTrigger("FadeOut");
            StartCoroutine(DestroyAndCreateObjectAfterAnimationEnded(1f, geometry));
        }
        else
        {
            InstantiateObjectAfterUnfreeze(geometry);
            instantiatedObjectStatus = InstantStatus.UNFROZEN;
        }
    }

    /// <summary>
    /// Listener for the event called when the object is removed from the projection dock
    /// </summary>
    protected virtual void RemoveObjectInPlatform()
    {
        if (_instantiatedObjectOrigin != null)
        {
            Animator anim = _instantiatedMainObject.GetComponent<Animator>();
            anim.SetTrigger("FadeOut");
            StartCoroutine(DestroyObjectAfterAnimationEnded(1f));
        }
    }

    /// <summary>
    /// Instantiates and properly set the projected object, desabling any unecessary script and setting the correct coordinates system 
    /// </summary>
    /// <param name="geometry">The gometry (centroid) being projected</param>
    protected virtual void InstantiateObject(GameObject geometry)
    {
        _instantiatedObjectOrigin = new GameObject();
        _instantiatedObjectOrigin.transform.parent = transform;
        _instantiatedObjectOrigin.name = "GeometryOrigin";
        _instantiatedObjectOrigin.transform.SetAsFirstSibling();
        _instantiatedMainObject = Instantiate(geometry, _instantiatedObjectOrigin.transform);    

        _instantiatedObjectOrigin.transform.localPosition = geometry.GetComponent<Platformable>().SavedOriginTransform.position;
        _instantiatedObjectOrigin.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
        _instantiatedObjectOrigin.transform.localRotation = Quaternion.identity;

        _instantiatedMainObject.transform.localPosition = geometry.GetComponent<Platformable>().SavedCentroidTransformWhenPivoted.position;
        _instantiatedMainObject.transform.localScale = geometry.GetComponent<Platformable>().SavedCentroidTransformWhenPivoted.scale;
        _instantiatedMainObject.transform.localRotation = geometry.GetComponent<Platformable>().SavedCentroidTransformWhenPivoted.rotation;

        _instantiatedMainObject.GetComponent<Dockable>().enabled = false;
        _instantiatedMainObject.GetComponent<Platformable>().typeOfGeometry = Platformable.GeometryType.Single;
        _instantiatedMainObject.GetComponent<Platformable>().enabled = false;
        _instantiatedMainObject.GetComponent<Collider>().enabled = false;
        _instantiatedMainObject.GetComponent<Animator>().enabled = true;
        _instantiatedMainObject.GetComponent<CustomBoundsControl>().enabled = false;
        _instantiatedMainObject.transform.GetChild(0).gameObject.GetComponent<MeshRenderer>().material = (GameManager.Instance.LightStatus == GameManager.GameLight.DAY ? realityOnMaterial : realityOffMaterial);
        _instantiatedMainObject.transform.GetChild(0).gameObject.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
        _instantiatedMainObject.GetComponent<Animator>().SetTrigger("Swap");

        //Outline for instantiated object first frame
        //_instantiatedMainObject.GetComponent<MeshSequenceControllerImproved>().GetCurrentFrame().AddComponent<MeshOutlineHierarchy>();
        _instantiatedMainObject.transform.GetChild(0).AddComponent<Outline>();
        _instantiatedMainObject.transform.GetChild(0).GetComponent<Outline>().OutlineColor = new Color(1, 0.808f, 0.322f, 1);
        _instantiatedMainObject.transform.GetChild(0).GetComponent<Outline>().OutlineWidth = 2f;
        _instantiatedMainObject.transform.GetChild(0).GetComponent<Outline>().enabled = true;
    }

    /// <summary>
    /// Instantiates and properly set the projected object after an unfreeze event, desabling any unecessary script and setting the correct coordinates system 
    /// </summary>
    /// <param name="geometry">The gometry being projected</param>
    protected virtual void InstantiateObjectAfterUnfreeze(GameObject geometry)
    {
        _instantiatedObjectOrigin = new GameObject();
        _instantiatedObjectOrigin.transform.parent = transform;
        _instantiatedObjectOrigin.name = "GeometryOrigin";
        _instantiatedMainObject = Instantiate(geometry, _instantiatedObjectOrigin.transform);

        trasformBeforeFreeze = GeometryStatusSaver.Instance.GetTransformValue(geometry);
        _instantiatedObjectOrigin.transform.localPosition = trasformBeforeFreeze.position;
        _instantiatedObjectOrigin.transform.localScale = trasformBeforeFreeze.scale;
        _instantiatedObjectOrigin.transform.localRotation = trasformBeforeFreeze.rotation;

        _instantiatedMainObject.transform.localPosition = geometry.GetComponent<Platformable>().SavedCentroidTransformWhenPivoted.position;
        _instantiatedMainObject.transform.localScale = geometry.GetComponent<Platformable>().SavedCentroidTransformWhenPivoted.scale;
        _instantiatedMainObject.transform.localRotation = geometry.GetComponent<Platformable>().SavedCentroidTransformWhenPivoted.rotation;

        _instantiatedMainObject.GetComponent<Dockable>().enabled = false;
        _instantiatedMainObject.GetComponent<Platformable>().typeOfGeometry = Platformable.GeometryType.Single;
        _instantiatedMainObject.GetComponent<Platformable>().enabled = false;
        _instantiatedMainObject.GetComponent<Collider>().enabled = false;
        _instantiatedMainObject.GetComponent<Animator>().enabled = true;
        _instantiatedMainObject.GetComponent<CustomBoundsControl>().enabled = false;
        _instantiatedMainObject.transform.GetChild(0).gameObject.GetComponent<MeshRenderer>().material = (GameManager.Instance.LightStatus == GameManager.GameLight.DAY ? realityOnMaterial : realityOffMaterial);
        _instantiatedMainObject.transform.GetChild(0).gameObject.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
        _instantiatedMainObject.GetComponent<Animator>().SetTrigger("Swap");
    }

    /// <summary>
    /// Listener for any reality mode changes, switching the material for the projected object
    /// </summary>
    protected virtual void ChangeMaterialOnRealityChange()
    {
        if (_instantiatedMainObject != null)
        {
            _instantiatedMainObject.transform.GetChild(0).gameObject.GetComponent<MeshRenderer>().material = (GameManager.Instance.LightStatus == GameManager.GameLight.DAY ? realityOnMaterial : realityOffMaterial);
        }
    }

    /// <summary>
    /// Coroutine called to destroy the projected object only after any animation done while it's being instantiated has ended
    /// </summary>
    /// <param name="seconds">Time to wait</param>
    /// <returns>IEnumerator</returns>
    /// <seealso cref="DestroyAndCreateObjectAfterAnimationEnded(float, GameObject)"/>
    protected virtual IEnumerator DestroyObjectAfterAnimationEnded(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        Destroy(_instantiatedObjectOrigin);
        _instantiatedObjectOrigin = null;
        _instantiatedMainObject = null;
    }

    /// <summary>
    /// Coroutine called to destroy the projected object only after any animation done while it's being instantiated has ended, and after then instantiate a new object.
    /// </summary>
    /// <remarks>Called when an object just dragged onto the dock has been immediately replaced by another object</remarks>
    /// <param name="seconds">Time to wait</param>
    /// <param name="geometry">The new geometry to create after the previous has been destroyed</param>
    /// <returns>IEnumerator</returns>
    /// <seealso cref="DestroyObjectAfterAnimationEnded(float)"/>
    protected virtual IEnumerator DestroyAndCreateObjectAfterAnimationEnded(float seconds, GameObject geometry)
    {
        yield return new WaitForSeconds(seconds);
        Destroy(_instantiatedObjectOrigin);
        _instantiatedObjectOrigin = null;
        _instantiatedMainObject = null;
        InstantiateObject(geometry);
        instantiatedObjectStatus = InstantStatus.INSTANTIATING;
    }

    /// <summary>
    /// Called when freeze geometry event is invoked: freezes the current geometry status in the scene.
    /// </summary>
    public virtual void OnGeometryFreeze()
    {
        FreezeGeometries.Instance.FreezeGeometry(_instantiatedObjectOrigin);
    }

    public virtual void DisableProjectedObject()
    {
        _instantiatedObjectOrigin.SetActive(false);
        _instantiatedObjectOrigin = null;
    }
}
