using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Experimental.UI;
using System.Linq;
using Bolt;
using Ludiq;
using Normal.Realtime;

/// <summary>
/// Class to set a new geometry in the scene in order to be used with the custom coordinate system. But differently from <see cref="GeometrySetModule"/>, this class:
/// <list type="number">
/// <item>create a new geometry which uses a mesh sequence in order to reproduce a Rhino animation</item>
/// <item>set it using the custom coordinate system as done by <see cref="GeometrySetModule"/></item>
/// </list>
/// </summary>
public class GeometryMeshSequenceSetModule : GeometrySetModule
{
    [Header("Multiple mesh (mesh sequence) geometries properties")]
    [Tooltip("List of folders name into StreamingAssets where all meshes for the same geometries can be found")]
    private List<string> geometriesName;

    [Tooltip("The mesh sequence objects already set in a mesh sequence gameobjects")]
    public List<GameObject> meshSequenceGeometriesPrefabs;

    [Space(15)]
    [SerializeField]
    [Tooltip("Custom prefab for a mesh sequence object, properly set with all necessary Components")]
    protected GameObject meshSequenceCentroidPefab;

    public Material importedMeshMaterial;
    public int meshSequenceDefaultIndex = 1;

    protected List<GameObject> meshSequenceGeometries = new List<GameObject>();
    protected List<GameObject> meshSequenceMeshes = new List<GameObject>();

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        //1 if the designer is connecting online to the room, so their scene must be properly set
        if ((int) Variables.Application.Get("SceneMode") == 1)
        {
            NormcoreManagerRealtime.didConnectToRoom += Realtime_didConnectToRoom;
            NormcoreManagerRealtime.didDisconnectFromRoom += Realtime_didDisconnectFromRoom;
        }
    }

    //Offline case
    private void Realtime_didDisconnectFromRoom(Realtime realtime)
    {
        if (NormcoreManagerRealtime.room.connectionState == Room.ConnectionState.Error)
        {
            if (!initDone)
            {
                LoadSingleMeshGeometries();

                LoadMeshSequenceGeometries();

                foreach (GameObject obj in meshSequenceGeometries)
                {
                    foreach (Transform meshObj in obj.transform.GetChild(0).GetChild(0))
                    {
                        meshObj.GetChild(0).gameObject.GetComponent<MeshRenderer>().material = importedMeshMaterial;
                    }
                    obj.GetComponent<MASelectable>().SaveOriginalMaterial(importedMeshMaterial);    //saving original material because after import the geometry is in the import dock and its material is different
                }

                initDone = true;
                dockObject.transform.GetComponentInChildren<SelectorDockPosition>().StartInitCoroutine();
            }
        }

    }

    //Online case
    private void Realtime_didConnectToRoom(Realtime realtime)
    {
        if (!initDone)
        {
            LoadSingleMeshGeometriesNormcore();

            LoadMeshSequenceGeometriesNormcore();

            foreach (GameObject obj in meshSequenceGeometries)
            {
                foreach (Transform meshObj in obj.transform.GetChild(0).GetChild(0))
                {
                    meshObj.GetChild(0).gameObject.GetComponent<MeshRenderer>().material = importedMeshMaterial;
                }
                obj.GetComponent<MASelectable>().SaveOriginalMaterial(importedMeshMaterial);    //saving original material because after import the geometry is in the import dock and its material is different
            }

            initDone = true;
            dockObject.transform.GetComponentInChildren<SelectorDockPosition>().StartInitCoroutine();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!initDone)
        {
            //0 if offline or 2 if guest
            if ((int)Variables.Application.Get("SceneMode") != 1)
            {
                LoadSingleMeshGeometries();

                LoadMeshSequenceGeometries();

                foreach (GameObject obj in meshSequenceGeometries)
                {
                    foreach (Transform meshObj in obj.transform.GetChild(0).GetChild(0))
                    {
                        meshObj.GetChild(0).gameObject.GetComponent<MeshRenderer>().material = importedMeshMaterial;
                    }
                    obj.GetComponent<MASelectable>().SaveOriginalMaterial(importedMeshMaterial);    //saving original material because after import the geometry is in the import dock and its material is different
                }

                initDone = true;
                dockObject.transform.GetComponentInChildren<SelectorDockPosition>().StartInitCoroutine();
            }

        }
    }

    /// <summary>
    /// Loads all meshes stored into Resources/foldername and built the mesh sequence gamobject structure
    /// </summary>
    /// <param name="folderName">Name of the folder in Resources containg all the single meshes</param>
    protected void LoadMeshSequence(string folderName)
    {
        GameObject[] meshes = Resources.LoadAll(folderName, typeof(GameObject)).Cast<GameObject>().ToArray();

        GameObject main = new GameObject("Dockable" + folderName);
        GameObject sequence = new GameObject(folderName + "sequence");
        sequence.transform.parent = main.transform;

        foreach (GameObject obj in meshes)
        {
            Instantiate(obj, Vector3.zero, Quaternion.identity, sequence.transform);
        }

        meshSequenceGeometries.Add(main);
    }

    /// <summary>
    /// Loads all mesh sequence geometries instantiated, and sets them into the dock, if any position is available
    /// </summary>
    protected void LoadMeshSequenceGeometries()
    {
        foreach (GameObject obj in meshSequenceGeometriesPrefabs)
        {
            GameObject main = new GameObject("Dockable" + obj.name);
            main.transform.localScale = Vector3.one;
            main.transform.position = Vector3.zero;
            GameObject geometry = Instantiate(obj, main.transform);
            geometry.transform.localScale = Vector3.one;

            if (SetNewGeometry(main, meshSequenceCentroidPefab) >= 1)
            {
                Debug.LogError($"Unable to correctly instantiate the new geometry {geometry.name}. Unknown error. Try again");
                ResetValues();
                return;
            }

            _geometryCentroid.GetComponent<MeshSequenceControllerImproved>().Setup();
            _geometryCentroid.GetComponent<MeshSequenceControllerImproved>().ActivateFrame(meshSequenceDefaultIndex);
            meshSequenceGeometries.Add(_geometryCentroid);
            //Destroy the mesh sequence gameobject created previously, or it'll be duplicated
            Destroy(main);

            //Default: goest first on dock, then on library
            if (_geometryCentroid.GetComponent<Dockable>().CanDock)
            {
                dockObject.transform.GetComponentInChildren<SelectorDockPosition>().AddGeometry(_geometryCentroid);
                _geometryCentroid.GetComponent<MASelectable>().Dock(dockObject.transform.GetComponentInChildren<SelectorDockPosition>());
                //dockObject.transform.GetComponentInChildren<SelectorDockPosition>().StartInitCoroutine();
            }
            ResetValues();
        }
    }


    /// <summary>
    /// Loads all mesh sequence geometries instantiated with Normcore Realtime component, and sets them into the dock, if any position is available
    /// </summary>
    protected void LoadMeshSequenceGeometriesNormcore()
    {
        foreach (GameObject obj in meshSequenceGeometriesPrefabs)
        {
            GameObject main = new GameObject("Dockable" + obj.name);
            main.transform.localScale = Vector3.one;
            main.transform.position = Vector3.zero;
            GameObject geometry = Instantiate(obj, main.transform);
            geometry.transform.localScale = Vector3.one;

            if (SetNewGeometry(main, meshSequenceCentroidPefab) == 1)
            {
               Debug.LogError($"Unable to correctly instantiate the new geometry {geometry.name}. Unknown error. Try again");
            }


            _geometryCentroid.GetComponent<MeshSequenceControllerImproved>().Setup();
            _geometryCentroid.GetComponent<MeshSequenceControllerImproved>().ActivateFrame(meshSequenceDefaultIndex);
            meshSequenceGeometries.Add(_geometryCentroid);
            //Destroy the mesh sequence gameobject created previously, or it'll be duplicated
            Destroy(main);

            //Default: goest first on dock, then on library
            if (_geometryCentroid.GetComponent<Dockable>().CanDock)
            {
                dockObject.transform.GetComponentInChildren<SelectorDockPosition>().AddGeometry(_geometryCentroid);
                _geometryCentroid.GetComponent<MASelectable>().Dock(dockObject.transform.GetComponentInChildren<SelectorDockPosition>());
                //dockObject.transform.GetComponentInChildren<SelectorDockPosition>().StartInitCoroutine();
            }
            ResetValues();
        }
    }

    /// <inheritdoc/>
    public override IEnumerator AdjustSize(GameObject centroid)
    {
        yield return new WaitForSeconds(1f);
        centroid.GetComponent<Dockable>().OnManipulationStarted();
        yield return new WaitForSeconds(1f);
        centroid.GetComponent<Dockable>().OnManipulationEnded();
        centroid.GetComponent<Dockable>().BlockScaleToFit = true;
    }
}
