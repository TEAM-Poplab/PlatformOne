using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Normal.Realtime;

[RequireComponent(typeof(MeshSequenceControllerImproved))]
public class RealtimeMeshSequenceController : RealtimeComponent<RealtimeMeshSequenceControllerModel>
{
    private Realtime core = null;
    private GameObject coreManager = null;

    private bool guestInstance = false;

    private void Awake()
    {
        coreManager = GameObject.Find("NormcoreManager");
        core = coreManager.GetComponent<Realtime>();
    }

    private void Start()
    {
        if (core != null && core.room.connected && core.clientID == 1)
        {
            GameObject[] obj = GameObject.FindGameObjectsWithTag("Centroid");

            foreach (GameObject ob in obj)
            {
                if (ob.name == model.geometryName)
                {
                    ob.transform.GetChild(0).parent = transform;
                    gameObject.name = model.geometryName;
                    Destroy(ob);
                }
            }
        }
    }

    private void Update()
    {
        if (!guestInstance)
        {
            if (coreManager == null)
            {
                coreManager = GameObject.Find("NormcoreManager");
                core = coreManager.GetComponent<Realtime>();
            }

            if (core.room.connected && core.clientID == 1)
            {
                GameObject[] obj = GameObject.FindGameObjectsWithTag("Centroid");

                foreach (GameObject ob in obj)
                {
                    if (ob.name == model.geometryName)
                    {
                        ob.transform.GetChild(0).parent = transform;
                        gameObject.name = model.geometryName;
                        Destroy(ob);
                    }
                }

                guestInstance = true;
            }
        }
    }

    protected override void OnRealtimeModelReplaced(RealtimeMeshSequenceControllerModel previousModel, RealtimeMeshSequenceControllerModel currentModel)
    {
        if (previousModel != null)
        {
            // Unregister from events
            previousModel.indexDidChange -= CurrentModel_indexDidChange;
            previousModel.materialNameDidChange -= CurrentModel_materialNameDidChange;
        }

        if (currentModel != null)
        {
            // If this is a model that has no data set on it, populate it with the current mesh renderer color.
            if (currentModel.isFreshModel)
            {
                currentModel.index = 0;
            }

            // Register for events so we'll know if the color changes later
            currentModel.indexDidChange += CurrentModel_indexDidChange;
            currentModel.materialNameDidChange += CurrentModel_materialNameDidChange;
        }
    }

    private void CurrentModel_materialNameDidChange(RealtimeMeshSequenceControllerModel model, string value)
    {
        if (core.clientID == 1)
        {
            //GetComponent<MeshSequenceControllerImproved>().ChangeMaterialToMeshes();
        }
    }

    private void CurrentModel_indexDidChange(RealtimeMeshSequenceControllerModel model, int value)
    {
        if (core.clientID == 1)
        {
            GetComponent<MeshSequenceControllerImproved>().ActivateFrame(value);
        }
    }

    public void SetIndex(int value)
    {
        if (core.clientID == 0)
        {
            model.index = value;
        }
    }

    public void SetName(string value)
    {
        model.geometryName = value;
    }
}
