using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Bolt;
using Ludiq;

/// <summary>
/// Class used for the selection dock to visualized saved geometries that can't be placed on the dock (due to a lack of space). The class handles the visualization of geometries and their management
/// </summary>
public class SelectorDockPosition : MonoBehaviour
{
    [Tooltip("Material when the object is visualized in the import dock")] public Material materialWhenViewed;
    [Tooltip("List of current geometries held by the selection dock")] public List<GameObject> savedGeometries = new List<GameObject>();
    public ParticleSystem creationParticleFX;

    public GameObject currentViewedGeometry;

    private Coroutine initCoroutine = null;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// This method shows the saved geometry at position <c>index</c>
    /// </summary>
    /// <param name="index">The index in the geoemtries list of the geometry to show in the selector</param>
    public void VisualizeGeometryByIndex(int index)
    {
        if (currentViewedGeometry == null)
        {
            currentViewedGeometry = savedGeometries[index];
            currentViewedGeometry.SetActive(true);
            currentViewedGeometry.GetComponent<MeshSequenceControllerImproved>().ChangeMaterialToMeshes(materialWhenViewed);
            currentViewedGeometry.GetComponent<MASelectable>().currentState = MASelectable.SelectionState.SELECTED;
        } else {
            currentViewedGeometry.SetActive(false);
            currentViewedGeometry.GetComponent<MASelectable>().currentState = MASelectable.SelectionState.UNSELECTED;
            currentViewedGeometry = savedGeometries[index];
            currentViewedGeometry.SetActive(true);
            currentViewedGeometry.GetComponent<MeshSequenceControllerImproved>().ChangeMaterialToMeshes(materialWhenViewed);
            currentViewedGeometry.GetComponent<MASelectable>().currentState = MASelectable.SelectionState.SELECTED;
        }
    }

    /// <summary>
    /// This method shows the saved geometry whose name is <c>name</c>
    /// </summary>
    /// <param name="name">The name in the geoemtries list of the geometry to show in the selector</param>
    public void VisualizeGeometryByName(string name)
    {
        for (int i = 0; i < savedGeometries.Count; i++)
        {
            if (savedGeometries[i].name == name)
            {
                if (currentViewedGeometry == null)
                {
                    currentViewedGeometry = savedGeometries[i];
                    currentViewedGeometry.SetActive(true);
                    currentViewedGeometry.GetComponent<MeshSequenceControllerImproved>().ChangeMaterialToMeshes(materialWhenViewed);
                    currentViewedGeometry.GetComponent<MASelectable>().currentState = MASelectable.SelectionState.SELECTED;
                }
                else
                {
                    currentViewedGeometry.SetActive(false);
                    currentViewedGeometry.GetComponent<MASelectable>().currentState = MASelectable.SelectionState.UNSELECTED;
                    currentViewedGeometry = savedGeometries[i];
                    currentViewedGeometry.SetActive(true);
                    currentViewedGeometry.GetComponent<MeshSequenceControllerImproved>().ChangeMaterialToMeshes(materialWhenViewed);
                    currentViewedGeometry.GetComponent<MASelectable>().currentState = MASelectable.SelectionState.SELECTED;
                }
            }
        }

        Debug.LogError($"No geometry with name {name} in the collection");
    }

    /// <summary>
    /// This method shows the saved geometry by reference
    /// </summary>
    /// <param name="obj">The object reference in the geoemtries list of the geometry to show in the selector</param>
    public void VisualizeGeometryByObject(GameObject obj)
    {
        int i = savedGeometries.IndexOf(obj);
        if (i != -1)
        {
            if (currentViewedGeometry == null)
            {
                currentViewedGeometry = savedGeometries[i];
                currentViewedGeometry.SetActive(true);
                currentViewedGeometry.GetComponent<MeshSequenceControllerImproved>().ChangeMaterialToMeshes(materialWhenViewed);
                currentViewedGeometry.GetComponent<MASelectable>().currentState = MASelectable.SelectionState.SELECTED;
            }
            else
            {
                currentViewedGeometry.SetActive(false);
                currentViewedGeometry = savedGeometries[i];
                currentViewedGeometry.SetActive(true);
                currentViewedGeometry.GetComponent<MeshSequenceControllerImproved>().ChangeMaterialToMeshes(materialWhenViewed);
                currentViewedGeometry.GetComponent<MASelectable>().currentState = MASelectable.SelectionState.SELECTED;
            }
        } else
        {
            Debug.LogError($"No geometry with name {obj.name} in the collection");
            return;
        }
    }

    #region Public methods (for interaction with the class)
    /// <summary>
    /// This method add a new geometry to the list of the geometries that this Selector holds
    /// </summary>
    /// <param name="geometry">The geometry to add</param>
    public void AddGeometry(GameObject geometry)
    {
        savedGeometries.Add(geometry);
        //geometry.SetActive(false);
        CustomEvent.Trigger(transform.GetChild(0).GetChild(1).gameObject, "UpdateGeometryList");
    }

    /// <summary>
    /// This method removes a geometry from the list of the geometries that this Selector holds
    /// </summary>
    /// <param name="geometry">The geometry to remove</param>
    public void RemoveGeometry(GameObject geometry)
    {
        savedGeometries.Remove(geometry);
        if (currentViewedGeometry == geometry)
        {
            currentViewedGeometry = null;
        }
        CustomEvent.Trigger(transform.GetChild(0).GetChild(1).gameObject, "UpdateGeometryList");
    }

    public void StartInitCoroutine()
    {
        if (initCoroutine == null)
        {
            initCoroutine = StartCoroutine(InitializeDockPosition());
        }
    }
    #endregion

    #region Coroutines
    /// <summary>
    /// The coroutine properly initialize the selector dock by visualizing the first geometry in the list
    /// </summary>
    /// <returns></returns>
    private IEnumerator InitializeDockPosition()
    {
        yield return new WaitForSeconds(1.5f);
        VisualizeGeometryByIndex(0);

        initCoroutine = null;
    }
    #endregion
}
