using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Experimental.UI;
using Bolt;
using Normal.Realtime;

/// <summary>
/// The class is used to set any object to use the custom coordinates system for the MRTK dock and custom projection dock
/// </summary>
/// <remarks>The custom coordinate system is needed in order to let the user freely manipulate objects</remarks>
public class GeometrySetModule : MonoBehaviour
{
    #region Fields and serialized fields
    [Header("Single mesh geometries and general properties")]
    [Tooltip("The original geometry to properly be set")]
    public List<GameObject> geometries;

    [Space(15)]
    [SerializeField]
    protected GameObject centroidPrefab;

    protected Dock dockObject;

    protected Realtime NormcoreManagerRealtime;

    //Instance related variables
    protected GameObject instanceGeometry;
    protected BoxCollider instanceBoxCollider;
    protected Bounds instanceColliderBounds;

    //Centroid related variables
    protected GameObject _geometryCentroid;
    protected BoxCollider _centroidBoxCollider;
    protected savedTranform _savedCentroidTransformFromOrigin;
    protected savedTranform _savedOriginTransform;

    //Origin related variables
    protected GameObject _geometryOrigin;

    //Other variables
    protected bool initDone = false;
    #endregion

    #region Properties
    public GameObject GeometryCentroid
    {
        get => _geometryCentroid;
    }

    public GameObject GeometryOrigin
    {
        get => _geometryOrigin;
    }
    #endregion

    #region Structs
    //We need our custom struct similar to Transform class in order to save values and not to reference them
    /// <value>
    /// Custom struct which holds transform values in an equivalent struct
    /// </value>
    [System.Serializable]
    public struct savedTranform
    {
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;
    }

    #endregion

    #region Unity Engine methods
    // Start is called before the first frame update
    protected virtual void Start()
    {
        dockObject = GameObject.Find("GuardianCenter/Dock").GetComponent<Dock>();
        NormcoreManagerRealtime = GameObject.Find("NormcoreManager").GetComponent<Realtime>();
    }

    private void OnEnable()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (!initDone)
        {
            LoadSingleMeshGeometries();

            initDone = true;
        }
    }
    #endregion

    #region Custom methods for geometry set
    /// <summary>
    /// Creates a new instance of the geometry, properly set for the custom coordinates system
    /// </summary>
    /// <param name="geometry">The geometry to be set in the custom coordinates system</param>
    /// <param name="centroidPrefab">Prefab for the centroid object</param>
    /// <returns>1 if the system creation fails, 0 if successful</returns>
    public virtual int SetNewGeometry(GameObject geometry, GameObject centroidPrefab, bool useNormcore = false)
    {
        //1. New object instancing
        //instanceGeometry = Instantiate(originalGeometry, Vector3.zero, Quaternion.identity);
        instanceGeometry = Instantiate(geometry);

        if (instanceGeometry == null)
        {
            return 1;
        }

        //2. Basics setting of the new instance and box collider creation
        instanceGeometry.name = "Mesh";
        instanceBoxCollider = instanceGeometry.AddComponent<BoxCollider>();
        instanceColliderBounds = instanceBoxCollider.bounds;

        //2.1 Extra settings for mesh sequence objects
        //Finding which mesh has the largest box collider and using that size
        if (instanceGeometry.transform.childCount != 0)
        {
            //instanceColliderBounds.size = CheckMaxBoundsSizeInSequence(instanceGeometry);
            instanceColliderBounds.size = CheckMaxSizeForBoxCollider(instanceGeometry);
            //instanceColliderBounds.center = CheckMaxBoundsSizeCenterInSequence(instanceGeometry);
        }

        //3.Center and centroid configuration
        //Setting center and pivot gameobjects
        if (useNormcore)
        {
            if (NormcoreManagerRealtime.clientID == 0)
            {
                _geometryCentroid = Realtime.Instantiate(centroidPrefab.name);
                _geometryCentroid.GetComponent<RealtimeTransform>().RequestOwnership();
            }
            else
            {
                _geometryCentroid = Instantiate(centroidPrefab);
                //Destroy(instanceGeometry);
                //return 2;
            }
        }
        else
        {
            _geometryCentroid = Instantiate(centroidPrefab);
        }


        if (_geometryCentroid == null)
        {
            return 1;
        }

        _geometryCentroid.name = geometry.name + "_centroid";
        _geometryOrigin = new GameObject();

        if (_geometryOrigin == null)
        {
            return 1;
        }

        _geometryOrigin.name = geometry.name + "_center";

        //4.Creating centroid collider and copying the instance values into it
        _centroidBoxCollider = _geometryCentroid.AddComponent<BoxCollider>();
        _centroidBoxCollider.isTrigger = true;
        _centroidBoxCollider.size = instanceColliderBounds.size;
        //Putting the centroid position in the center of the geometry instance gameobject
        _geometryCentroid.transform.position = instanceColliderBounds.center;

        //4.1 Different setting for mesh sequence objects
        if (instanceGeometry.transform.childCount != 0)
        {
            _geometryCentroid.transform.position = CheckMaxBoundsSizeCenterInSequence(instanceGeometry);
        }

        Destroy(instanceBoxCollider);

        //5.Setting the center of the new system (center which has been configured in the external software and may be external the actual mesh)
        _geometryOrigin.transform.position = instanceGeometry.transform.position;

        //6.Configuring the correct hierarchy of GameObjects
        instanceGeometry.transform.parent = _geometryCentroid.transform;
        _geometryCentroid.transform.parent = _geometryOrigin.transform;

        /*Hierarchy now should be:
         *  ― _geometryOrigin
         *   └ _geometryCentroid
         *    └ instanceGeometry
         */

        //7.Saving necessary data
        SaveLocalTransformToSavedTransformData(_geometryCentroid.transform, out _savedCentroidTransformFromOrigin);
        SaveLocalTransformToSavedTransformData(_geometryOrigin.transform, out _savedOriginTransform);
        //Unparenting the centroid from the origin, values have been saved
        _geometryCentroid.transform.parent = null;

        //8. Changing original scale for the entire object
        _geometryCentroid.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);

        //8.1. For some geometries, expecially the bigger ones, this scale may be still too big, so we should check the lossy scale
        float maxGeometryLossyScale = SizeGreaterThan(_geometryCentroid.transform.lossyScale.x, _geometryCentroid.transform.lossyScale.y, _geometryCentroid.transform.lossyScale.z);
        float maxBoundsSize = SizeGreaterThan(_centroidBoxCollider.size.x, _centroidBoxCollider.size.y, _centroidBoxCollider.size.z);

        while (maxGeometryLossyScale * maxBoundsSize > 0.2f)
        {
            _geometryCentroid.transform.localScale -= new Vector3(0.001f, 0.001f, 0.001f);
            maxGeometryLossyScale = SizeGreaterThan(_geometryCentroid.transform.lossyScale.x, _geometryCentroid.transform.lossyScale.y, _geometryCentroid.transform.lossyScale.z);
        }

        //9.Setting Bolt variables and docking the centroid
        Variables.Object(_geometryCentroid).Set("ObjectParent", GameObject.Find("ObjectParent"));

        //Saving transform values to the platformable script in order to be used when docked in the projection dock
        _geometryCentroid.GetComponent<Platformable>().SavedCentroidTransformWhenPivoted = _savedCentroidTransformFromOrigin;
        _geometryCentroid.GetComponent<Platformable>().SavedOriginTransform = _savedOriginTransform;

        //SETTINGS DONE!
        return 0;
    }

    /// <summary>
    /// Copies the origin transform data into the custom struct for transform data.
    /// </summary>
    /// <remarks>This is done because a copy of Tranform data is need instead of changing the original values</remarks>
    /// <param name="origin">The Tranform data to copy</param>
    /// <param name="destination">The copied Tranform data into the custom struct <see cref="GeometrySetModule.savedTranform"/></param>
    static public void SaveLocalTransformToSavedTransformData(Transform origin, out savedTranform destination)
    {
        destination.position = origin.localPosition;
        destination.rotation = origin.localRotation;
        destination.scale = origin.localScale;
    }

    /// <summary>
    /// Copies the custom struct for transform data into the custom struct for transform data.
    /// </summary>
    /// <param name="origin">The Tranform data to copy</param>
    /// <param name="destination">The copied Tranform data into the custom struct <see cref="GeometrySetModule.savedTranform"/></param>
    static public void SavedTransformDataToSavedTransformData(savedTranform origin, out savedTranform destination)
    {
        destination.position = origin.position;
        destination.rotation = origin.rotation;
        destination.scale = origin.scale;
    }

    /// <summary>
    /// Reset values used in the creation of the custom system
    /// </summary>
    public void ResetValues()
    {
        instanceGeometry = null;
        instanceBoxCollider = null;
        _geometryCentroid = null;
        _centroidBoxCollider = null;
        _geometryOrigin = null;
    }

    #endregion

    #region Coroutines
    /// <summary>
    /// Coroutines to adjust the size of docked geometry. If docking occurs too quick, size won't adjust properly, so calling this method will adjust it to properly fit the dock
    /// </summary>
    /// <param name="centroid">The centroid of a geometry which has been processed in the custom coordinates system</param>
    public virtual IEnumerator AdjustSize(GameObject centroid)
    {
        yield return new WaitForSeconds(1f);
        centroid.GetComponent<Dockable>().OnManipulationStarted();
        yield return new WaitForSeconds(1f);
        centroid.GetComponent<Dockable>().OnManipulationEnded();
        centroid.GetComponent<Dockable>().BlockScaleToFit = true;
    }

    /// <summary>
    /// Loads all single mesh geometries instantiated, and sets them into the dock, if any position is available
    /// </summary>
    protected void LoadSingleMeshGeometries()
    {
        foreach (GameObject geometry in geometries)
        {
            if (SetNewGeometry(geometry, centroidPrefab) == 1)
            {
                Debug.LogError($"Unable to correctly instantiate the new geometry {geometry.name}. Unknown error. Try again");
            }

            foreach (DockPosition dp in dockObject.DockPositions)
            {
                if (!dp.IsOccupied)
                {
                    _geometryCentroid.GetComponent<Dockable>().Dock(dp);
                    _geometryCentroid.GetComponent<Dockable>().BlockScaleToFit = true;
                    //StartCoroutine(AdjustSize(_geometryCentroid));
                    break;
                }
            }

            ResetValues();
        }
    }

    /// <summary>
    /// Loads all single mesh geometries instantiated with Norcmore Realtime components, and sets them into the dock, if any position is available
    /// </summary>
    protected void LoadSingleMeshGeometriesNormcore()
    {
        foreach (GameObject geometry in geometries)
        {
            //if (SetNewGeometry(geometry, centroidNormcorePrefab, true) == 1)
            //{
            //    Debug.LogError($"Unable to correctly instantiate the new geometry {geometry.name}. Unknown error. Try again");
            //}

            foreach (DockPosition dp in dockObject.DockPositions)
            {
                if (!dp.IsOccupied)
                {
                    _geometryCentroid.GetComponent<Dockable>().Dock(dp);
                    _geometryCentroid.GetComponent<Dockable>().BlockScaleToFit = true;
                    //StartCoroutine(AdjustSize(_geometryCentroid));
                    break;
                }
            }

            ResetValues();
        }
    }
    #endregion

    #region Extra utils methods for child classes
    /// <summary>
    /// Checks which mesh in the mesh sequence has the largest bounding box, and returns that bounding box size
    /// </summary>
    /// <param name="meshSequenceMainObject">Parent object containing the entire mesh sequence struct</param>
    /// <returns>The size of the largest bounding box in the mesh sequence</returns>
    protected Vector3 CheckMaxBoundsSizeInSequence(GameObject meshSequenceMainObject)
    {
        Vector3 maxBoundsSize = Vector3.zero;
        var meshSequence = meshSequenceMainObject.transform.GetChild(0).transform;

        foreach (Transform meshSequenceInstance in meshSequence)
        {
            Bounds tempBounds = meshSequenceInstance.GetChild(0).gameObject.GetComponent<MeshRenderer>().bounds;
            if (tempBounds.size.x > maxBoundsSize.x)
            {
                maxBoundsSize.x = tempBounds.size.x;
            }
            if (tempBounds.size.y > maxBoundsSize.y)
            {
                maxBoundsSize.y = tempBounds.size.y;
            }
            if (tempBounds.size.z > maxBoundsSize.z)
            {
                maxBoundsSize.z = tempBounds.size.z;
            }
            //Vector3 tempBoundsSize = meshSequenceInstance.GetChild(0).gameObject.GetComponent<MeshRenderer>().bounds.size;
            //maxBoundsSize = VectorGreaterThan(tempBoundsSize, maxBoundsSize) ? tempBoundsSize : maxBoundsSize;
        }

        return maxBoundsSize;
    }

    /// <summary>
    /// Checks which mesh in the mesh sequence has the largest bounding box, check which dimension is the biggest, and build a cubic bounding box of that size
    /// </summary>
    /// <param name="meshSequenceMainObject">Parent object containing the entire mesh sequence struct</param>
    /// <returns>The size of the largest cubic bounding box of the largest dimension in the mesh sequence</returns>
    protected Vector3 CheckMaxSizeForBoxCollider(GameObject meshSequenceMainObject)
    {
        float maxSize = 0;
        Vector3 maxBoundingBox = CheckMaxBoundsSizeInSequence(meshSequenceMainObject);
        if (maxBoundingBox.x >= maxBoundingBox.y)
        {
            maxSize = maxBoundingBox.x;
        }
        else
        {
            maxSize = maxBoundingBox.y;
        }
        if (maxBoundingBox.z >= maxSize)
        {
            maxSize = maxBoundingBox.z;
        }

        return new Vector3(maxSize, maxSize, maxSize);
    }

    /// <summary>
    /// Checks which mesh in the mesh sequence has the largest bounding box, and returns that bounding box center
    /// </summary>
    /// <param name="meshSequenceMainObject">Parent object containing the entire mesh sequence struct</param>
    /// <returns>The center local coordinates of the largest bounding box in the mesh sequence</returns>
    protected Vector3 CheckMaxBoundsSizeCenterInSequence(GameObject meshSequenceMainObject)
    {
        Vector3 maxBoundsSize = Vector3.zero;
        Vector3 center = Vector3.zero;
        var meshSequence = meshSequenceMainObject.transform.GetChild(0).transform;

        foreach (Transform meshSequenceInstance in meshSequence)
        {
            Bounds tempBounds = meshSequenceInstance.GetChild(0).gameObject.GetComponent<MeshRenderer>().bounds;
            if (tempBounds.size.x > maxBoundsSize.x)
            {
                maxBoundsSize.x = tempBounds.size.x;
                center.x = tempBounds.center.x + meshSequenceInstance.GetChild(0).position.x;
            }
            if (tempBounds.size.y > maxBoundsSize.y)
            {
                maxBoundsSize.y = tempBounds.size.y;
                center.y = tempBounds.center.y + meshSequenceInstance.GetChild(0).position.y;
            }
            if (tempBounds.size.z > maxBoundsSize.z)
            {
                maxBoundsSize.z = tempBounds.size.z;
                center.z = tempBounds.center.z + meshSequenceInstance.GetChild(0).position.z;
            }
            //if (VectorGreaterThan(tempBounds.size, maxBoundsSize))
            //{
            //    maxBoundsSize = tempBounds.size;
            //    center = tempBounds.center + meshSequenceInstance.GetChild(0).position;
            //    Debug.Log("center is " + center);
            //}
        }

        return center;
    }

    /// <summary>
    /// Recenter the centroid position in case it's not at the object center. Both centroid and parented object's position is changed.
    /// </summary>
    protected void RecenterCentroid(GameObject centroid, GameObject meshSequenceMainObject)
    {
        Vector3 center = CheckMaxBoundsSizeCenterInSequence(meshSequenceMainObject);
        Vector3 diff = center - centroid.transform.position;
        centroid.transform.localPosition += diff;
        meshSequenceMainObject.transform.localPosition -= diff;
    }

    /// <summary>
    /// Checks if a vector is greater than another component by component
    /// </summary>
    /// <returns>True if first vector is greater component by component than the second, false if otherwise or equal</returns>
    protected static bool VectorGreaterThan(Vector3 greater, Vector3 lower)
    {
        return greater.x > lower.x && greater.y > lower.y && greater.z > lower.z;
    }

    public static float SizeGreaterThan(float x, float y, float z)
    {
        float xy = x > y ? x : y;
        return xy > z ? xy : z;
    }

    public static float SizeLowerThan(float x, float y, float z)
    {
        float xy = x < y ? x : y;
        return xy < z ? xy : z;
    }
    #endregion
}
