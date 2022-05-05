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

/// <inheritdoc/>
/// <remarks>This class extension supports the docking onto the platform dock of a mesh sequence object</remarks>
public class PlatformableMeshSequence : Platformable
{
    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        typeOfGeometry = GeometryType.Sequence;
    }

    public override void Update()
    {
        if (platformState == PlatformState.Platforming)
        {
            Debug.Log("Platforming mesh sequence: 1");
            transform.localPosition = Solver.SmoothTo(transform.localPosition, savedCentroidTransformWhenPivoted.position, Time.deltaTime, moveLerpTime);
            transform.localRotation = Solver.SmoothTo(transform.localRotation, savedCentroidTransformWhenPivoted.rotation, Time.deltaTime, moveLerpTime);
            transform.localScale = Solver.SmoothTo(transform.localScale, Vector3.one, Time.deltaTime, moveLerpTime);
            objectMeshGameObject.transform.localRotation = Solver.SmoothTo(objectMeshGameObject.transform.localRotation, GetComponent<Dockable>().OriginalRotation, Time.deltaTime, moveLerpTime);

            Debug.Log("Platforming mesh sequence: 2");
            Debug.Log(savedCentroidTransformWhenPivoted.position + " : " + savedCentroidTransformWhenPivoted.rotation + " : " + savedCentroidTransformWhenPivoted.scale);
            if (VectorExtensions.CloseEnough(savedCentroidTransformWhenPivoted.position, transform.localPosition, distanceTolerance) &&
                QuaternionExtensions.AlignedEnough(savedCentroidTransformWhenPivoted.rotation, transform.localRotation, angleTolerance) &&
                AboutTheSameSize(Vector3.one.x, transform.localScale.x))
            {
                // Finished docking
                platformState = PlatformState.Platformed;

                Debug.Log("Platforming mesh sequence: 3");
                // Snap to position
                transform.localPosition = savedCentroidTransformWhenPivoted.position;
                transform.localRotation = savedCentroidTransformWhenPivoted.rotation;
                transform.localScale = savedCentroidTransformWhenPivoted.scale;

                platformPlaceholderMS.EnableLiveChanges = false; //it is set here because all animations have eneded and changes can be synced without interferences

                Debug.Log("Platforming mesh sequence: 4");
                //platformDockPosition.ChangeScale(BoundsExtensions.GetScaleToFitInside(GetComponent<BoxCollider>().bounds, platformDockPosition.FittingBounds), 0.5f);
                float maxGeometryLossyScale = GeometrySetModule.SizeGreaterThan(transform.GetChild(0).lossyScale.x, transform.GetChild(0).lossyScale.y, transform.GetChild(0).lossyScale.z);
                float maxBoundsSize = GeometrySetModule.SizeGreaterThan(GetComponent<BoxCollider>().size.x, GetComponent<BoxCollider>().size.y, GetComponent<BoxCollider>().size.z);
                float minGeometryLossyScale = GeometrySetModule.SizeLowerThan(transform.GetChild(0).lossyScale.x, transform.GetChild(0).lossyScale.y, transform.GetChild(0).lossyScale.z);
                float minBoundsSize = GeometrySetModule.SizeLowerThan(GetComponent<BoxCollider>().size.x, GetComponent<BoxCollider>().size.y, GetComponent<BoxCollider>().size.z);
                while (minGeometryLossyScale * minBoundsSize < 0.135f)
                {
                    platformDockPosition.ReduceScale(-0.01f);
                    minGeometryLossyScale = GeometrySetModule.SizeLowerThan(transform.GetChild(0).lossyScale.x, transform.GetChild(0).lossyScale.y, transform.GetChild(0).lossyScale.z);
                }

                while (maxGeometryLossyScale * maxBoundsSize > 0.145f)
                {
                    platformDockPosition.ReduceScale(0.01f);
                    maxGeometryLossyScale = GeometrySetModule.SizeGreaterThan(transform.GetChild(0).lossyScale.x, transform.GetChild(0).lossyScale.y, transform.GetChild(0).lossyScale.z);
                }

                Debug.Log("Platforming mesh sequence: 5");
                //StartCoroutine(ChangeScale());
                FreezeGeometries.Instance.frozenGeometryEvent.AddListener(OnFreezeEvent);

                Debug.Log("Platforming mesh sequence: 6");
                //Creating rotation handles
                StartCoroutine(InitBoundsAfterDelay(1f));
            }
        }
        else if (platformState == PlatformState.Unplatforming)
        {
            transform.localScale = Solver.SmoothTo(transform.localScale, originalScale, Time.deltaTime, moveLerpTime);

            if (boundsControl.HandlesContainer != null)
            {
                boundsControl.DestroyBounds();
                GetComponent<CustomBoxDisplay>().DestroyBoxDisplay();
                GetComponent<ValueLabelsVisualizer>().DestroyLabels();
            }

            if (AboutTheSameSize(originalScale.x, transform.localScale.x))
            {
                // Finished undocking
                platformState = PlatformState.Unplatformed;

                platformDockPosition.RestoreScale();

                // Snap to size
                transform.localScale = originalScale;

                FreezeGeometries.Instance.frozenGeometryEvent.RemoveListener(OnFreezeEvent);
            }
        }
        else if (platformState == PlatformState.Platformed)
        {
            if (!platformPlaceholder.EnableLiveChanges)
            {
                miniSceneCenter.transform.rotation = platformPlaceholderMS.transform.GetChild(0).rotation;
            }
        }
    }

    public override void Dock()
    {
        if (!CanPlatform)
        {
            Debug.LogError($"Trying to dock an object that was not undocked. State = {platformState}");
            return;
        }

        objectMeshGameObject = GetComponent<MeshSequenceControllerImproved>().GetCurrentFrame().transform.GetChild(0).gameObject;

        platformDockPosition.PlatformedObject = gameObject;

        if (platformState == PlatformState.Unplatformed)
        {
            // Only register the original scale and material when first docking
            originalScale = transform.localScale;
            originalMaterial = objectMeshGameObject.GetComponent<MeshRenderer>().material;
            Debug.Log("Saved material: " + originalMaterial);
        }

        //Setting origin system for manipulation
        deltaAngle = Vector3.SignedAngle(platformPlaceholderMS.transform.position - (new Vector3(0, 1, 0)), platformPlaceholderMS.transform.position - GameObject.Find("GuardianCenter/Platform").transform.position, Vector3.up);
        miniSceneCenter = new GameObject();
        miniSceneCenter.name = "GeometryOrigin";
        miniSceneCenter.transform.parent = miniScene.transform;
        miniSceneCenter.transform.localPosition = savedOriginTransform.position;
        miniSceneCenter.transform.localScale = Vector3.one;
        miniSceneCenter.transform.rotation = platformPlaceholderMS.transform.rotation;
        transform.parent = miniSceneCenter.transform;

        platformDockPosition.onObjectPlatformedMS.Invoke(gameObject);

        //Changing material when platformed
        GetComponent<MeshSequenceControllerImproved>().ChangeMaterialToMeshes(platformDockPosition.materialWhenPlatformed);
        GetComponent<MeshSequenceControllerImproved>().ActivateFrame(1);
        //objectMeshGameObject.GetComponent<MeshRenderer>().material = platformDockPosition.materialWhenPlatformed;

        platformState = PlatformState.Platforming;
    }

    public override void Undock()
    {
        if (!CanUnplatform)
        {
            Debug.LogError($"Trying to undock an object that was not docked. State = {platformState}");
            return;
        }

        Debug.Log($"Undocking object {gameObject.name} from position {platformDockPosition.gameObject.name}");

        platformDockPosition.PlatformedObject = null;
        platformDockPosition.onObjectUnplatformedMS.Invoke();

        GetComponent<MeshSequenceControllerImproved>().ChangeMaterialToMeshes(originalMaterial);
        GetComponent<MeshSequenceControllerImproved>().ResetForDock();

        platformPlaceholderMS.EnableLiveChanges = false; //it is set here because all animations have not started yet and changes can be synced without interferences

        platformState = PlatformState.Unplatforming;

        //Setting origin system for manipulation
        transform.parent = null;
        Destroy(miniSceneCenter);
        miniSceneCenter = null;
    }

    public override void InstantUndock()
    {
        platformDockPosition.PlatformedObject = null;
        platformDockPosition.onObjectUnplatformedMS.Invoke();
        platformState = PlatformState.Unplatformed;
        platformDockPosition.RestoreScale();

        GetComponent<MeshSequenceControllerImproved>().ChangeMaterialToMeshes(originalMaterial);

        //Setting origin system for manipulation
        transform.parent = null;
        Destroy(miniSceneCenter);
        miniSceneCenter = null;

        platformPlaceholderMS.EnableLiveChanges = false; //it is set here because all animations have not started yet and changes can be synced without interferences

        transform.localScale = originalScale;

        FreezeGeometries.Instance.frozenGeometryEvent.RemoveListener(OnFreezeEvent);

        foreach (DockPosition dp in dockObject.DockPositions)
        {
            if (!dp.IsOccupied)
            {
                if (boundsControl.HandlesContainer != null)
                {
                    boundsControl.DestroyBounds();
                    GetComponent<CustomBoxDisplay>().DestroyBoxDisplay();
                    GetComponent<ValueLabelsVisualizer>().DestroyLabels();
                }
                GetComponent<Dockable>().FastDock(dp);
                GetComponent<MeshSequenceControllerImproved>().ResetForDock();
                break;
            }
        }
        if (GetComponent<Dockable>().CanDock)
        {
            //CustomEvent.Trigger(GameManager.Instance.gameObject, "ForcedGrabOutRange");
            //Destroy(gameObject);
            StartCoroutine(DestroyAnimation());
        }
    }

    public override void OnManipulationStarted(ManipulationEventData e)
    {
        base.OnManipulationStarted(e);
    }

    public override void OnManipulationEnded(ManipulationEventData e)
    {
        base.OnManipulationEnded(e);
    }

    public override void OnManipulationStarted()
    {
        base.OnManipulationStarted();
    }

    public override void OnManipulationEnded()
    {
        //Here I should differentiate geometry mesh type because single and sequence meshes cohesists in platform dock interactions
        isDragging = false;

        if (CanPlatform)
        {
            if (platformDockPosition.IsOccupied)
            {
                if (platformDockPosition.PlatformedObject.GetComponent<Platformable>())
                {
                    platformDockPosition.PlatformedObject.GetComponent<Platformable>().InstantUndock();
                }
                else
                {
                    platformDockPosition.PlatformedObject.GetComponent<PlatformableMeshSequence>().InstantUndock();
                }
            }
            Dock();
        }
    }

    public override void OnManipulationEndedVoid(ManipulationEventData e)
    {
        base.OnManipulationEndedVoid(e);
    }

    public override void OnManipulationEndedVoid()
    {
        base.OnManipulationEndedVoid();
    }

    public override void OnManipulationEndedDestroy()
    {
        base.OnManipulationEndedDestroy();
    }

    protected override void OnDayLightSetMaterial()
    {
        base.OnDayLightSetMaterial();
    }

    protected override void OnNightLightSetMaterial()
    {
        base.OnNightLightSetMaterial();
    }

    protected override IEnumerator InitBoundsAfterDelay(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        Debug.Log("Platforming mesh sequence: init bounds 1");

        //The object may be unplatformed while the coroutine has not eneded yet, resulting handles to appear when in a Dock. So we check if the object is still platformed
        if (platformState == PlatformState.Platformed)
        {
            boundsControl.InitBounds();
            Debug.Log("Platforming mesh sequence: init bounds 2");
            var boxExtends = Vector3.Scale(VectorOfMaxComponent(GetComponent<BoxCollider>().bounds.size), VectorOfMaxComponent(transform.localScale))
               + Vector3.Scale(Vector3.Scale(VectorOfMaxComponent(GetComponent<BoxCollider>().bounds.size), VectorOfMaxComponent(transform.localScale)), VectorOfMaxComponent(transform.GetChild(1).localScale));
            Debug.Log("Platforming mesh sequence: init bounds 3");
            GetComponent<CustomBoxDisplay>().AddBoxDisplay(transform, boxExtends*(GetComponent<CustomBoundsControlMeshSequence>().boundsScale/6));
            Debug.Log("Platforming mesh sequence: init bounds 4");
            GetComponent<CustomBoxDisplay>().RotateBoxDisplay();
            Debug.Log("Platforming mesh sequence: init bounds 5");
            Variables.ActiveScene.Set("TranslationOffset", Vector3.zero);   //Reset the scene variable for projected object offset
            Debug.Log("Platforming mesh sequence: init bounds 6");
            yield return new WaitForSeconds(0.5f);
            Debug.Log("Platforming mesh sequence: init bounds 7");
            GetComponent<ValueLabelsVisualizer>().CreateLabels();
        }
    }

    public override void EnableChanges(bool status)
    {
        Debug.Log("Changes are " + (status ? "enabled" : "disabled"));
        platformPlaceholderMS.EnableLiveChanges = status;
    }

    #region Freeze/Unfreeze methods
    /// <summary>
    /// It's called upon a Freeze geometry event: the event is called after the projected object has been frozen, so this method safely remove the manipulable geometry from the scene
    /// </summary>
    public override void DeleteGeometryUponFreezeEvent()
    {
        //Safely removing anything related to the geometry before destroying the geometry
        platformDockPosition.PlatformedObject = null;
        platformPlaceholderMS.EnableLiveChanges = false;
        if (boundsControl.HandlesContainer != null)
        {
            GetComponent<CustomBoxDisplay>().DestroyBoxDisplay();
            GetComponent<ValueLabelsVisualizer>().DestroyLabels();
            boundsControl.SetBoundsActive(false);
        }
        platformDockPosition.onObjectUnplatformedMS.Invoke();
        platformState = PlatformState.Unplatformed;
        transform.parent = null;
        Destroy(miniSceneCenter);
        miniSceneCenter = null;
        GetComponent<MeshSequenceControllerImproved>().ChangeMaterialToMeshes(originalMaterial);
    }

    /// <summary>
    /// Registered to <see cref="FreezeGeometries"/> Frozen Geometry Event, and called when a projected geometry has been frozen. It ensures the proper undock for the docked geometry
    /// </summary>
    protected override void OnFreezeEvent()
    {
        GeometryStatusSaver.Instance.SaveMiniSceneScale(transform.parent.gameObject, miniScene.transform.localScale);
        platformDockPosition.PlatformedObject = null;
        platformPlaceholderMS.EnableLiveChanges = false;
        GeometryStatusSaver.Instance.SaveSliderValue(gameObject, GetComponent<MeshSequenceControllerImproved>().ActiveFrameIndex); //Saving the active geometry index
        if (boundsControl.HandlesContainer != null) //only disabling the box display and the bounds (because they're created correctly only when a new geometry is pltaformed)
        {
            GetComponent<CustomBoxDisplay>().DisableBoxDisplay();
            GetComponent<ValueLabelsVisualizer>().DestroyLabels(); //Deleting labels (they're properly created anyway)
            boundsControl.SetBoundsActive(false);
        }
        StartCoroutine(ChangeAlphaUponFreezing());
    }

    //protected override void OnUnfreezeEvent()
    //{
    //    GetComponent<MeshSequenceControllerImproved>().ChangeMaterialToMeshes(originalMaterial);
    //    foreach (DockPosition dp in dockObject.DockPositions)
    //    {
    //        if (!dp.IsOccupied)
    //        {
    //            if (boundsControl.HandlesContainer != null)
    //            {
    //                boundsControl.DestroyBounds();
    //                GetComponent<CustomBoxDisplay>().DestroyBoxDisplay();
    //                GetComponent<ValueLabelsVisualizer>().DestroyLabels();
    //            }
    //            GetComponent<Dockable>().FastDock(dp);
    //            GetComponent<MeshSequenceControllerImproved>().ResetForDock();
    //            break;
    //        }
    //    }
    //}

    /// <summary>
    /// Registered to <see cref="FreezeGeometries"/> Unfrozen Geometry Event, and called when a frozen geometry has been unfrozen. It ensures the proper dock for the restored geometry
    /// </summary>
    protected override void OnUnfreezeEvent()
    {
        GetComponent<MeshSequenceControllerImproved>().ChangeMaterialToMeshes(platformDockPosition.materialWhenPlatformed);
        platformPlaceholderMS.EnableLiveChanges = false;
        if (platformDockPosition.IsOccupied)    //Undocking any other geometry that is currently in the platform dock
        {
            if (platformDockPosition.PlatformedObject.GetComponent<PlatformableMeshSequence>())
            {
                platformDockPosition.PlatformedObject.GetComponent<PlatformableMeshSequence>().InstantUndock();
            }
            else
            {
                platformDockPosition.PlatformedObject.GetComponent<Platformable>().InstantUndock();
            }
            DockAfterUnfreeze();
        }
        else
        {
            DockAfterUnfreeze();
        }
        IsFrozen = false;   //now is changed after the platformed after unfreeze event has been called in order to properly recreate the projected geometry
    }


    /// <summary>
    /// Called at the end of the unfreeze event method listener, and properly dock the geometry back in the platform dock
    /// </summary>
    public override void DockAfterUnfreeze()
    {
        if (!CanPlatform)   //Emergency case: if unfrozen geometry cannot be platformed, it will be docked
        {
            foreach (DockPosition dp in dockObject.DockPositions)
            {
                if (!dp.IsOccupied)
                {
                    if (boundsControl.HandlesContainer != null)
                    {
                        boundsControl.DestroyBounds();
                        GetComponent<CustomBoxDisplay>().DestroyBoxDisplay();
                        GetComponent<ValueLabelsVisualizer>().DestroyLabels();
                    }
                    platformDockPosition.onObjectUnplatformedMS.Invoke();
                    GetComponent<Dockable>().FastDock(dp);
                    GetComponent<MeshSequenceControllerImproved>().ResetForDock();
                    break;
                }
            }
            if (GetComponent<Dockable>().CanDock)
            {
                StartCoroutine(DestroyAnimation());
            } else
            {
                return;
            }            
        }

        miniScene.transform.localScale = GeometryStatusSaver.Instance.GetMiniSceneScale(transform.parent.gameObject);

        objectMeshGameObject = GetComponent<MeshSequenceControllerImproved>().GetCurrentFrame().transform.GetChild(0).gameObject;

        platformDockPosition.PlatformedObject = gameObject;

        platformDockPosition.onObjectPlatformedMSAfterFreeze.Invoke(gameObject);

        platformState = PlatformState.Platformed;

        FreezeGeometries.Instance.frozenGeometryEvent.AddListener(OnFreezeEvent);

        StartCoroutine(InitBoundsAfterFreezeDelay(1f));
    }

    //protected override IEnumerator ChangeAlphaUponFreezing()
    //{
    //    Material mat = GetComponent<MeshSequenceControllerImproved>().GetCurrentFramesMaterial();
    //    Color color = mat.GetColor("_Color");
    //    float alpha = color.a;
    //    float timer = 0f;
    //    while (true)
    //    {
    //        mat.SetColor("_Color", new Color(color.r, color.g, color.b, Mathf.Lerp(alpha, 0, timer * (1 / 0.5f))));
    //        if (timer * (1 / 0.5f) >= 1f)
    //        {
    //                            platformDockPosition.onObjectUnplatformedMS.Invoke();
    //platformState = PlatformState.Unplatformed;
    //            transform.parent = null;
    //            Destroy(miniSceneCenter);
    //miniSceneCenter = null;
    //            transform.localScale = originalScale;
    //            FreezeGeometries.Instance.frozenGeometryEvent.RemoveListener(OnFreezeEvent);
    //            IsFrozen = true;
    //            gameObject.SetActive(false);
    //            yield break;
    ////        }
    //        timer += Time.deltaTime;
    //        yield return null;
    //    }
    //}

    /// <summary>
    /// The coroutine is started at the end of the freeze event method listener. It animates the geometry in the the projecion dock for fading out, saves last values and disable the geometry
    /// </summary>
    protected override IEnumerator ChangeAlphaUponFreezing()
    {
        Material mat = GetComponent<MeshSequenceControllerImproved>().GetCurrentFramesMaterial();
        Color color = mat.GetColor("_Color");
        float alpha = color.a;
        float timer = 0f;
        while (true)
        {
            mat.SetColor("_Color", new Color(color.r, color.g, color.b, Mathf.Lerp(alpha, 0, timer * (1 / 0.5f))));
            if (timer * (1 / 0.5f) >= 1f)
            {
                GeometrySetModule.SaveLocalTransformToSavedTransformData(transform.parent, out savedOriginTransformUponFreeze);
                GeometryStatusSaver.Instance.SaveTransformValue(transform.parent.gameObject, platformPlaceholderMS.InstantiatedObject.transform.localPosition, platformPlaceholderMS.InstantiatedObject.transform.localRotation, platformPlaceholderMS.InstantiatedObject.transform.localScale);
                platformState = PlatformState.Frozen;
                FreezeGeometries.Instance.frozenGeometryEvent.RemoveListener(OnFreezeEvent);
                GeometryStatusSaver.Instance.SaveProjectedGeometry(transform.parent.gameObject, platformPlaceholderMS.InstantiatedObject);
                platformPlaceholderMS.DisableProjectedObject();
                SliderManager.Instance.DisableCurrentSlider();
                IsFrozen = true;
                transform.parent.gameObject.SetActive(false);  //Disabling origin here, it will be enabled in the FrozenGeometries script
                yield break;
            }
            timer += Time.deltaTime;
            yield return null;
        }
    }

    protected override IEnumerator InitBoundsAfterFreezeDelay(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        //The object may be unplatformed while the coroutine has not eneded yet, resulting handles to appear when in a Dock. So we check if the object is still platformed
        if (platformState == PlatformState.Platformed)
        {
            boundsControl.SetBoundsActive(true);
            GetComponent<CustomBoxDisplay>().EnableBoxDisplay();
            yield return new WaitForSeconds(0.5f);
            GetComponent<ValueLabelsVisualizer>().CreateLabelsAfterFreeze();
            gameObject.GetComponent<ValueLabelsVisualizer>().UpdateMiniSceneLabel(GeometryStatusSaver.Instance.GetMiniSceneScale(transform.parent.gameObject));
        }

        GeometryStatusSaver.Instance.RemoveAllObjectValues(transform.parent.gameObject);    //Cleaning up
    }
    #endregion
}
