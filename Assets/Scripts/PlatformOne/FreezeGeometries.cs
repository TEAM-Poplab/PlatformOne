using Microsoft.MixedReality.Toolkit.Experimental.UI;
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Microsoft.MixedReality.Toolkit.Utilities;

/// <summary>
/// Freezes single mesh and mesh sequence geometries in the world, and handles any aspect related to their managements, such as:
/// <list type="bullet">
/// <item>Freeze geometry status</item>
/// <item>Unfreeze geometry</item>
/// <item>Methods to manipulate geometries while frozen</item>
/// <item>Events handler</item>
/// </list>
/// </summary>
public class FreezeGeometries : Singleton<FreezeGeometries>, MATKPointerResponder
{
    #region Fields
    [SerializeField] protected List<GameObject> frozenGeometries = new List<GameObject>();
    protected Dictionary<GameObject, GameObject> originalGeometries = new Dictionary<GameObject, GameObject>();
    //Variables used with the selection system
    protected Dictionary<GameObject, Material> materialsList = new Dictionary<GameObject, Material>();
    protected Dictionary<GameObject, Coroutine> coroutinesList = new Dictionary<GameObject, Coroutine>();
    protected GameObject currentselectedGeometryOverlay;
    protected Material selectedGeometryOriginalMaterial;
    Coroutine selectionAnimationOnFocus = null;
    Coroutine selectionAnimationLostFocus = null;
    protected GameObject previousSelectedGeometry;
    #endregion

    #region Properties
    [Header("General settings")]
    public PlatformDockPosition platformDock;
    public PlatformPlaceholder objectPlaceholder;
    public PlatformPlaceholderMeshSequence objectPlaceholderMS;
    public Dock dockObject;
    [Space(15)]
    [Header("Materials")]
    public Material materialWhenSelectedByPointer;
    public Color selectionInMaterialColor;
    public Color selectionOutMaterialColor;
    [Space(7)]
    public Material realityOffFadeMaterial;
    public Color realityOffFadeInMaterialColor;
    public Color realityOffFadeOutMaterialColor;
    [Space(7)]
    public Material realityOnFadeMaterial;
    public Color realityOnFadeInMaterialColor;
    public Color realityOnFadeOutMaterialColor;
    #endregion

    #region Events
    [Header("Events")]
    /// <summary>
    /// Called when a geometry has yet been frozen: classes can add listener to implement custom behaviour in response of the event
    /// </summary>
    [Tooltip("Called when a geometry has yet been frozen")] public UnityEvent frozenGeometryEvent = new UnityEvent();
    /// <summary>
    /// Called when a geometry has yet been unfrozen: classes can add listener to implement custom behaviour in response of the event
    /// </summary>
    [Tooltip("Called when a geometry has yet been unfrozen")] public UnityEvent unfrozenGeometryEvent = new UnityEvent();
    #endregion

    #region Unity Events
    private void Start()
    {
        currentselectedGeometryOverlay = new GameObject("SelectionOverlay");
        currentselectedGeometryOverlay.transform.parent = transform;
        currentselectedGeometryOverlay.AddComponent<MeshFilter>();
        currentselectedGeometryOverlay.AddComponent<MeshRenderer>();
        currentselectedGeometryOverlay.SetActive(false);
    }
    #endregion

    /// <summary>
    /// Freezes the current geometry status in the scene, setting it properly
    /// </summary>
    /// <param name="instantiatedObjectOrigin">The geometry origin gameobjet to freeze</param>
    public virtual void FreezeGeometry(GameObject instantiatedObjectOrigin)
    {
        if (instantiatedObjectOrigin != null)
        {
            GameObject obj = Instantiate(instantiatedObjectOrigin, instantiatedObjectOrigin.transform.position, instantiatedObjectOrigin.transform.rotation, transform);
            obj.name = "Frozen_" + instantiatedObjectOrigin.transform.GetChild(0).name; //Name = Frozen_DockableSequenceName_centroid || Frozen_DockableSingle_centroid
            Destroy(obj.transform.GetChild(0).GetComponent<MeshSequenceControllerImproved>().GetCurrentFrame().GetComponent<Outline>());
            Material[] mat = { obj.transform.GetChild(0).GetComponent<MeshSequenceControllerImproved>().GetCurrentFrame().transform.GetComponentInChildren<MeshRenderer>().materials[0] };
            Destroy(obj.transform.GetChild(0).GetComponent<MeshSequenceControllerImproved>().GetCurrentFrame().transform.GetComponentInChildren<MeshRenderer>().materials[1]);
            Destroy(obj.transform.GetChild(0).GetComponent<MeshSequenceControllerImproved>().GetCurrentFrame().transform.GetComponentInChildren<MeshRenderer>().materials[2]);
            obj.transform.GetChild(0).GetComponent<MeshSequenceControllerImproved>().GetCurrentFrame().transform.GetComponentInChildren<MeshRenderer>().materials = mat;
            SetCollider(obj);
            SetSelectionOverlay(obj);
            frozenGeometries.Add(obj);
            originalGeometries.Add(obj, platformDock.PlatformedObject.transform.parent.gameObject);
            frozenGeometryEvent.Invoke();
        }
    }

    /// <summary>
    /// Unfreeze the specified geometry in the scene, cleaning lists
    /// </summary>
    /// <param name="frozenGeometry">The frozen geometry to unfreeze</param>
    public virtual void UnfreezeGeometry(GameObject frozenGeometry)
    {
        GameObject frozenGeometryOrigin;
        if (frozenGeometry.transform.childCount != 0)
        {
            frozenGeometryOrigin = frozenGeometry.transform.parent.parent.parent.parent.gameObject;  //frozenGeoemtry is a frame, I have to get the origin name
        } else
        {
            frozenGeometryOrigin = frozenGeometry.transform.parent.parent.gameObject;
        }
        if (currentselectedGeometryOverlay.activeSelf)
        {
            currentselectedGeometryOverlay.SetActive(false);
        }
        frozenGeometries.Remove(frozenGeometryOrigin);
        StartCoroutine(AnimationBeforeDestroy(frozenGeometryOrigin));
        originalGeometries[frozenGeometryOrigin].SetActive(true); //The original object must implements its own behaviour when enabled again
        originalGeometries.Remove(frozenGeometryOrigin);
        unfrozenGeometryEvent.Invoke();
    }

    public virtual void ChangeMaterialToFrozenGeometries()
    {
        foreach (GameObject obj in frozenGeometries)
        {
            if (obj.transform.GetChild(0).GetChild(0).childCount != 0)
            {
                Debug.LogWarning("Name: " + obj.name);
                obj.transform.GetChild(0).GetComponent<MeshSequenceControllerImproved>().GetCurrentFrame().GetComponentInChildren<MeshRenderer>().material = GameManager.Instance.LightStatus == GameManager.GameLight.DAY ? objectPlaceholderMS.realityOnMaterial : objectPlaceholderMS.realityOffMaterial;
                obj.transform.GetChild(0).GetComponent<MeshSequenceControllerImproved>().ChangeShadowCastingModeToMeshes(GameManager.Instance.LightStatus == GameManager.GameLight.DAY ? true : false);
            }
            else
            {
                obj.transform.GetChild(0).GetChild(0).gameObject.GetComponent<MeshRenderer>().material = (GameManager.Instance.LightStatus == GameManager.GameLight.DAY ? objectPlaceholder.realityOnMaterial : objectPlaceholder.realityOffMaterial);
            }
        }
    }

    /// <summary>
    /// Properly set the frozen geometry to be used with the <see cref="MATKPointer">MATKPointer system</see>
    /// </summary>
    /// <param name="objOrigin">The origin gameobject to set</param>
    protected virtual void SetCollider(GameObject objOrigin)
    {
        GameObject centroid = objOrigin.transform.GetChild(0).gameObject;
        if (objOrigin.transform.GetChild(0).GetChild(0).childCount != 0)
        {
            objOrigin.transform.GetComponentInChildren<MeshSequenceControllerImproved>().SetupAfterFreeze();  //After being instantiated, no value is saved so we need to properly set the mesh sequence controller
            centroid.GetComponent<MeshSequenceControllerImproved>().GetCurrentFrame().layer = 14;
            var meshCollider = centroid.GetComponent<MeshSequenceControllerImproved>().GetCurrentFrame().AddComponent<MeshCollider>();
            meshCollider.sharedMesh = centroid.GetComponent<MeshSequenceControllerImproved>().GetCurrentFrame().transform.GetChild(0).GetComponent<MeshFilter>().mesh;
            var rigidbody = centroid.GetComponent<MeshSequenceControllerImproved>().GetCurrentFrame().AddComponent<Rigidbody>();
            rigidbody.isKinematic = true;
        } else
        {
            var mesh = centroid.transform.GetChild(0).gameObject;
            mesh.layer = 14;
            var meshCollider = mesh.AddComponent<MeshCollider>();
            meshCollider.sharedMesh = mesh.GetComponent<MeshFilter>().mesh;
            mesh.AddComponent<Rigidbody>().isKinematic = true;
        }
    }

    /// <summary>
    /// Properly set the selection overlay gameobject to be used with the <see cref="MATKPointer">MATKPointer system</see> when the frozen geometry is selected
    /// </summary>
    /// <param name="objOrigin">The object to create the overlay for</param>
    protected virtual void SetSelectionOverlay(GameObject objOrigin)
    {
        GameObject centroid = objOrigin.transform.GetChild(0).gameObject;
        GameObject selectionOverlay = new GameObject(objOrigin.name + "_selectionOverlay");
        var meshFilter = selectionOverlay.AddComponent<MeshFilter>();
        var meshRender = selectionOverlay.AddComponent<MeshRenderer>();
        meshRender.material = materialWhenSelectedByPointer;
        meshRender.material.SetColor("_Color", selectionInMaterialColor);
        if (objOrigin.transform.GetChild(0).GetChild(0).childCount != 0)
        {
            meshFilter.mesh = centroid.GetComponent<MeshSequenceControllerImproved>().GetCurrentFrame().transform.GetChild(0).GetComponent<MeshFilter>().mesh;
            selectionOverlay.transform.rotation = centroid.transform.GetChild(0).GetChild(0).GetChild(0).GetChild(0).transform.rotation;
        } else
        {
            meshFilter.mesh = centroid.transform.GetChild(0).GetComponent<MeshFilter>().mesh;
            selectionOverlay.transform.rotation = centroid.transform.GetChild(0).transform.rotation;
        }
        selectionOverlay.transform.localScale = objOrigin.transform.lossyScale * 1.01f;
        selectionOverlay.transform.parent = objOrigin.transform;
        selectionOverlay.transform.localPosition = Vector3.zero;
        selectionOverlay.transform.SetSiblingIndex(1);
        selectionOverlay.SetActive(false);
    }

    #region Utils
    public virtual void AddOriginalGeometry(GameObject objectToAdd)
    {
        originalGeometries.Add(objectToAdd, objectToAdd);
    }

    public virtual void AddAndDisableOriginalGeometry(GameObject objectToAdd)
    {
        originalGeometries.Add(objectToAdd, objectToAdd);
        objectToAdd.SetActive(false);
    }

    private string Reverse(string input)
    {
        char[] array = input.ToCharArray();
        Array.Reverse(array);
        return new string(array);
    }

    protected static bool AboutTheSameSize(float scale1, float scale2)
    {
        return Mathf.Abs(scale1 / scale2 - 1.0f) < 0.01f;
    }
    #endregion

    #region Coroutines
    /// <summary>
    /// Animates the frozen geometry towards the first free dock space before destrying it
    /// </summary>
    /// <param name="obj">The frozen instance of a projected object which is going to be animated and removed</param>
    /// <returns></returns>
    //protected IEnumerator AnimationBeforeDestroy(GameObject obj)
    //{
    //    Vector3 dest = platformDock.transform.position;
    //    foreach (DockPosition dp in dockObject.DockPositions)
    //    {
    //        if (!dp.IsOccupied)
    //        {
    //            dest = dp.transform.position;
    //            break;
    //        }
    //    }
    //    while (true)
    //    {
    //        obj.transform.localScale = Solver.SmoothTo(obj.transform.localScale, Vector3.zero, Time.deltaTime, 0.15f);
    //        obj.transform.position = Solver.SmoothTo(obj.transform.position, dest, Time.deltaTime, 0.15f);

    //        if (AboutTheSameSize(Vector3.zero.x, obj.transform.localScale.x))
    //        {
    //            Destroy(obj);
    //            yield break;
    //        }
    //        yield return null;
    //    }
    //}

    /// <summary>
    /// Animates the frozen geometry towards the platformable (projection) dock space before destrying it
    /// </summary>
    /// <param name="obj">The frozen instance of a projected object which is going to be animated and removed</param>
    /// <returns></returns>
    protected IEnumerator AnimationBeforeDestroy(GameObject obj)
    {
        Vector3 dest = platformDock.transform.GetChild(2).position;
        while (true)
        {
            obj.transform.localScale = Solver.SmoothTo(obj.transform.localScale, Vector3.zero, Time.deltaTime, 0.15f);
            obj.transform.position = Solver.SmoothTo(obj.transform.position, dest, Time.deltaTime, 0.15f);

            if (AboutTheSameSize(Vector3.zero.x, obj.transform.localScale.x))
            {
                Destroy(obj);
                yield break;
            }
            yield return null;
        }
    }

    /// <summary>
    /// Selection coroutines which starts when a frozen geometry is selected, animating the transition both for the frozen geometry and for its overlay
    /// </summary>
    /// <param name="frozenGeometry">The geometry selected by the <see cref="MATKPointer">pointer</see></param>
    /// <returns>An IEnumerator</returns>
    protected IEnumerator animateSelectionOverlayIn(GameObject frozenGeometry)
    {
        selectedGeometryOriginalMaterial = frozenGeometry.transform.GetChild(0).GetComponent<MeshRenderer>().material;
        frozenGeometry.transform.GetChild(0).GetComponent<MeshRenderer>().material = (GameManager.Instance.LightStatus == GameManager.GameLight.DAY ? realityOnFadeMaterial : realityOffFadeMaterial);
        float timer = 0f;
        previousSelectedGeometry = frozenGeometry;
        while(timer < 0.5f)
        {
            if (GameManager.Instance.LightStatus == GameManager.GameLight.NIGHT)
            {
                frozenGeometry.transform.GetChild(0).GetComponent<MeshRenderer>().material.SetColor("_Color", Color.Lerp(realityOffFadeInMaterialColor, realityOffFadeOutMaterialColor, timer*2));
            } else
            {
                frozenGeometry.transform.GetChild(0).GetComponent<MeshRenderer>().material.SetColor("_Color", Color.Lerp(realityOnFadeInMaterialColor, realityOnFadeOutMaterialColor, timer*2));
            }

            currentselectedGeometryOverlay.GetComponent<MeshRenderer>().material.SetColor("_Color", Color.Lerp(selectionInMaterialColor, selectionOutMaterialColor, timer*2));
            timer += Time.deltaTime;
            yield return null;
        }
        previousSelectedGeometry = null;
        selectionAnimationOnFocus = null;
    }

    /// <summary>
    /// Selection coroutines which starts when a frozen geometry is deselected, animating the transition both for the frozen geometry and for its overlay
    /// </summary>
    /// <param name="frozenGeometry">The geometry unselected by the <see cref="MATKPointer">pointer</see></param>
    /// <returns>An IEnumerator</returns>
    protected IEnumerator animateSelectionOverlayOut(GameObject frozenGeometry)
    {
        previousSelectedGeometry = frozenGeometry;
        float timer = 0f;
        while (timer < 0.5f)
        {
            if (GameManager.Instance.LightStatus == GameManager.GameLight.NIGHT)
            {
                frozenGeometry.transform.GetChild(0).GetComponent<MeshRenderer>().material.SetColor("_Color", Color.Lerp(realityOffFadeOutMaterialColor, realityOffFadeInMaterialColor, timer * 2));
            }
            else
            {
                frozenGeometry.transform.GetChild(0).GetComponent<MeshRenderer>().material.SetColor("_Color", Color.Lerp(realityOnFadeOutMaterialColor, realityOnFadeInMaterialColor, timer * 2));
            }

            currentselectedGeometryOverlay.GetComponent<MeshRenderer>().material.SetColor("_Color", Color.Lerp(selectionOutMaterialColor, selectionInMaterialColor, timer * 2));
            timer += Time.deltaTime;
            yield return null;
        }
        frozenGeometry.transform.GetChild(0).GetComponent<MeshRenderer>().material = selectedGeometryOriginalMaterial;
        currentselectedGeometryOverlay.SetActive(false);
        previousSelectedGeometry = null;
        selectionAnimationLostFocus = null;
    }

    /// <summary>
    /// Selection coroutines which starts when a frozen geometry is selected, animating the transition both for the frozen geometry and for its overlay.
    /// This version works with another selection overlay mechanism.
    /// </summary>
    /// <param name="frozenGeometry">The geometry selected by the <see cref="MATKPointer">pointer</see></param>
    /// <param name="currentselectedGeometryOverlay">The selection overlay for the selected frozen geometry</param>
    /// <returns>An IEnumerator</returns>
    protected IEnumerator animateSelectionOverlayIn(GameObject frozenGeometry, GameObject currentselectedGeometryOverlay)
    {
        GameObject geometry;
        if (frozenGeometry.transform.childCount != 0)
        {
            geometry = frozenGeometry.transform.GetChild(0).gameObject;
        }
        else
        {
            geometry = frozenGeometry;
        }
        materialsList.Add(frozenGeometry, geometry.GetComponent<MeshRenderer>().material);
        geometry.GetComponent<MeshRenderer>().material = (GameManager.Instance.LightStatus == GameManager.GameLight.DAY ? realityOnFadeMaterial : realityOffFadeMaterial);
        float timer = 0f;
        while (timer < 0.35f)
        {
            if (GameManager.Instance.LightStatus == GameManager.GameLight.NIGHT)
            {
                geometry.GetComponent<MeshRenderer>().material.SetColor("_Color", Color.Lerp(realityOffFadeInMaterialColor, realityOffFadeOutMaterialColor, timer * 2));
            }
            else
            {
                geometry.GetComponent<MeshRenderer>().material.SetColor("_Color", Color.Lerp(realityOnFadeInMaterialColor, realityOnFadeOutMaterialColor, timer * 2));
            }

            currentselectedGeometryOverlay.GetComponent<MeshRenderer>().material.SetColor("_Color", Color.Lerp(selectionInMaterialColor, selectionOutMaterialColor, timer * 2));
            timer += Time.deltaTime;
            yield return null;
        }
        coroutinesList.Remove(frozenGeometry);
    }

    /// <summary>
    /// Selection coroutines which starts when a frozen geometry is deselected, animating the transition both for the frozen geometry and for its overlay.
    /// This version works with another selection overlay mechanism.
    /// </summary>
    /// <param name="frozenGeometry">The geometry deselected by the <see cref="MATKPointer">pointer</see></param>
    /// <param name="currentselectedGeometryOverlay">The selection overlay for the deselected frozen geometry</param>
    /// <returns>An IEnumerator</returns>
    protected IEnumerator animateSelectionOverlayOut(GameObject frozenGeometry, GameObject currentselectedGeometryOverlay)
    {
        GameObject geometry;
        if (frozenGeometry.transform.childCount != 0)
        {
            geometry = frozenGeometry.transform.GetChild(0).gameObject;
        } else
        {
            geometry = frozenGeometry;
        }
        float timer = 0f;
        while (timer < 0.35f)
        {
            if (GameManager.Instance.LightStatus == GameManager.GameLight.NIGHT)
            {
                geometry.GetComponent<MeshRenderer>().material.SetColor("_Color", Color.Lerp(realityOffFadeOutMaterialColor, realityOffFadeInMaterialColor, timer * 2));
            }
            else
            {
                geometry.GetComponent<MeshRenderer>().material.SetColor("_Color", Color.Lerp(realityOnFadeOutMaterialColor, realityOnFadeInMaterialColor, timer * 2));
            }

            currentselectedGeometryOverlay.GetComponent<MeshRenderer>().material.SetColor("_Color", Color.Lerp(selectionOutMaterialColor, selectionInMaterialColor, timer * 2));
            timer += Time.deltaTime;
            yield return null;
        }
        geometry.GetComponent<MeshRenderer>().material = materialsList[frozenGeometry];
        materialsList.Remove(frozenGeometry);
        currentselectedGeometryOverlay.SetActive(false);
        coroutinesList.Remove(frozenGeometry);
    }
    #endregion

    #region Events response methods
    public void GeometryOnFocus(GameObject geometry)
    {
        //if (!currentselectedGeometryOverlay.activeSelf)
        //{
        //    currentselectedGeometryOverlay.GetComponent<MeshFilter>().sharedMesh = geometry.GetComponent<MeshCollider>().sharedMesh;
        //    currentselectedGeometryOverlay.GetComponent<MeshRenderer>().material = materialWhenSelectedByPointer;
        //    currentselectedGeometryOverlay.GetComponent<MeshRenderer>().material.SetColor("_Color", selectionInMaterialColor);
        //    currentselectedGeometryOverlay.transform.position = geometry.transform.position;
        //    currentselectedGeometryOverlay.transform.rotation = geometry.transform.rotation;
        //    currentselectedGeometryOverlay.transform.localScale = geometry.transform.lossyScale*1.01f;
        //    currentselectedGeometryOverlay.SetActive(true);

        //    if (selectionAnimationOnFocus == null)
        //    {
        //        selectionAnimationOnFocus = StartCoroutine(animateSelectionOverlayIn(geometry));
        //    }
        //}
        GameObject overlay;
        if (geometry.transform.parent.childCount > 1)
        {
            overlay = geometry.transform.parent.parent.parent.parent.GetChild(1).gameObject;
        } else
        {
            overlay = geometry.transform.parent.parent.GetChild(1).gameObject;
        }
        if (!overlay.activeSelf)
        {
            overlay.SetActive(true);
            Coroutine currentCoroutine = null;
            coroutinesList.TryGetValue(geometry, out currentCoroutine);
            if (currentCoroutine == null) //Nessuna coroutine in corso per questo oggetto, quindi siccome attivo la selezione avvio la coroutine di animazione ingresso
            {
                coroutinesList.Add(geometry, StartCoroutine(animateSelectionOverlayIn(geometry, overlay)));

            } else //Una qualche coroutine in corso per l'oggetto selezionato, quindi la blocco e avvio quella di selezione
            {
                StopCoroutine(coroutinesList[geometry]);
                coroutinesList[geometry] = StartCoroutine(animateSelectionOverlayIn(geometry, overlay));
            }
        }
    }

    public void GeometryOnLostFocus(GameObject geometry)
    {
        //if (currentselectedGeometryOverlay.activeSelf)
        //{
        //    if (selectionAnimationLostFocus == null)
        //    {
        //        if (selectionAnimationOnFocus != null)
        //        {
        //            StopCoroutine(selectionAnimationOnFocus);
        //            geometry.transform.GetChild(0).GetComponent<MeshRenderer>().material = selectedGeometryOriginalMaterial;
        //            currentselectedGeometryOverlay.SetActive(false);
        //            previousSelectedGeometry = null;
        //            selectionAnimationLostFocus = null;
        //        } else
        //        {
        //            selectionAnimationLostFocus = StartCoroutine(animateSelectionOverlayOut(geometry));
        //        }
        //    }
        //}
        GameObject overlay;
        if (geometry.transform.parent.childCount > 1)
        {
            overlay = geometry.transform.parent.parent.parent.parent.GetChild(1).gameObject;
        }
        else
        {
            overlay = geometry.transform.parent.parent.GetChild(1).gameObject;
        }
        if (overlay.activeSelf)
        {
            Coroutine currentCoroutine = null;
            coroutinesList.TryGetValue(geometry, out currentCoroutine);
            if (currentCoroutine == null) //Nessuna coroutine in corso per questo oggetto, quindi siccome disattivo la selezione avvio la coroutine di animazione uscita
            {
                coroutinesList.Add(geometry, StartCoroutine(animateSelectionOverlayOut(geometry, overlay)));

            }
            else //Una qualche coroutine in corso per l'oggetto selezionato, quindi la blocco e avvio quella di selezione
            {
                StopCoroutine(coroutinesList[geometry]);
                coroutinesList[geometry] = StartCoroutine(animateSelectionOverlayOut(geometry, overlay));
            }
        }
    }
    #endregion
}
