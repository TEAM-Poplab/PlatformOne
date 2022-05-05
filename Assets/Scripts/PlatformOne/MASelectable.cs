using Microsoft.MixedReality.Toolkit;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;
using Microsoft.MixedReality.Toolkit.Experimental.UI;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.UI.BoundsControl;
using Bolt;
using Ludiq;
using Normal.Realtime;

/// <summary>
/// The class <c>Selectable</c> behaves similar to <see cref="Microsoft.MixedReality.Toolkit.Experimental.UI.Dockable"/>, but it handles also any aspect related to object manipulation while docked in the selection dock.
/// It is attached to the object which needs to implement any manipulation event while docked in the special selection dock.
/// </summary>
/// <remarks>This script in order to work properly requires:
/// <list type="bullet">
/// <item>a <see cref="SelectorDockPosition"/> object (i.e: a dock)</item>
/// </list>
/// </remarks>

public class MASelectable : MonoBehaviour
{
    #region Public fields
    [SerializeField]
    [Tooltip("Time to animate any move/scale into or out of the dock.")]
    private float moveLerpTime = 0.1f;

    [SerializeField]
    [Tooltip("Time to animate an element when it's following the dock (use 0 for tight attachment)")]
    private float moveLerpTimeWhenDocked = 0.05f;

    public bool BlockScaleToFit = false;
    #endregion

    #region Fields
    public SelectionState currentState = SelectionState.UNDOCKED;
    private SelectorDockPosition dockedPosition = null;
    private Vector3 dockedPositionScale = Vector3.one;
    private bool isDragging = false;
    public Material originalMaterial;

    // Constants
    private const float DistanceTolerance = 0.01f; // in meters
    private const float AngleTolerance = 3.0f; // in degrees
    private const float ScaleTolerance = 0.01f; // in percentage

    public Vector3 originalScale = Vector3.one;
    public Quaternion originalRotation = Quaternion.identity;
    public Vector3 originalPositionInParent = Vector3.zero;
    #endregion

    #region Properties and public stuff
    public bool IsDragging
    {
        get => isDragging;
        set => isDragging = value;
    }

    public enum SelectionState
    {
        UNSELECTED,
        SELECTED,
        UNDOCKED,
        DOCKING,
        UNDOCKING,
        DOCKED,
        INSTANTIATED
    }
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (currentState == SelectionState.SELECTED || currentState == SelectionState.DOCKING)
        {

            var lerpTime = currentState == SelectionState.SELECTED ? moveLerpTimeWhenDocked : moveLerpTime;

            if (!isDragging)
            {
                // Don't override dragging
                transform.position = Solver.SmoothTo(transform.position, dockedPosition.transform.position, Time.deltaTime, lerpTime);
                transform.rotation = Solver.SmoothTo(transform.rotation, dockedPosition.transform.rotation, Time.deltaTime, lerpTime);
            }

            transform.localScale = Solver.SmoothTo(transform.localScale, dockedPositionScale, Time.deltaTime, lerpTime);

            if (VectorExtensions.CloseEnough(dockedPosition.transform.position, transform.position, DistanceTolerance) &&
                QuaternionExtensions.AlignedEnough(dockedPosition.transform.rotation, transform.rotation, AngleTolerance) &&
                AboutTheSameSize(dockedPositionScale.x, transform.localScale.x))
            {
                // Finished docking
                // Snap to position
                transform.position = dockedPosition.transform.position;
                transform.rotation = dockedPosition.transform.rotation;
                transform.localScale = dockedPositionScale;
            }
            if (currentState == SelectionState.DOCKING)
            {
                currentState = SelectionState.UNSELECTED;
                gameObject.SetActive(false);

            }
            if (currentState == SelectionState.SELECTED)
            {
                //Object rotation while docked
                transform.GetChild(0).rotation = Quaternion.AngleAxis(25 * Time.deltaTime, Vector3.up) * transform.GetChild(0).rotation;
            }

        } else if (currentState == SelectionState.UNDOCKING)
        {
            transform.localScale = Solver.SmoothTo(transform.localScale, originalScale, Time.deltaTime, moveLerpTime);

            if (AboutTheSameSize(originalScale.x, transform.localScale.x))
            {
                // Finished undocking
                currentState = SelectionState.UNDOCKED;

                // Snap to size
                transform.localScale = originalScale;
                transform.GetChild(0).localRotation = originalRotation;
                transform.GetChild(0).localPosition = originalPositionInParent;
            }
        }
    }

    /// <summary>
    /// Docks this object in a given <see cref="SelectorDockPosition"/>.
    /// </summary>
    /// <param name="position">The <see cref="SelectorDockPosition"/> where we'd like to dock this object.</param>
    public void Dock(SelectorDockPosition position)
    {
        dockedPosition = position;

        if (!BlockScaleToFit)
        {
            dockedPositionScale = transform.localScale;
            while (dockedPositionScale.x * GetComponent<BoxCollider>().size.x > 0.12f)
            {
                dockedPositionScale -= new Vector3(0.001f, 0.001f, 0.001f);
            }
        }

        if (currentState == SelectionState.UNDOCKED)
        {
            // Only register the original scale and rotation when first docking
            originalScale = transform.localScale;
            originalRotation = transform.GetChild(0).localRotation;
            originalPositionInParent = transform.GetChild(0).localPosition;
        }

        currentState = SelectionState.DOCKING;
    }

    private void InstantDock(SelectorDockPosition position)
    {
        dockedPosition = position;
        isDragging = false;

        if (!BlockScaleToFit)
        {
            dockedPositionScale = transform.localScale;
            while (dockedPositionScale.x * GetComponent<BoxCollider>().size.x > 0.12f)
            {
                dockedPositionScale -= new Vector3(0.001f, 0.001f, 0.001f);
            }
        }

        currentState = SelectionState.SELECTED;
    }

    public void Undock()
    {
        currentState = SelectionState.UNDOCKING;
    }

    #region Helpers
    private static bool AboutTheSameSize(float scale1, float scale2)
    {
        return Mathf.Abs(scale1 / scale2 - 1.0f) < ScaleTolerance;
    }

    public void SaveOriginalMaterial(Material mat)
    {
        originalMaterial = mat;
    }
    #endregion

    #region Manipulation Events
    public virtual void OnManipulationStarted(ManipulationEventData e)
    {
        isDragging = true;

        if (currentState == SelectionState.SELECTED)
        {
            dockedPosition.RemoveGeometry(gameObject);
            var newObj = Instantiate(gameObject, transform.position, transform.rotation, transform.parent);
            GeometrySetModule.SavedTransformDataToSavedTransformData(GetComponent<PlatformableMeshSequence>().savedCentroidTransformWhenPivoted, out newObj.GetComponent<PlatformableMeshSequence>().savedCentroidTransformWhenPivoted);
            GeometrySetModule.SavedTransformDataToSavedTransformData(GetComponent<PlatformableMeshSequence>().savedOriginTransform, out newObj.GetComponent<PlatformableMeshSequence>().savedOriginTransform);
            GeometrySetModule.SavedTransformDataToSavedTransformData(GetComponent<PlatformableMeshSequence>().savedOriginTransformUponFreeze, out newObj.GetComponent<PlatformableMeshSequence>().savedOriginTransformUponFreeze);
            GetComponent<MeshSequenceControllerImproved>().ChangeMaterialToMeshes(originalMaterial);    //restoring original material when undocked from the import dock
            Undock();
            newObj.name = gameObject.name;
            dockedPosition.AddGeometry(newObj);
            newObj.GetComponent<MASelectable>().InstantDock(dockedPosition);
            dockedPosition.VisualizeGeometryByObject(newObj);
            dockedPosition = null;
        }

        GetComponent<MASelectable>().enabled = false;
    }

    public virtual void OnManipulationEnded(ManipulationEventData e)
    {
        //    isDragging = false;

        //    if (CanPlatform)
        //    {
        //        if (platformDockPosition.IsOccupied)
        //        {
        //            if (platformDockPosition.PlatformedObject.GetComponent<Platformable>())
        //            {
        //                platformDockPosition.PlatformedObject.GetComponent<Platformable>().InstantUndock();
        //            }
        //            else
        //            {
        //                platformDockPosition.PlatformedObject.GetComponent<PlatformableMeshSequence>().InstantUndock();
        //            }
        //        }
        //        Dock();
        //    }

        //isDragging = false;
        //if (currentState == SelectionState.UNDOCKED)
        //{
        //    Dock(dockedPosition);
        //}
    }

    public virtual void OnManipulationStarted()
    {
        isDragging = true;

        if (currentState == SelectionState.SELECTED)
        {
            dockedPosition.RemoveGeometry(gameObject);
            var newObj = Instantiate(gameObject, transform.position, transform.rotation, transform.parent);
            StartCoroutine(PlayCloneEffect());
            dockedPosition.GetComponent<AudioSource>().Play();  //Cloning audio effect
            //dockedPosition.creationParticleFX.Play();
            GeometrySetModule.SavedTransformDataToSavedTransformData(GetComponent<PlatformableMeshSequence>().savedCentroidTransformWhenPivoted, out newObj.GetComponent<PlatformableMeshSequence>().savedCentroidTransformWhenPivoted);
            GeometrySetModule.SavedTransformDataToSavedTransformData(GetComponent<PlatformableMeshSequence>().savedOriginTransform, out newObj.GetComponent<PlatformableMeshSequence>().savedOriginTransform);
            GeometrySetModule.SavedTransformDataToSavedTransformData(GetComponent<PlatformableMeshSequence>().savedOriginTransformUponFreeze, out newObj.GetComponent<PlatformableMeshSequence>().savedOriginTransformUponFreeze);
            GetComponent<MeshSequenceControllerImproved>().ChangeMaterialToMeshes(originalMaterial);    //restoring original material when undocked from the import dock
            Undock();
            //StartCoroutine(PlayCloneEffect(newObj));  //Visual FX for cloning
            newObj.name = gameObject.name;
            dockedPosition.AddGeometry(newObj);
            newObj.GetComponent<MASelectable>().InstantDock(dockedPosition);
            dockedPosition.VisualizeGeometryByObject(newObj);
            dockedPosition = null;
        }

        GetComponent<MASelectable>().enabled = false;
    }

    public virtual void OnManipulationEnded()
    {
        //    isDragging = false;

        //    if (CanPlatform)
        //    {
        //        if (platformDockPosition.IsOccupied)
        //        {
        //            if (platformDockPosition.PlatformedObject.GetComponent<Platformable>())
        //            {
        //                platformDockPosition.PlatformedObject.GetComponent<Platformable>().InstantUndock();
        //            }
        //            else
        //            {
        //                platformDockPosition.PlatformedObject.GetComponent<PlatformableMeshSequence>().InstantUndock();
        //            }
        //        }
        //        Dock();
        //    }

        //isDragging = false;
        //if (currentState == SelectionState.UNDOCKED)
        //{
        //    Dock(dockedPosition);
        //}
    }

    //public virtual void OnManipulationEndedVoid(ManipulationEventData e)
    //{
    //    if (currentState == PlatformState.Unplatformed && GetComponent<Dockable>().CanDock)
    //    {
    //        transform.localScale = originalScale;

    //        foreach (DockPosition dp in GameObject.Find("Dock").GetComponent<Dock>().DockPositions)
    //        {
    //            if (!dp.IsOccupied)
    //            {
    //                GetComponent<Dockable>().Dock(dp);
    //                break;
    //            }
    //        }
    //    }
    //}

    //public virtual void OnManipulationEndedVoid()
    //{
    //    if (currentState == SelectionState.UNDOCKED && GetComponent<Dockable>().CanDock)
    //    {
    //        foreach (DockPosition dp in GameObject.Find("Dock").GetComponent<Dock>().DockPositions)
    //        {
    //            if (!dp.IsOccupied)
    //            {
    //                GetComponent<Dockable>().Dock(dp);
    //                break;
    //            }
    //        }
    //    }
    //}
    #endregion

    public IEnumerator PlayCloneEffect()
    {
        float t = 0;
        while(true)
        {
            if (GetComponent<PlatformableMeshSequence>() != null)
            {
                GetComponent<MeshSequenceControllerImproved>().GetFrameWhenDeockedMaterial().SetFloat("_EnvironmentColorThreshold", 1.5f * Mathf.Sin(Mathf.PI * t*1.42f) + 1.5f);
            } else
            {
                transform.GetChild(0).GetComponent<MeshRenderer>().material.SetFloat("_EnvironmentColorThreshold", 1.5f * Mathf.Sin(Mathf.PI * t) + 1.5f);
            }
            
            if (t >= 0.7f)
            {
                if (GetComponent<PlatformableMeshSequence>())
                {
                    GetComponent<MeshSequenceControllerImproved>().GetFrameWhenDeockedMaterial().SetFloat("_EnvironmentColorThreshold", 1.5f);
                }
                else
                {
                    transform.GetChild(0).GetComponent<MeshRenderer>().material.SetFloat("_EnvironmentColorThreshold", 1.5f);
                }
                yield break;
            } else
            {
                t += Time.deltaTime;
                yield return null;
            }
        }
    }

    private float SmoothTo(float source, float goal, float deltaTime, float lerpTime)
    {
        return Mathf.Lerp(source, goal, lerpTime.Equals(0.0f) ? 1f : deltaTime / lerpTime);
    }
}
