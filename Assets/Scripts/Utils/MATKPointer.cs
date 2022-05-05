using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Class to set a custom pointer (used with the Dock system and with frozen geometries). It handles the generation of points for the line renderer, the raycast and events system on focus actions
/// </summary>
[RequireComponent(typeof(LineRenderer))]
public class MATKPointer : MonoBehaviour
{
    [Header("Settings")]
    //[SerializeField][Tooltip("Current object on focus by the pointer")] private GameObject currentObjectOnFocus;
    //[Space(12)]
    [Tooltip("Color used by default, when no object is on focus")] public Gradient defaultPointerColor;
    [Tooltip("Color used when an object is on focus")] public Gradient pointerColorlOnRaycastHit;
    [Tooltip("Layers which defines which objects can be selected")] public LayerMask layerMask;
    [Tooltip("Marker when the line hit an object")] public GameObject hitMarker;
    [Tooltip("Line lenght when at rest")] public float lineLength = 20f;
    [Tooltip("Max raycast lenght to detected objects. It's independent from the visual length of the pointer")] public float maxRaycastLength = 250;

    [Space(15)]
    [Header("Line settings")]
    [Tooltip("Line width")] public AnimationCurve lineWidth;
    [Space(8)]
    [Tooltip("Inertia applied to line points when moving")] public float inertia = 4f;
    [Tooltip("Dampen applied to line points when moving")] public float dampen = 5f;
    [Tooltip("Dampen applied to line points when moving")] public float strenght = 1f;

    [Space(15)]
    [Header("Events")]
    [Tooltip("Event invoked when something gets focused by the pointer")] public PointerEvent onFocus;
    [Tooltip("Event invoked when something gets selected by the pointer")] public PointerEvent onSelection;
    [Tooltip("Event invoked when the focused object of the pointer changes")] public PointerEvent onLostFocus;

    #region Fields
    private int linePoints = 8;
    private Transform startPosition;
    private Transform endPosition;
    private List<Transform> points = new List<Transform>();
    private LineRenderer lineRenderer = null;

    private Vector3[] velocityVector = new Vector3[10];
    public float[] inertiaMultiplier = {0.80f, 0.64f, 0.56f, 0.49f, 0.42f, 0.44f, 0.48f, 0.54f, 0.61f, 0.61f};

    private bool useInertia = false;
    private Vector3 prevHitPoint = Vector3.zero;
    #endregion
    //public GameObject CurrentObjectOnFocus
    //{
    //    get => currentObjectOnFocus;
    //    private set => currentObjectOnFocus = value;
    //}

    #region Unity methods
    // Start is called before the first frame update
    protected virtual void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        startPosition = transform.GetChild(0);
        endPosition = transform.GetChild(9);
        lineRenderer.positionCount = 10;
        CalculateStartPoints();
        MATKPointerManager.Instance.AddPointer(this);
    }

    protected virtual void OnEnable()
    {
        if (lineRenderer == null)
        {
            lineRenderer = GetComponent<LineRenderer>();
            startPosition = transform.GetChild(0);
            endPosition = transform.GetChild(9);
            lineRenderer.positionCount = 10;
        }
        StartCoroutine(StopInertiaWithTimer(1f));
        CalculateStartPoints();
        if (linePoints > 0)
        {
            for (int i = 1; i <= linePoints; i++)
            {
                lineRenderer.SetPosition(i, transform.GetChild(i).transform.position);
            }
        }
        lineRenderer.SetPosition(0, startPosition.position);
        lineRenderer.SetPosition(9, endPosition.position);
        //Specific for PlatformOne Freeze Geometries
        onSelection.AddListener(FreezeGeometries.Instance.UnfreezeGeometry);
        onFocus.AddListener(FreezeGeometries.Instance.GeometryOnFocus);
        onLostFocus.AddListener(FreezeGeometries.Instance.GeometryOnLostFocus);
        if (MATKPointerManager.Instance.ActivePointerInScene == null)
        {
            MATKPointerManager.Instance.ActivePointerInScene = this;
        } else
        {
            if (MATKPointerManager.Instance.ActivePointerInScene != this)
            {
                MATKPointerManager.Instance.ActivePointerInScene.gameObject.SetActive(false);
                MATKPointerManager.Instance.ActivePointerInScene = this;
            }
        }
    }

    private void OnDisable()
    {
        if (MATKPointerManager.Instance.ActivePointerInScene == this)
        {
            MATKPointerManager.Instance.ActivePointerInScene = null;
        }
    }

    private void OnDestroy()
    {
        MATKPointerManager.Instance.RemovePointer(this);
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        if (linePoints > 0)
        {
            for (int i = 1; i <= linePoints; i++)
            {
                if(useInertia)
                {
                    Inertia(transform.GetChild(i).transform.position, lineRenderer.GetPosition(i), i);
                } else
                {
                    lineRenderer.SetPosition(i, transform.GetChild(i).transform.position);
                }
            }
        }
        if (useInertia)
        {
            Inertia(startPosition.position, lineRenderer.GetPosition(0), 0);
            Inertia(endPosition.position, lineRenderer.GetPosition(9), 9);
        } else
        {
            lineRenderer.SetPosition(0, startPosition.position);
            lineRenderer.SetPosition(9, endPosition.position);
        }
        lineRenderer.widthCurve = lineWidth;
    }

    protected virtual void FixedUpdate()
    {
        RaycastHit hit;
        if (Physics.Raycast(startPosition.position, Vector3.Normalize(endPosition.position - startPosition.position), out hit, maxRaycastLength, layerMask.value))
        {
            Debug.DrawRay(transform.position, Vector3.Normalize(endPosition.position - startPosition.position) * hit.distance, Color.yellow);
            ExtendPoints(Vector3.Magnitude(transform.InverseTransformDirection(startPosition.position - hit.point)));
            lineRenderer.colorGradient = pointerColorlOnRaycastHit;

            if (!hitMarker.activeSelf)
            {
                hitMarker.SetActive(true);
            }
            if (hit.point != prevHitPoint)
            {
                hitMarker.transform.position = hit.point + hit.normal.normalized * 0.004f;
                hitMarker.transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
                prevHitPoint = hit.point;
            }

            if (MATKPointerManager.Instance.CurrentObjectOnFocus != null)
            {
                if (hit.collider.gameObject.name != MATKPointerManager.Instance.CurrentObjectOnFocus.name)
                {
                    //onLostFocus.Invoke(MATKPointerManager.Instance.CurrentObjectOnFocus);
                    //MATKPointerManager.Instance.CurrentObjectOnFocus = hit.collider.gameObject;
                    //onFocus.Invoke(MATKPointerManager.Instance.CurrentObjectOnFocus);
                    StartCoroutine(StopAndWaitFocusChange(hit));
                }
            } else
            {
                MATKPointerManager.Instance.CurrentObjectOnFocus = hit.collider.gameObject;
                onFocus.Invoke(MATKPointerManager.Instance.CurrentObjectOnFocus);
            }
        } else
        {
            if (hitMarker.activeSelf)
            {
                CalculateStartPoints();
                hitMarker.SetActive(false);
            }
            lineRenderer.colorGradient = defaultPointerColor;
            
            if(MATKPointerManager.Instance.CurrentObjectOnFocus != null)
            {
                onLostFocus.Invoke(MATKPointerManager.Instance.CurrentObjectOnFocus);
                MATKPointerManager.Instance.CurrentObjectOnFocus = null;
            }
        }
    }
    #endregion

    /// <summary>
    /// Calculates the inertia applied to each line renderer vertex during translation and applies it to the corresponding vertex
    /// </summary>
    /// <param name="pos">The vertex destination position</param>
    /// <param name="prevPos">The vertex previous position</param>
    /// <param name="index">The reference gameobject index in the hierarchy</param>
    private void Inertia(Vector3 pos, Vector3 prevPos, int index)
    {
        //Vector3 targetVector = new Vector3(0, 0, Mathf.Lerp(0.25f, 1f, Mathf.Abs(1 - ((index - 4) / 4))));
        Vector3 offset = pos - prevPos;
        //Vector3 worldTarget = transform.TransformPoint(pos+targetVector[index]);
        //offset += worldTarget-prevPos;
        velocityVector[index] = Vector3.Lerp(velocityVector[index], offset, Time.deltaTime * inertia * inertiaMultiplier[index]);
        velocityVector[index] = Vector3.Lerp(velocityVector[index], Vector3.zero, Time.deltaTime * dampen);
        Vector3 retPos = prevPos + velocityVector[index];
        retPos = Vector3.Lerp(retPos, pos, strenght * Time.deltaTime);
        lineRenderer.SetPosition(index, retPos);
    }

    /// <summary>
    /// Based on the base lenght defined, this method calculates the correct position of each vertex of the line renderer, changing the position of reference vertex gameobjects
    /// </summary>
    private void CalculateStartPoints()
    {
        endPosition.localPosition = new Vector3(0, 0, lineLength);
        float equalDistance = ((endPosition.localPosition.z - startPosition.localPosition.z)/4) / lineRenderer.positionCount;
        for(int i = 1; i <= (lineRenderer.positionCount-2); i++)
        {
            transform.GetChild(i).transform.localPosition = new Vector3(0, 0, transform.GetChild(i - 1).localPosition.z + equalDistance*i);
        }
    }
    
    /// <summary>
    /// When the line renderer length is changed, this method recalculates its points to properly fit the new distance without losing its properties (points count and inertia)
    /// </summary>
    /// <param name="distance">The length of the line</param>
    private void ExtendPoints(float distance)
    {
        if (endPosition.localPosition.z - distance > 0)
        {
            useInertia = false;
        } else
        {
            useInertia = true;
        }

        endPosition.localPosition = new Vector3(0, 0, distance);
        float equalDistance = ((endPosition.localPosition.z - startPosition.localPosition.z)/4) / (lineRenderer.positionCount - 1);
        for (int i = 1; i <= (lineRenderer.positionCount - 2); i++)
        {
            transform.GetChild(i).transform.localPosition = new Vector3(0, 0, transform.GetChild(i - 1).localPosition.z + equalDistance*i);
        }
    }

    /// <summary>
    /// Public method to call the pointer Selection event by other objects (used when implementing custom gestures)
    /// </summary>
    public void InvokeSelectionEvent()
    {
        if (MATKPointerManager.Instance.CurrentObjectOnFocus != null)
        {
            onSelection.Invoke(MATKPointerManager.Instance.CurrentObjectOnFocus);
            MATKPointerManager.Instance.CurrentObjectOnFocus = null;
        }
    }

    /// <summary>
    /// Public method to call the pointer Lost Focus event by other objects (used when implementing custom gestures)
    /// </summary>
    public void LostFocusOnObject()
    {
        if (MATKPointerManager.Instance.CurrentObjectOnFocus != null)
        {
            onLostFocus.Invoke(MATKPointerManager.Instance.CurrentObjectOnFocus);
            MATKPointerManager.Instance.CurrentObjectOnFocus = null;
        }
    }

    /// <summary>
    /// Stop inertia applied to each line render vertex in order to avoid unexpected artifacts/glitches when rapidly changing points positions
    /// </summary>
    /// <param name="time">Time to stop the inertia by</param>
    /// <returns>IEnumerator</returns>
    private IEnumerator StopInertiaWithTimer(float time)
    {
        useInertia = false;
        yield return new WaitForSeconds(time);
        useInertia = true;
    }

    private IEnumerator StopAndWaitFocusChange(RaycastHit hit)
    {
        onLostFocus.Invoke(MATKPointerManager.Instance.CurrentObjectOnFocus);
        yield return new WaitForSeconds(0.15f);
        MATKPointerManager.Instance.CurrentObjectOnFocus = hit.collider.gameObject;
        onFocus.Invoke(MATKPointerManager.Instance.CurrentObjectOnFocus);
    }
}

/// <summary>
/// Custom pointer event class. It derives from a single argument Unity Event
/// </summary>
[System.Serializable]
public class PointerEvent : UnityEvent<GameObject> { 
}
