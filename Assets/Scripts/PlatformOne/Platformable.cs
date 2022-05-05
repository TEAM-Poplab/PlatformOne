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

/// <summary>
/// The class <c>Platformable</c> behaves similar to <see cref="Microsoft.MixedReality.Toolkit.Experimental.UI.Dockable"/>, but it handles also any aspect related to object manipulation while docked.
/// It is attached to the object which needs to implement any manipulation event while docked in the special projection dock.
/// <para>It also implements the function to project the docked object</para>
/// </summary>
/// <remarks>This script in order to work properly requires:
/// <list type="bullet">
/// <item>a <see cref="PlatformDockPosition"/> object (i.e: a dock)</item>
/// <item>A <see cref="PlatformPlaceholder"/> object where the projected objects must appear</item>
/// </list>
/// </remarks>
public class Platformable : MonoBehaviour
{
    //Properties and public fields
    [SerializeField]
    [Tooltip("Time to animate any move/scale into or out of the platform.")]
    protected float moveLerpTime = 0.1f;

    public bool CanPlatform => platformState == PlatformState.Unplatformed || platformState == PlatformState.Unplatforming || platformState == PlatformState.Frozen;

    public bool CanUnplatform => platformState == PlatformState.Platformed || platformState == PlatformState.Platforming || platformState == PlatformState.Unfrozen;

    // Constants
    protected static readonly float distanceTolerance = 0.01f; // in meters
    protected static readonly float angleTolerance = 3.0f; // in degrees
    protected static readonly float scaleTolerance = 0.01f; // in percentage

    //Fields
    [HideInInspector] public PlatformState platformState = PlatformState.Unplatformed;

    [HideInInspector] public PlatformDockPosition platformDockPosition = null;
    [HideInInspector] public PlatformPlaceholder platformPlaceholder = null;
    [HideInInspector] public PlatformPlaceholderMeshSequence platformPlaceholderMS = null;
    [HideInInspector] public Vector3 platformPositionScale = new Vector3(0.04f, 0.04f, 0.04f);

    [HideInInspector] public GameObject miniScene = null;
    [HideInInspector] public GameObject miniSceneCenter = null;

    [HideInInspector] public GeometrySetModule.savedTranform savedCentroidTransformWhenPivoted;
    [HideInInspector] public GeometrySetModule.savedTranform savedOriginTransform;
    [HideInInspector] public GeometrySetModule.savedTranform savedOriginTransformUponFreeze;

    public Vector3 originalScale = Vector3.one;
    [HideInInspector] public bool isDragging = false;

    [HideInInspector] public GameObject objectMeshGameObject;

    [HideInInspector] public Dock dockObject;

    [HideInInspector] public Material originalMaterial;

    [HideInInspector] public Material realityOnMaterial;
    [HideInInspector] public Material realityOffMaterial;

    [HideInInspector] public CustomBoundsControl boundsControl;
    [HideInInspector] public float deltaAngle = 0; //Angle between original distance vector from platform and objectplaceholder, and current distance vector

    [HideInInspector] public TrashBinDockPositionTrigger trashBin;

    public GeometrySetModule.savedTranform SavedCentroidTransformWhenPivoted
    {
        get => savedCentroidTransformWhenPivoted;
        set => savedCentroidTransformWhenPivoted = value;
    }

    public GeometrySetModule.savedTranform SavedOriginTransform
    {
        get => savedOriginTransform;
        set => savedOriginTransform = value;
    }

    public GeometrySetModule.savedTranform SavedOriginTransformUponFreeze
    {
        get => savedOriginTransformUponFreeze;
        set => savedOriginTransformUponFreeze = value;
    }

    public PlatformState Platformstate
    {
        get => platformState;
    }

    public bool IsFrozen
    {
        get;
        protected set;
    }

    public GeometryType typeOfGeometry {
        get;
        set;
    }

    public enum PlatformState
    {
        Unplatformed,
        Platformed,
        Platforming,
        Unplatforming,
        Frozen,
        Frozing,
        Unfrozen,
        Unfrozing
    }

    public enum GeometryType
    {
        Single,
        Sequence
    }

    protected virtual void OnEnable()
    {
        if (IsFrozen)
        {
            OnUnfreezeEvent();
        }
    }

    protected virtual void Awake()
    {
        miniScene = GameObject.Find("GuardianCenter/Dock/PlatformDockPosition/MiniScene");
        platformDockPosition = GameObject.Find("GuardianCenter/Dock/PlatformDockPosition").GetComponent<PlatformDockPosition>();
        platformPlaceholder = GameObject.Find("GuardianCenter/ObjectPlaceholder").GetComponent<PlatformPlaceholder>();
        platformPlaceholderMS = GameObject.Find("GuardianCenter/ObjectPlaceholderMS").GetComponent<PlatformPlaceholderMeshSequence>();
        trashBin = GameObject.Find("GuardianCenter/Dock/TrashBinDockPosition").GetComponentInChildren<TrashBinDockPositionTrigger>();
        dockObject = GameObject.Find("GuardianCenter/Dock").GetComponent<Dock>();
        boundsControl = GetComponent<CustomBoundsControl>();
        IsFrozen = false;
    }

    protected virtual void Start()
    {
        //GameManager.Instance.OnDayLightSet.AddListener(OnDayLightSetMaterial);
        //GameManager.Instance.OnNightLightSet.AddListener(OnNightLightSetMaterial);
        typeOfGeometry = GeometryType.Single;
    }

    public virtual void Update()
    {
        if (platformState == PlatformState.Platforming)
        {
            transform.localPosition = Solver.SmoothTo(transform.localPosition, savedCentroidTransformWhenPivoted.position, Time.deltaTime, moveLerpTime);
            transform.localRotation = Solver.SmoothTo(transform.localRotation, savedCentroidTransformWhenPivoted.rotation, Time.deltaTime, moveLerpTime);
            transform.localScale = Solver.SmoothTo(transform.localScale, Vector3.one, Time.deltaTime, moveLerpTime);
            objectMeshGameObject.transform.localRotation = Solver.SmoothTo(objectMeshGameObject.transform.localRotation, GetComponent<Dockable>().OriginalRotation, Time.deltaTime, moveLerpTime);

            if (VectorExtensions.CloseEnough(savedCentroidTransformWhenPivoted.position, transform.localPosition, distanceTolerance) &&
                QuaternionExtensions.AlignedEnough(savedCentroidTransformWhenPivoted.rotation, transform.localRotation, angleTolerance) &&
                AboutTheSameSize(Vector3.one.x, transform.localScale.x))
            {
                // Finished docking
                platformState = PlatformState.Platformed;

                // Snap to position
                transform.localPosition = savedCentroidTransformWhenPivoted.position;
                transform.localRotation = savedCentroidTransformWhenPivoted.rotation;
                transform.localScale = savedCentroidTransformWhenPivoted.scale;

                platformPlaceholder.EnableLiveChanges = false; //it is set here because all animations have eneded and changes can be synced without interferences

                //platformDockPosition.ChangeScale(BoundsExtensions.GetScaleToFitInside(GetComponent<BoxCollider>().bounds, platformDockPosition.FittingBounds), 0.5f);
                float maxGeometryLossyScale = GeometrySetModule.SizeGreaterThan(transform.lossyScale.x, transform.lossyScale.y, transform.lossyScale.z);
                float maxBoundsSize = GeometrySetModule.SizeGreaterThan(GetComponent<BoxCollider>().size.x, GetComponent<BoxCollider>().size.y, GetComponent<BoxCollider>().size.z);
                while (maxGeometryLossyScale * maxBoundsSize < 0.135f || maxGeometryLossyScale * maxBoundsSize > 0.145f)
                {
                    if (maxGeometryLossyScale * maxBoundsSize > 0.145f)
                    {
                        platformDockPosition.ReduceScale(0.01f);
                        maxGeometryLossyScale = GeometrySetModule.SizeGreaterThan(transform.lossyScale.x, transform.lossyScale.y, transform.lossyScale.z);
                    }
                    else if (maxGeometryLossyScale * maxBoundsSize < 0.135f)
                    {
                        platformDockPosition.ReduceScale(-0.01f);
                        maxGeometryLossyScale = GeometrySetModule.SizeGreaterThan(transform.lossyScale.x, transform.lossyScale.y, transform.lossyScale.z);
                    }
                }

                //StartCoroutine(ChangeScale());
                FreezeGeometries.Instance.frozenGeometryEvent.AddListener(OnFreezeEvent);

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
                miniSceneCenter.transform.rotation = platformPlaceholder.transform.GetChild(0).rotation;
            }
        }
    }

    public virtual void Dock()
    {
        if (!CanPlatform)
        {
            Debug.LogError($"Trying to dock an object that was not undocked. State = {platformState}");
            return;
        }

        objectMeshGameObject = transform.GetChild(0).gameObject;

        platformDockPosition.PlatformedObject = gameObject;

        if (platformState == PlatformState.Unplatformed)
        {
            // Only register the original scale and material when first docking
            originalScale = transform.localScale;
            originalMaterial = objectMeshGameObject.GetComponent<MeshRenderer>().material;
        }

        //Setting origin system for manipulation
        miniSceneCenter = new GameObject();
        miniSceneCenter.name = "GeometryOrigin";
        miniSceneCenter.transform.parent = miniScene.transform;
        miniSceneCenter.transform.localPosition = savedOriginTransform.position;
        miniSceneCenter.transform.localScale = Vector3.one;
        miniSceneCenter.transform.rotation = platformPlaceholder.transform.rotation;
        transform.parent = miniSceneCenter.transform;

        platformDockPosition.onObjectPlatformed.Invoke(gameObject);


        //Changing material when platformed
        objectMeshGameObject.GetComponent<MeshRenderer>().material = platformDockPosition.materialWhenPlatformed;

        platformState = PlatformState.Platforming;
    }

    public virtual void Undock()
    {
        if (!CanUnplatform)
        {
            Debug.LogError($"Trying to undock an object that was not docked. State = {platformState}");
            return;
        }

        Debug.Log($"Undocking object {gameObject.name} from position {platformDockPosition.gameObject.name}");

        platformDockPosition.PlatformedObject = null;
        platformDockPosition.onObjectUnplatformed.Invoke();

        objectMeshGameObject.GetComponent<MeshRenderer>().material = originalMaterial;

        platformPlaceholder.EnableLiveChanges = false; //it is set here because all animations have not started yet and changes can be synced without interferences

        platformState = PlatformState.Unplatforming;

        //Setting origin system for manipulation
        transform.parent = null;
        Destroy(miniSceneCenter);
        miniSceneCenter = null;
    }

    public virtual void InstantUndock()
    {
        platformDockPosition.PlatformedObject = null;
        platformDockPosition.onObjectUnplatformed.Invoke();
        platformState = PlatformState.Unplatformed;
        platformDockPosition.RestoreScale();

        //Removing rotation handles
        //transform.GetChild(0).gameObject.GetComponent<CustomBoundsControl>().DestroyBounds();

        //Setting origin system for manipulation
        transform.parent = null;
        Destroy(miniSceneCenter);
        miniSceneCenter = null;

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
                break;
            }
        }
        objectMeshGameObject.GetComponent<MeshRenderer>().material = originalMaterial;
        if (GetComponent<Dockable>().CanDock)
        {
            //CustomEvent.Trigger(GameManager.Instance.gameObject, "ForcedGrabOutRange");
            //Destroy(gameObject);
            OnManipulationEndedDestroy();
        }
    }

    public virtual void DeleteGeometryUponFreezeEvent()
    {
        //Safely removing anything related to the geometry before destroying the geometry
        platformDockPosition.PlatformedObject = null;
        platformPlaceholder.EnableLiveChanges = false;
        if (boundsControl.HandlesContainer != null)
        {
            boundsControl.DestroyBounds();
            GetComponent<CustomBoxDisplay>().DestroyBoxDisplay();
            GetComponent<ValueLabelsVisualizer>().DestroyLabels();
        }
        platformDockPosition.onObjectUnplatformed.Invoke();
        platformState = PlatformState.Unplatformed;
        transform.parent = null;
        Destroy(miniSceneCenter);
        miniSceneCenter = null;
        Destroy(gameObject);
    }

    protected virtual void OnFreezeEvent()
    {
        platformDockPosition.PlatformedObject = null;
        platformPlaceholder.EnableLiveChanges = false;
        platformDockPosition.RestoreScale();
        if (boundsControl.HandlesContainer != null)
        {
            boundsControl.DestroyBounds();
            GetComponent<CustomBoxDisplay>().DestroyBoxDisplay();
            GetComponent<ValueLabelsVisualizer>().DestroyLabels();
        }
        StartCoroutine(ChangeAlphaUponFreezing());
    }

    protected virtual void OnUnfreezeEvent()
    {
        objectMeshGameObject.GetComponent<MeshRenderer>().material = originalMaterial;
        //foreach (DockPosition dp in dockObject.DockPositions)
        //{
        //    if (!dp.IsOccupied)
        //    {
        //        if (boundsControl.HandlesContainer != null)
        //        {
        //            boundsControl.DestroyBounds();
        //            GetComponent<CustomBoxDisplay>().DestroyBoxDisplay();
        //            GetComponent<ValueLabelsVisualizer>().DestroyLabels();
        //        }
        //        GetComponent<Dockable>().FastDock(dp);
        //        break;
        //    }
        //}
        if (platformDockPosition.IsOccupied)
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
        IsFrozen = false;
    }

    public virtual void DockAfterUnfreeze()
    {
        if (!CanPlatform)
        {
            Debug.LogError($"Trying to dock an object that was not undocked. State = {platformState}");
            return;
        }

        objectMeshGameObject = transform.GetChild(0).gameObject;

        platformDockPosition.PlatformedObject = gameObject;

        //Setting origin system for manipulation
        miniSceneCenter = new GameObject();
        miniSceneCenter.name = "GeometryOrigin";
        miniSceneCenter.transform.parent = miniScene.transform;
        miniSceneCenter.transform.localPosition = savedOriginTransformUponFreeze.position;
        miniSceneCenter.transform.localScale = savedOriginTransformUponFreeze.scale;
        miniSceneCenter.transform.rotation = savedOriginTransformUponFreeze.rotation;
        transform.parent = miniSceneCenter.transform;
        transform.localPosition = savedCentroidTransformWhenPivoted.position;
        transform.localRotation = savedCentroidTransformWhenPivoted.rotation;
        transform.localScale = savedCentroidTransformWhenPivoted.scale;

        platformDockPosition.onObjectPlatformedAfterFreeze.Invoke(gameObject);

        platformState = PlatformState.Platformed;

        FreezeGeometries.Instance.frozenGeometryEvent.AddListener(OnFreezeEvent);

        //Creating rotation handles
        StartCoroutine(InitBoundsAfterFreezeDelay(1f));
    }

    public virtual void OnManipulationStarted(ManipulationEventData e)
    {
        isDragging = true;

        if (CanUnplatform)
        {
            Undock();
        }
    }

    public virtual void OnManipulationEnded(ManipulationEventData e)
    {
        isDragging = false;

        if (CanPlatform)
        {
            if (platformDockPosition.IsOccupied)
            {
                if (platformDockPosition.PlatformedObject.GetComponent<Platformable>())
                {
                    platformDockPosition.PlatformedObject.GetComponent<Platformable>().InstantUndock();
                } else
                {
                    platformDockPosition.PlatformedObject.GetComponent<PlatformableMeshSequence>().InstantUndock();
                }
            }
            Dock();
        }
    }

    public virtual void OnManipulationStarted()
    {
        isDragging = true;

        if (CanUnplatform)
        {
            Undock();
        }
    }

    public virtual void OnManipulationEnded()
    {
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

    public virtual void OnManipulationEndedVoid(ManipulationEventData e)
    {
        if (platformState == PlatformState.Unplatformed && GetComponent<Dockable>().CanDock)
        {
            transform.localScale = originalScale;

            foreach (DockPosition dp in GameObject.Find("Dock").GetComponent<Dock>().DockPositions)
            {
                if (!dp.IsOccupied)
                {
                    GetComponent<Dockable>().Dock(dp);
                    break;
                }
            }
        }
    }

    public virtual void OnManipulationEndedVoid()
    {
        if (platformState == PlatformState.Unplatformed && GetComponent<Dockable>().CanDock)
        {
            foreach (DockPosition dp in GameObject.Find("Dock").GetComponent<Dock>().DockPositions)
            {
                if (!dp.IsOccupied)
                {
                    GetComponent<Dockable>().Dock(dp);
                    break;
                }
            }
        }
        if (GetComponent<Dockable>().CanDock)
        {
            //CustomEvent.Trigger(GameManager.Instance.gameObject, "ForcedGrabOutRange");
            //Destroy(gameObject);
            OnManipulationEndedDestroy();
        }
    }

    public virtual void OnManipulationEndedDestroy()
    {
        CustomEvent.Trigger(GameManager.Instance.gameObject, "ForcedGrabOutRange");
        StartCoroutine(DestroyAnimation());
    }

    protected virtual void OnDayLightSetMaterial()
    {
        if (platformState != PlatformState.Platformed)
        {
            objectMeshGameObject.GetComponent<MeshRenderer>().material = realityOnMaterial;
        }
        else
        {
            originalMaterial = realityOnMaterial;
        }
    }

    protected virtual void OnNightLightSetMaterial()
    {
        if (platformState != PlatformState.Platformed)
        {
            objectMeshGameObject.GetComponent<MeshRenderer>().material = realityOffMaterial;
        }
        else
        {
            originalMaterial = realityOffMaterial;
        }
    }

    protected static bool AboutTheSameSize(float scale1, float scale2)
    {
        return Mathf.Abs(scale1 / scale2 - 1.0f) < scaleTolerance;
    }

    protected virtual IEnumerator InitBoundsAfterDelay(float seconds)
    {
        yield return new WaitForSeconds(seconds);

        //The object may be unplatformed while the coroutine has not eneded yet, resulting handles to appear when in a Dock. So we check if the object is still platformed
        if (platformState == PlatformState.Platformed)
        {
            boundsControl.InitBounds();
            //var boxExtends = Vector3.Scale(GetComponent<BoxCollider>().bounds.size, transform.localScale) +
            //    Vector3.Scale(Vector3.Scale(GetComponent<BoxCollider>().bounds.size, transform.localScale), transform.GetChild(1).localScale); 
            var boxExtends = Vector3.Scale(GetComponent<BoxCollider>().bounds.size, VectorOfMaxComponent(transform.localScale)) +
                Vector3.Scale(Vector3.Scale(GetComponent<BoxCollider>().bounds.size, VectorOfMaxComponent(transform.localScale)), VectorOfMaxComponent(transform.GetChild(1).localScale));
            GetComponent<CustomBoxDisplay>().AddBoxDisplay(transform, boxExtends);
            yield return new WaitForSeconds(0.5f);
            GetComponent<ValueLabelsVisualizer>().CreateLabels();
        } 
    }

    protected virtual IEnumerator InitBoundsAfterFreezeDelay(float seconds)
    {
        yield return new WaitForSeconds(seconds);

        //The object may be unplatformed while the coroutine has not eneded yet, resulting handles to appear when in a Dock. So we check if the object is still platformed
        if (platformState == PlatformState.Platformed)
        {
            GetComponent<CustomBoxDisplay>().EnableBoxDisplay();
            yield return new WaitForSeconds(0.5f);
            GetComponent<ValueLabelsVisualizer>().EnableLabels();
        }
    }

    protected IEnumerator ChangeScale()
    {
        bool hasDone = false;
        float scaleFactor = BoundsExtensions.GetScaleToFitInside(GetComponent<BoxCollider>().bounds, platformDockPosition.FittingBounds);
        Vector3 destScale = platformDockPosition.MiniScene.transform.localScale * scaleFactor;

        while (!hasDone)
        {
            platformDockPosition.MiniScene.transform.localScale = Solver.SmoothTo(platformDockPosition.MiniScene.transform.localScale, destScale, Time.deltaTime, 0.5f);

            if (AboutTheSameSize(platformDockPosition.MiniScene.transform.localScale.x, destScale.x)) { }
            {
                platformDockPosition.ChangeScale(scaleFactor, 0.5f);
                hasDone = true;
            }

            yield return null;
        }
    }

    protected virtual IEnumerator ChangeAlphaUponFreezing()
    {
        Material mat = transform.GetChild(0).GetComponent<MeshRenderer>().material;
        Color color = mat.GetColor("_Color");
        float alpha = color.a;
        float timer = 0f;
        while (true)
        {
            mat.SetColor("_Color", new Color(color.r, color.g, color.b, Mathf.Lerp(alpha, 0, timer * (1 / 0.5f))));
            if (timer * (1 / 0.5f) >= 1f)
            {
                GeometrySetModule.SaveLocalTransformToSavedTransformData(transform.parent, out savedOriginTransformUponFreeze);
                GeometryStatusSaver.Instance.SaveTransformValue(gameObject, platformPlaceholder.InstantiatedObject.transform.localPosition, platformPlaceholder.InstantiatedObject.transform.localRotation, platformPlaceholder.InstantiatedObject.transform.localScale);
                platformDockPosition.onObjectUnplatformed.Invoke();
                platformState = PlatformState.Unplatformed;
                transform.parent = null;
                Destroy(miniSceneCenter);
                miniSceneCenter = null;
                transform.localScale = originalScale;
                FreezeGeometries.Instance.frozenGeometryEvent.RemoveListener(OnFreezeEvent);
                IsFrozen = true;
                gameObject.SetActive(false);
                yield break;
            }
            timer += Time.deltaTime;
            yield return null;
        }
    }

    protected virtual IEnumerator DestroyAnimation()
    {
        trashBin?.onDeleteEvent.Invoke();
        if (boundsControl.HandlesContainer != null)
        {
            boundsControl.DestroyBounds();
            GetComponent<CustomBoxDisplay>().DestroyBoxDisplay();
            GetComponent<ValueLabelsVisualizer>().DestroyLabels();
        }
        while (true)
        {
            transform.position = Solver.SmoothTo(transform.position, trashBin.transform.parent.position, Time.deltaTime, 0.1f);
            transform.localScale = Solver.SmoothTo(transform.localScale, Vector3.zero, Time.deltaTime, 0.1f);
            if (transform.localScale.x < 0.0003f)
            {
                trashBin?.onDeleteAndDestroyEvent.Invoke();
                Destroy(gameObject);
                yield break;
            }
            yield return null;
        }
    }

    public static Vector3 VectorOfMaxComponent(Vector3 vector)
    {
        float maxValue = Mathf.Max(Mathf.Max(vector.x, vector.y), vector.z);
        return new Vector3(maxValue, maxValue, maxValue);
    }
    
    public virtual void EnableChanges(bool status) {
        Debug.Log("Changes are " + (status ? "enabled" : "disabled"));
        platformPlaceholder.EnableLiveChanges = status;
    }
}
