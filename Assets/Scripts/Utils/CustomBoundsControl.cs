using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Bolt;
using Ludiq;

/// <summary>
/// The class <c>CustomBoundsControl</c> is responsible for creating the bounds interaction handles wrapping around the <see cref="Platformable"/> object.
/// </summary>
public class CustomBoundsControl : MonoBehaviour
{
    public GameObject handlePrefab;
    public GameObject rotationHandlePrefab;
    public GameObject translateHandlePrefab;
    public float boundsScale = 1f;
    private float offset = 0.07f;

    public GameObject target = null;
    public Bounds targetBounds;
    [HideInInspector] public BoxCollider collider;

    [HideInInspector] public PlatformDockPosition platformDockPosition;

    [HideInInspector] public List<GameObject> scaleHandlesList = null;

    [HideInInspector] public List<GameObject> rotateHandlesList = null;

    [HideInInspector] public List<GameObject> translateHandlesList = null;

    [HideInInspector] public GameObject handlesContainer = null;

    [HideInInspector] public GameObject translationHandleContainer;

    public List<GameObject> ScaleHandlesList
    {
        get => scaleHandlesList;
    }

    public List<GameObject> RotateHandlesList
    {
        get => rotateHandlesList;
    }

    public List<GameObject> TranslateHandleList
    {
        get => translateHandlesList;
    }

    public GameObject HandlesContainer
    {
        get => handlesContainer;
    }

    [HideInInspector] public List<Quaternion> rotationForRotateHandle = new List<Quaternion>();
    [HideInInspector] public List<Quaternion> rotationForTranslateHandle = new List<Quaternion>();

    protected void Awake()
    {
        platformDockPosition = GameObject.Find("GuardianCenter/Dock/PlatformDockPosition").GetComponent<PlatformDockPosition>();
        translationHandleContainer = GameObject.Find("GuardianCenter/Dock/PlatformDockPosition/TranslationHandlesContainer");
    }

    protected void OnDisable()
    {
        //if (handlesContainer != null)
        //{
        //    DestroyBounds();
        //}
        SetBoundsActive(false);
    }

    // Start is called before the first frame update
    protected void Start()
    {
        //platformDockPosition.onObjectPlatformed.AddListener(InitBounds);
        //platformDockPosition.onObjectUnplatformed.AddListener(DestroyBounds);
        if (target == null)
        {
            target = gameObject;
        }

        InitAnglesForRotateHandle();
        InitAnglesForTranslateHandle();
    }

    protected virtual void DetermineTargetBounds()
    {
        if (collider != null)
        {
            Destroy(collider);
        }

        collider = target.transform.GetChild(0).gameObject.GetComponent<BoxCollider>();

        if (collider == null) {
            collider = target.transform.GetChild(0).gameObject.AddComponent<BoxCollider>();
        }

        Bounds bounds = collider.bounds;
        targetBounds.center = bounds.center;
        //targetBounds.size = bounds.size*boundsScale;
        targetBounds.size = Platformable.VectorOfMaxComponent(bounds.size) * boundsScale;

        collider.enabled = false;
    }

    protected List<Vector3> CalculateCorners()
    {
        List<Vector3> corners = new List<Vector3>();
        Vector3 c = targetBounds.center;
        Vector3 s = targetBounds.size;
        corners.Add(new Vector3(c.x - (s.x / 2), c.y + (s.y / 2), c.z - (s.z / 2)));
        corners.Add(new Vector3(c.x - (s.x / 2), c.y + (s.y / 2), c.z + (s.z / 2)));
        corners.Add(new Vector3(c.x - (s.x / 2), c.y - (s.y / 2), c.z - (s.z / 2)));
        corners.Add(new Vector3(c.x - (s.x / 2), c.y - (s.y / 2), c.z + (s.z / 2)));
        corners.Add(new Vector3(c.x + (s.x / 2), c.y + (s.y / 2), c.z - (s.z / 2)));
        corners.Add(new Vector3(c.x + (s.x / 2), c.y + (s.y / 2), c.z + (s.z / 2)));
        corners.Add(new Vector3(c.x + (s.x / 2), c.y - (s.y / 2), c.z - (s.z / 2)));
        corners.Add(new Vector3(c.x + (s.x / 2), c.y - (s.y / 2), c.z + (s.z / 2)));
        return corners;
    }

    protected List<Vector3> CalculateMiddles()
    {
        List<Vector3> middles = new List<Vector3>();
        Vector3 c = targetBounds.center;
        Vector3 s = targetBounds.size;
        middles.Add(new Vector3(c.x - (s.x / 2), c.y + (s.y / 2), c.z));
        middles.Add(new Vector3(c.x, c.y + (s.y / 2), c.z - (s.z / 2)));
        middles.Add(new Vector3(c.x + (s.x / 2), c.y + (s.y / 2), c.z));
        middles.Add(new Vector3(c.x, c.y + (s.y / 2), c.z + (s.z / 2)));
        middles.Add(new Vector3(c.x - (s.x / 2), c.y, c.z - (s.z / 2)));
        middles.Add(new Vector3(c.x + (s.x / 2), c.y, c.z - (s.z / 2)));
        middles.Add(new Vector3(c.x + (s.x / 2), c.y, c.z + (s.z / 2)));
        middles.Add(new Vector3(c.x - (s.x / 2), c.y, c.z + (s.z / 2)));
        middles.Add(new Vector3(c.x - (s.x / 2), c.y - (s.y / 2), c.z));
        middles.Add(new Vector3(c.x, c.y - (s.y / 2), c.z - (s.z / 2)));
        middles.Add(new Vector3(c.x + (s.x / 2), c.y - (s.y / 2), c.z));
        middles.Add(new Vector3(c.x, c.y - (s.y / 2), c.z + (s.z / 2)));
        return middles;
    }

    public List<Vector3> CalculateCenters()
    {
        List<Vector3> centers = new List<Vector3>();
        Vector3 c = targetBounds.center;
        Vector3 s = targetBounds.size;
        centers.Add(new Vector3(c.x, c.y, c.z - (s.z / 2) - offset));
        centers.Add(new Vector3(c.x, c.y, c.z + (s.z / 2) + offset));
        centers.Add(new Vector3(c.x - (s.x / 2) - offset, c.y, c.z));
        centers.Add(new Vector3(c.x + (s.x / 2) + offset, c.y, c.z));
        centers.Add(new Vector3(c.x, c.y - (s.y / 2) - offset, c.z));
        centers.Add(new Vector3(c.x, c.y + (s.y / 2) + offset, c.z));
        return centers;
    }

    protected void InstantianteScaleHandles(List<Vector3> positions)
    {
        for (int i = 0; i < 8; i++)
        {
            scaleHandlesList.Add(Instantiate(handlePrefab, positions[i], Quaternion.identity, handlesContainer.transform));
            scaleHandlesList[i].GetComponent<ScaleHandleBehaviour>().Init(target, this);
            scaleHandlesList[i].transform.LookAt(transform);
            scaleHandlesList[i].name = "ScaleHandle_" + i;
        }
    }

    protected void InstantianteRotateHandles(List<Vector3> positions)
    {
        for (int i = 0; i < 12; i++)
        {
            rotateHandlesList.Add(Instantiate(rotationHandlePrefab, positions[i], rotationForRotateHandle[i], handlesContainer.transform));
            rotateHandlesList[i].GetComponent<RotationHandleBehaviour>().Init(target, this);
            SetTag(rotateHandlesList[i]);
            rotateHandlesList[i].name = "RotateHandle_" + i;
        }
    }

    public void InstantiateTranslateHandles(List<Vector3> pos)
    {
        for (int i = 0; i < 6; i++)
        {
            translateHandlesList.Add(Instantiate(translateHandlePrefab, pos[i], rotationForTranslateHandle[i], handlesContainer.transform));
            translateHandlesList[i].GetComponent<TranslateHandleBehaviour>().Init(target, this);
            SetTagTranslate(translateHandlesList[i]);
            translateHandlesList[i].name = "TranslateHandle_" + i;
            CustomEvent.Trigger(translateHandlesList[i], "OnPlatformedObject", gameObject);
        }
    }

    /// <summary>
    /// Method used if we use translation handles that have been already instantiated in the scene in fixed locations, and just need to be properly set
    /// </summary>
    public void InstantiateTranslateHandles()
    {
        foreach (Transform handle in translationHandleContainer.transform)
        {
            translateHandlesList.Add(handle.gameObject);
            handle.GetComponent<MeshRenderer>().enabled = true;
            handle.GetComponent<BoxCollider>().enabled = true;
            handle.GetComponent<TranslateHandleBehaviour>().Init(target, this);
            CustomEvent.Trigger(handle.gameObject, "OnPlatformedObject", gameObject);
        }
    }

    protected void UpdateScaleHandles(List<Vector3> positions)
    {
        for (int i = 0; i<8; i++)
        {
            scaleHandlesList[i].transform.position = positions[i];
        }
    }

    protected void UpdateRotateHandles(List<Vector3> positions)
    {
        for (int i = 0; i<12; i++)
        {
            rotateHandlesList[i].transform.position = positions[i];
        }
    }

    public virtual void InitBounds()
    {
        scaleHandlesList = new List<GameObject>();
        rotateHandlesList = new List<GameObject>();
        translateHandlesList = new List<GameObject>();
        handlesContainer = Instantiate(new GameObject(), transform);
        handlesContainer.name = "HandlesContainer";
        DetermineTargetBounds();
        InstantianteScaleHandles(CalculateCorners());
        InstantianteRotateHandles(CalculateMiddles());
        InstantiateTranslateHandles(CalculateCenters());
        //InstantiateTranslateHandles();
        handlesContainer.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
        //handlesContainer.transform.localEulerAngles = new Vector3(0, 45, 0);
        //Quaternion currentRotation = Target.transform.rotation;
        //Target.transform.rotation = Quaternion.identity;
        //UnityPhysics.SyncTransforms(); // Update collider bounds

        //Vector3 boundsExtents = TargetBounds.bounds.extents;

        //// After bounds are computed, restore rotation...
        //Target.transform.rotation = currentRotation;
        //UnityPhysics.SyncTransforms();
    }

    public virtual void DestroyBounds()
    {
        foreach(GameObject handle in scaleHandlesList)
        {
            Destroy(handle);
        }

        foreach (GameObject handle in rotateHandlesList)
        {
            Destroy(handle);
        }

        foreach (GameObject handle in translateHandlesList)
        {
            //handle.GetComponent<MeshRenderer>().enabled = false;
            //handle.GetComponent<BoxCollider>().enabled = false;
            Destroy(handle);
        }

        Destroy(handlesContainer);
        Destroy(collider);

        scaleHandlesList = null;
        rotateHandlesList = null;
        translateHandlesList = null;
        handlesContainer = null;
    }

    public void UpdateVisuals()
    {
        DetermineTargetBounds();
        UpdateScaleHandles(CalculateCorners());
        UpdateRotateHandles(CalculateMiddles());
    }

    protected void InitAnglesForRotateHandle()
    {
        rotationForRotateHandle.Add(Quaternion.Euler(90, 0, 0));
        rotationForRotateHandle.Add(Quaternion.Euler(0, 0, 90));
        rotationForRotateHandle.Add(Quaternion.Euler(90, 0, 0));
        rotationForRotateHandle.Add(Quaternion.Euler(0, 0, 90));
        rotationForRotateHandle.Add(Quaternion.Euler(0, 0, 0));
        rotationForRotateHandle.Add(Quaternion.Euler(0, 0, 0));
        rotationForRotateHandle.Add(Quaternion.Euler(0, 0, 0));
        rotationForRotateHandle.Add(Quaternion.Euler(0, 0, 0));
        rotationForRotateHandle.Add(Quaternion.Euler(90, 0, 0));
        rotationForRotateHandle.Add(Quaternion.Euler(0, 0, 90));
        rotationForRotateHandle.Add(Quaternion.Euler(90, 0, 0));
        rotationForRotateHandle.Add(Quaternion.Euler(0, 0, 90));
    }
    protected void InitAnglesForTranslateHandle()
    {
        rotationForTranslateHandle.Add(Quaternion.Euler(0, 180, 0));
        rotationForTranslateHandle.Add(Quaternion.Euler(0, 0, 0));
        rotationForTranslateHandle.Add(Quaternion.Euler(0, -90, 90));
        rotationForTranslateHandle.Add(Quaternion.Euler(0, 90, 90));
        rotationForTranslateHandle.Add(Quaternion.Euler(90, 0, 0));
        rotationForTranslateHandle.Add(Quaternion.Euler(-90, 0, 0));
    }


    protected void SetTag(GameObject handle)
    {
        if (handle.transform.rotation.eulerAngles.x == 90)
        {
            handle.tag = "Z";
        } else if (handle.transform.rotation.eulerAngles.z == 90)
        {
            handle.tag = "X";
        } else
        {
            handle.tag = "Y";
        }
    }

    protected void SetTagTranslate(GameObject handle)
    {
        if (handle.transform.up == Vector3.up || handle.transform.up == Vector3.down)
        {
            handle.tag = "Z";
        } else if (handle.transform.right == Vector3.up)
        {
            handle.tag = "X";
        } else
        {
            handle.tag = "Y";
        }
    }

    public void RotateBounds(Quaternion rot)
    {
        if (handlesContainer != null)
        {
            handlesContainer.transform.rotation = rot;
            translationHandleContainer.transform.rotation = rot;
        }
    }

    public void SetBoundsActive(bool val)
    {
        if (handlesContainer != null)
        {
            handlesContainer.SetActive(val);
        }
    }
}
