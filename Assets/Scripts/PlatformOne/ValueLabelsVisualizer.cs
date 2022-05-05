using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;
using Bolt;
using Ludiq;

/// <summary>
/// Visualizer for labels when the platformed object is manipulated. The labels shows transform informations about the manipulated object's transform changes
/// </summary>
public class ValueLabelsVisualizer : MonoBehaviour
{
    [Tooltip("The label prefab")] public GameObject labelPrefab;
    public bool isMeshSequence = true;

    [HideInInspector] public Transform miniSceneLabelPlaceholder;
    [HideInInspector] public Transform miniScene;

    [HideInInspector] public Transform projectedObjectPlaceholder;
    [HideInInspector] public Transform projectedObject;

    [HideInInspector] public GameObject labelsContainer;
    [HideInInspector] public List<GameObject> rotateLabelsList;
    [HideInInspector] public List<GameObject> translateLabelsList;
    [HideInInspector] public GameObject scaleLabel;
    [HideInInspector] public GameObject miniSceneScaleLabel;
    [HideInInspector] public Transform playerCamera;

    [HideInInspector] public TransformStruct originalObjectTransform;
    [HideInInspector] public Vector3 originalSceneScale;

    //List of all handles
    [HideInInspector] public List<GameObject> xRotationHandles;
    [HideInInspector] public List<GameObject> yRotationHandles;
    [HideInInspector] public List<GameObject> zRotationHandles;
    [HideInInspector] public List<GameObject> xTranslationHandles;
    [HideInInspector] public List<GameObject> yTranslationHandles;
    [HideInInspector] public List<GameObject> zTranslationHandles;
    [HideInInspector] public List<GameObject> scaleHandles;

    [HideInInspector] public bool allSet = false;
    [HideInInspector] public List<GameObject> currHandles = new List<GameObject>();
    [HideInInspector] public List<GameObject> prevHandles = new List<GameObject>();

    [HideInInspector] public float xDisplacement = 0, yDisplacement = 0, zDisplacement = 0;

    /// <summary>
    /// Used to memorize the x-axes displacement of projected object. It is used by the translation handles to save the value
    /// </summary>
    public float Xdisplacement
    {
        private get => xDisplacement;
        set => xDisplacement += value;
    }

    /// <summary>
    /// Used to memorize the y-axes displacement of projected object. It is used by the translation handles to save the value
    /// </summary>
    public float Ydisplacement
    {
        private get => yDisplacement;
        set => yDisplacement += value;
    }

    /// <summary>
    /// Used to memorize the z-axes displacement of projected object. It is used by the translation handles to save the value
    /// </summary>
    public float Zdisplacement
    {
        private get => zDisplacement;
        set => zDisplacement += value;
    }

    /// <summary>
    /// Custom struct to save Transform values by value
    /// </summary>
    public struct TransformStruct
    {
        public TransformStruct(Vector3 p, Quaternion r, Vector3 s)
        {
            pos = p;
            locPos = Vector3.zero;
            rot = r;
            scale = s;
        }

        public TransformStruct(Vector3 p, Vector3 lp, Quaternion r, Vector3 s)
        {
            pos = p;
            locPos = lp;
            rot = r;
            scale = s;
        }

        public Vector3 pos;
        public Vector3 locPos;
        public Quaternion rot;
        public Vector3 scale;
    }

    private void Start()
    {
        miniScene = GameObject.Find("GuardianCenter/Dock/PlatformDockPosition/MiniScene").transform;
        miniSceneLabelPlaceholder = GameObject.Find("GuardianCenter/Dock/PlatformDockPosition/MiniSceneScale placeholder").transform;
        if (isMeshSequence)
        {
            projectedObjectPlaceholder = GameObject.Find("GuardianCenter/ObjectPlaceholderMS").transform;
        } else
        {
            projectedObjectPlaceholder = GameObject.Find("GuardianCenter/ObjectPlaceholder").transform;
        }
    }


    // Update is called once per frame
    void Update()
    {
        if (allSet)
        {
            UpdateAllLabels(projectedObject.position, projectedObject.localPosition, transform.rotation, transform.parent.localScale, miniScene.lossyScale);
        }
    }
    private void FixedUpdate()
    {
        if (allSet)
        {
            currHandles = UpdateNearestHandles();
            for (int i = 0; i < currHandles.Count; i++)
            {
                if (currHandles[i].name != prevHandles[i].name)
                {
                    UpdateLabelsPosition(currHandles);
                    prevHandles = currHandles;
                    break;
                }
            }
        }
    }

    /// <summary>
    /// Creates labels from the label prefab, near the manipulation handles
    /// </summary>
    /// <remarks>Only one handle per axis is chosen, and only the closest to the player</remarks>
    public void CreateLabels()
    {
        List<GameObject> handles = FindNearestHandles();

        labelsContainer = new GameObject("LabelsContainer");
        labelsContainer.transform.parent = transform;

        rotateLabelsList = new List<GameObject>();
        translateLabelsList = new List<GameObject>();

        for (int i = 0; i < 3; i++)
        {
            rotateLabelsList.Add(Instantiate(labelPrefab, new Vector3(handles[i].transform.position.x, handles[i].transform.position.y, handles[i].transform.position.z),
                Quaternion.identity, labelsContainer.transform));
        }

        for (int i = 3; i < 6; i++)
        {
            translateLabelsList.Add(Instantiate(labelPrefab, new Vector3(handles[i].transform.position.x, handles[i].transform.position.y, handles[i].transform.position.z),
                Quaternion.identity, labelsContainer.transform));
        }

        for (int i = 0; i < 6; i++)
        {
            if (i < 3)
            {
                rotateLabelsList[i].GetComponent<SolverHandler>().TransformOverride = handles[i].transform;
            }
            else
            {
                translateLabelsList[i - 3].GetComponent<SolverHandler>().TransformOverride = handles[i].transform;
            }
        }

        //for (int i = 0; i < 3; i++)
        //{
        //    rotateLabelsList[i].GetComponent<SolverHandler>().TransformOverride = handles[i].transform;
        //}

        scaleLabel = Instantiate(labelPrefab, new Vector3(handles[6].transform.position.x, handles[6].transform.position.y, handles[6].transform.position.z),
        Quaternion.identity, labelsContainer.transform);

        scaleLabel.GetComponent<SolverHandler>().TransformOverride = handles[6].transform;

        miniSceneScaleLabel = Instantiate(labelPrefab, miniSceneLabelPlaceholder);
        miniSceneScaleLabel.GetComponent<SolverHandler>().TransformOverride = miniSceneLabelPlaceholder;

        projectedObject = projectedObjectPlaceholder.GetChild(0);
        SaveOriginalTransformValues();
        UpdateAllLabels(originalObjectTransform.pos, originalObjectTransform.locPos, originalObjectTransform.rot, originalObjectTransform.scale, originalSceneScale);

        currHandles = handles;
        prevHandles = handles;

        allSet = true;

        SetLabelsVisibility(UIManagerForUserMenuMRTKWithoutButtons.Instance.AreLabelsActive);
    }

    /// <summary>
    /// Creates labels from the label prefab, near the manipulation handles
    /// </summary>
    /// <remarks>Only one handle per axis is chosen, and only the closest to the player</remarks>
    public void CreateLabelsAfterFreeze()
    {
        List<GameObject> handles = FindNearestHandles();

        labelsContainer = new GameObject("LabelsContainer");
        labelsContainer.transform.parent = transform;

        rotateLabelsList = new List<GameObject>();
        translateLabelsList = new List<GameObject>();

        for (int i = 0; i < 3; i++)
        {
            rotateLabelsList.Add(Instantiate(labelPrefab, new Vector3(handles[i].transform.position.x, handles[i].transform.position.y, handles[i].transform.position.z),
                Quaternion.identity, labelsContainer.transform));
        }

        for (int i = 3; i < 6; i++)
        {
            translateLabelsList.Add(Instantiate(labelPrefab, new Vector3(handles[i].transform.position.x, handles[i].transform.position.y, handles[i].transform.position.z),
                Quaternion.identity, labelsContainer.transform));
        }

        for (int i = 0; i < 6; i++)
        {
            if (i < 3)
            {
                rotateLabelsList[i].GetComponent<SolverHandler>().TransformOverride = handles[i].transform;
            }
            else
            {
                translateLabelsList[i - 3].GetComponent<SolverHandler>().TransformOverride = handles[i].transform;
            }
        }

        //for (int i = 0; i < 3; i++)
        //{
        //    rotateLabelsList[i].GetComponent<SolverHandler>().TransformOverride = handles[i].transform;
        //}

        scaleLabel = Instantiate(labelPrefab, new Vector3(handles[6].transform.position.x, handles[6].transform.position.y, handles[6].transform.position.z),
        Quaternion.identity, labelsContainer.transform);

        scaleLabel.GetComponent<SolverHandler>().TransformOverride = handles[6].transform;

        miniSceneScaleLabel = Instantiate(labelPrefab, miniSceneLabelPlaceholder);
        miniSceneScaleLabel.GetComponent<SolverHandler>().TransformOverride = miniSceneLabelPlaceholder;

        projectedObject = projectedObjectPlaceholder.GetChild(0);
        UpdateAllLabels(originalObjectTransform.pos, originalObjectTransform.locPos, originalObjectTransform.rot, originalObjectTransform.scale, originalSceneScale);

        currHandles = handles;
        prevHandles = handles;

        allSet = true;

        SetLabelsVisibility(UIManagerForUserMenuMRTKWithoutButtons.Instance.AreLabelsActive);
    }

    public void DestroyLabels()
    {
        allSet = false;
        Destroy(labelsContainer);
        Destroy(miniSceneScaleLabel);
        labelsContainer = null;
        miniSceneScaleLabel = null;
        projectedObject = null;
        //originalSceneScale = Vector3.zero;

        xDisplacement = 0;
        yDisplacement = 0;
        zDisplacement = 0;
    }

    /// <summary>
    /// Used to update the value for each label, using new ones as input and determing the ratio with original ones
    /// </summary>
    /// <param name="newPos">New object world position</param>
    /// <param name="newLocPos">New object local position</param>
    /// <param name="newRot">New object rotation</param>
    /// <param name="newScale">New object scale</param>
    /// <param name="newSceneScale">New mini scene scale</param>
    public void UpdateAllLabels(Vector3 newPos, Vector3 newLocPos, Quaternion newRot, Vector3 newScale, Vector3 newSceneScale)
    {
        translateLabelsList[0].GetComponent<TMP_Text>().text = $"{Xdisplacement.ToString("00.00")} \n <size=70%> {(newPos.x - originalObjectTransform.pos.x).ToString("00.00")}";
        translateLabelsList[1].GetComponent<TMP_Text>().text = $"{Ydisplacement.ToString("00.00")} \n <size=70%> {(newPos.y - originalObjectTransform.pos.y).ToString("00.00")}";
        translateLabelsList[2].GetComponent<TMP_Text>().text = $"{Zdisplacement.ToString("00.00")} \n <size=70%> {(newPos.z - originalObjectTransform.pos.z).ToString("00.00")}";

        //translateLabelsList[1].GetComponent<TMP_Text>().text = $"{(newLocPos.y - originalObjectTransform.locPos.y).ToString("00.00")} \n <size=70%> {(newPos.y - originalObjectTransform.pos.y).ToString("00.00")}";
        //translateLabelsList[2].GetComponent<TMP_Text>().text = $"{(newLocPos.z - originalObjectTransform.locPos.z).ToString("00.00")} \n <size=70%> {(newPos.z - originalObjectTransform.pos.z).ToString("00.00")}";

        var x = (newRot.eulerAngles.x - originalObjectTransform.rot.eulerAngles.x);
        var y = (newRot.eulerAngles.y - originalObjectTransform.rot.eulerAngles.y);
        var z = (newRot.eulerAngles.z - originalObjectTransform.rot.eulerAngles.z);

        if (x > 180)
            x -= 360;
        if (y > 180)
            y -= 360;
        if (z > 180)
            z -= 360;

        rotateLabelsList[0].GetComponent<TMP_Text>().text = x.ToString("00.00°");
        rotateLabelsList[1].GetComponent<TMP_Text>().text = y.ToString("00.00°");
        rotateLabelsList[2].GetComponent<TMP_Text>().text = z.ToString("00.00°");

        //rotateLabelsList[0].GetComponent<TMP_Text>().text = ((newRot.eulerAngles.x - originalObjectTransform.rot.eulerAngles.x) * Mathf.Sign(Quaternion.Dot(originalObjectTransform.rot, newRot)) % 180).ToString("00.00°");
        //rotateLabelsList[1].GetComponent<TMP_Text>().text = ((newRot.eulerAngles.y - originalObjectTransform.rot.eulerAngles.y) * Mathf.Sign(Quaternion.Dot(originalObjectTransform.rot, newRot)) % 180).ToString("00.00°");
        //rotateLabelsList[2].GetComponent<TMP_Text>().text = ((newRot.eulerAngles.z - originalObjectTransform.rot.eulerAngles.z) * Mathf.Sign(Quaternion.Dot(originalObjectTransform.rot, newRot)) % 180).ToString("00.00°");

        scaleLabel.GetComponent<TMP_Text>().text = (newScale.x / originalObjectTransform.scale.x).ToString("0.0");
        miniSceneScaleLabel.GetComponent<TMP_Text>().text = (newSceneScale.x / originalSceneScale.x).ToString("0.0");
    }

    /// <summary>
    /// Used to update the value for each label, using new ones as input and determing the ratio with original ones
    /// </summary>
    /// <param name="newPos">New object position</param>
    /// <param name="newRot">New object rotation</param>
    /// <param name="newScale">New object scale</param>
    /// <param name="newSceneScale">New mini scene scale</param>
    public void UpdateAllLabels(Vector3 newPos, Quaternion newRot, Vector3 newScale, Vector3 newSceneScale)
    {
        translateLabelsList[0].GetComponent<TMP_Text>().text = (newPos.x - originalObjectTransform.pos.x).ToString("00.00");
        translateLabelsList[1].GetComponent<TMP_Text>().text = (newPos.y - originalObjectTransform.pos.y).ToString("00.00");
        translateLabelsList[2].GetComponent<TMP_Text>().text = (newPos.z - originalObjectTransform.pos.z).ToString("00.00");
        //translateLabelsList[0].GetComponent<TMP_Text>().text = "";
        //translateLabelsList[1].GetComponent<TMP_Text>().text = "";
        //translateLabelsList[2].GetComponent<TMP_Text>().text = "";

        var x = (newRot.eulerAngles.x - originalObjectTransform.rot.eulerAngles.x);
        var y = (newRot.eulerAngles.y - originalObjectTransform.rot.eulerAngles.y);
        var z = (newRot.eulerAngles.z - originalObjectTransform.rot.eulerAngles.z);

        if (x > 180)
            x -= 360;
        if (y > 180)
            y -= 360;
        if (z > 180)
            z -= 360;

        rotateLabelsList[0].GetComponent<TMP_Text>().text = x.ToString("00.00°");
        rotateLabelsList[1].GetComponent<TMP_Text>().text = y.ToString("00.00°");
        rotateLabelsList[2].GetComponent<TMP_Text>().text = z.ToString("00.00°");

        //rotateLabelsList[0].GetComponent<TMP_Text>().text = ((newRot.eulerAngles.x - originalObjectTransform.rot.eulerAngles.x) * Mathf.Sign(Quaternion.Dot(originalObjectTransform.rot, newRot)) % 180).ToString("00.00°");
        //rotateLabelsList[1].GetComponent<TMP_Text>().text = ((newRot.eulerAngles.y - originalObjectTransform.rot.eulerAngles.y) * Mathf.Sign(Quaternion.Dot(originalObjectTransform.rot, newRot)) % 180).ToString("00.00°");
        //rotateLabelsList[2].GetComponent<TMP_Text>().text = ((newRot.eulerAngles.z - originalObjectTransform.rot.eulerAngles.z) * Mathf.Sign(Quaternion.Dot(originalObjectTransform.rot, newRot)) % 180).ToString("00.00°");

        scaleLabel.GetComponent<TMP_Text>().text = (newScale.x / originalObjectTransform.scale.x).ToString("0.0");
        miniSceneScaleLabel.GetComponent<TMP_Text>().text = (newSceneScale.x / originalSceneScale.x).ToString("0.0");
    }

    /// <summary>
    /// Used to update the value for each label of the object, using new ones as input and determing the ratio with original ones
    /// </summary>
    /// <param name="newPos">New object position</param>
    /// <param name="newRot">New object rotation</param>
    /// <param name="newScale">New object scale</param>
    public void UpdateObjectLabels(Vector3 newPos, Quaternion newRot, Vector3 newScale)
    {
        translateLabelsList[0].GetComponent<TMP_Text>().text = (newPos.x - originalObjectTransform.pos.x).ToString("00.00");
        translateLabelsList[1].GetComponent<TMP_Text>().text = (newPos.y - originalObjectTransform.pos.y).ToString("00.00");
        translateLabelsList[2].GetComponent<TMP_Text>().text = (newPos.z - originalObjectTransform.pos.z).ToString("00.00");

        var x = (newRot.eulerAngles.x - originalObjectTransform.rot.eulerAngles.x);
        var y = (newRot.eulerAngles.y - originalObjectTransform.rot.eulerAngles.y);
        var z = (newRot.eulerAngles.z - originalObjectTransform.rot.eulerAngles.z);

        if (x > 180)
            x -= 360;
        if (y > 180)
            y -= 360;
        if (z > 180)
            z -= 360;

        rotateLabelsList[0].GetComponent<TMP_Text>().text = x.ToString("00.00°");
        rotateLabelsList[1].GetComponent<TMP_Text>().text = y.ToString("00.00°");
        rotateLabelsList[2].GetComponent<TMP_Text>().text = z.ToString("00.00°");

        //rotateLabelsList[0].GetComponent<TMP_Text>().text = Vector3.SignedAngle(originalObjectTransform.pos, newPos, Vector3.right).ToString("00.00°");
        //rotateLabelsList[1].GetComponent<TMP_Text>().text = Vector3.SignedAngle(originalObjectTransform.pos, newPos, Vector3.up).ToString("00.00°");
        //rotateLabelsList[2].GetComponent<TMP_Text>().text = Vector3.SignedAngle(originalObjectTransform.pos, newPos, Vector3.forward).ToString("00.00°");

        scaleLabel.GetComponent<TMP_Text>().text = (newScale.x/originalObjectTransform.scale.x).ToString("0.0");
    }

    /// <summary>
    /// Used to update the value for the mini scene scale label, using new one as input and determing the ratio with original one
    /// </summary>
    /// <param name="newSceneScale">New mini scene scale</param>
    public void UpdateMiniSceneLabel(Vector3 newSceneScale)
    {
        miniSceneScaleLabel.GetComponent<TMP_Text>().text = (newSceneScale.x / originalSceneScale.x).ToString("0.0");
    }

    /// <summary>
    /// Find the nearest handles, 3 for rotation, 3 for translation, and 1 for scale
    /// </summary>
    /// <returns>The list of handles in the order: [nearest rotation handle 1, 2, 3, nearest translation handle 1, 2, 3, nearest scale handle]</returns>
    private List<GameObject> FindNearestHandles()
    {
        playerCamera = GameObject.Find("MixedRealityPlayspace/MRTK-Quest_OVRCameraRig(Clone)/TrackingSpace/CenterEyeAnchor").transform;
        List<GameObject> handlesList = new List<GameObject>();

        /*** Finding nearest rotation handles for each axis ***/
        List<GameObject> rotationHandles = GetComponent<CustomBoundsControl>().RotateHandlesList;
        xRotationHandles = new List<GameObject>();
        yRotationHandles = new List<GameObject>();
        zRotationHandles = new List<GameObject>();

        xRotationHandles.Add(rotationHandles[1]);
        xRotationHandles.Add(rotationHandles[3]);
        xRotationHandles.Add(rotationHandles[9]);
        xRotationHandles.Add(rotationHandles[11]);
        yRotationHandles.Add(rotationHandles[4]);
        yRotationHandles.Add(rotationHandles[5]);
        yRotationHandles.Add(rotationHandles[6]);
        yRotationHandles.Add(rotationHandles[7]);
        zRotationHandles.Add(rotationHandles[0]);
        zRotationHandles.Add(rotationHandles[2]);
        zRotationHandles.Add(rotationHandles[8]);
        zRotationHandles.Add(rotationHandles[10]);

        float distance = Vector3.Distance(playerCamera.position, xRotationHandles[0].transform.position);
        GameObject nearestHandle = xRotationHandles[0];
        foreach (GameObject handle in xRotationHandles)
        {
            if (Vector3.Distance(playerCamera.position, handle.transform.position) <= distance)
            {
                nearestHandle = handle;
                distance = Vector3.Distance(playerCamera.position, handle.transform.position);
            }
        }
        handlesList.Add(nearestHandle);
        distance = Vector3.Distance(playerCamera.position, yRotationHandles[0].transform.position);
        nearestHandle = yRotationHandles[0];
        foreach (GameObject handle in yRotationHandles)
        {
            if (Vector3.Distance(playerCamera.position, handle.transform.position) <= distance)
            {
                nearestHandle = handle;
                distance = Vector3.Distance(playerCamera.position, handle.transform.position);
            }
        }
        handlesList.Add(nearestHandle);
        distance = Vector3.Distance(playerCamera.position, zRotationHandles[0].transform.position);
        nearestHandle = zRotationHandles[0];
        foreach (GameObject handle in zRotationHandles)
        {
            if (Vector3.Distance(playerCamera.position, handle.transform.position) <= distance)
            {
                nearestHandle = handle;
                distance = Vector3.Distance(playerCamera.position, handle.transform.position);
            }
        }
        handlesList.Add(nearestHandle);
        /******************************/

        /*** Finding nearest translation handles for each axis ***/
        List<GameObject> translationHandles = GetComponent<CustomBoundsControl>().TranslateHandleList;
        xTranslationHandles = new List<GameObject>();
        yTranslationHandles = new List<GameObject>();
        zTranslationHandles = new List<GameObject>();

        xTranslationHandles.Add(translationHandles[2]);
        xTranslationHandles.Add(translationHandles[3]);
        yTranslationHandles.Add(translationHandles[4]);
        yTranslationHandles.Add(translationHandles[5]);
        zTranslationHandles.Add(translationHandles[0]);
        zTranslationHandles.Add(translationHandles[1]);

        if (Vector3.Distance(playerCamera.position, xTranslationHandles[0].transform.position) < Vector3.Distance(playerCamera.position, xTranslationHandles[1].transform.position))
        {
            handlesList.Add(xTranslationHandles[0]);
        } else
        {
            handlesList.Add(xTranslationHandles[1]);
        }
        if (Vector3.Distance(playerCamera.position, yTranslationHandles[0].transform.position) < Vector3.Distance(playerCamera.position, yTranslationHandles[1].transform.position))
        {
            handlesList.Add(yTranslationHandles[0]);
        }
        else
        {
            handlesList.Add(yTranslationHandles[1]);
        }
        if (Vector3.Distance(playerCamera.position, zTranslationHandles[0].transform.position) < Vector3.Distance(playerCamera.position, zTranslationHandles[1].transform.position))
        {
            handlesList.Add(zTranslationHandles[0]);
        }
        else
        {
            handlesList.Add(zTranslationHandles[1]);
        }
        /******************************/

        /*** Finding nearest scale handle ***/
        scaleHandles = GetComponent<CustomBoundsControl>().ScaleHandlesList;
        distance = Vector3.Distance(playerCamera.position, scaleHandles[0].transform.position);
        nearestHandle = scaleHandles[0];

        foreach (GameObject handle in scaleHandles)
        {
            if (Vector3.Distance(playerCamera.position, handle.transform.position) <= distance)
            {
                nearestHandle = handle;
                distance = Vector3.Distance(playerCamera.position, handle.transform.position);
            }
        }
        handlesList.Add(nearestHandle);
        /******************************/

        return handlesList;
    }

    private List<GameObject> UpdateNearestHandles()
    {
        List<GameObject> updatedHandlesList = new List<GameObject>();
        float distance = Vector3.Distance(playerCamera.position, xRotationHandles[0].transform.position);
        GameObject nearestHandle = xRotationHandles[0];
        foreach (GameObject handle in xRotationHandles)
        {
            if (Vector3.Distance(playerCamera.position, handle.transform.position) <= distance)
            {
                nearestHandle = handle;
                distance = Vector3.Distance(playerCamera.position, handle.transform.position);
            }
        }
        updatedHandlesList.Add(nearestHandle);
        distance = Vector3.Distance(playerCamera.position, yRotationHandles[0].transform.position);
        nearestHandle = yRotationHandles[0];
        foreach (GameObject handle in yRotationHandles)
        {
            if (Vector3.Distance(playerCamera.position, handle.transform.position) <= distance)
            {
                nearestHandle = handle;
                distance = Vector3.Distance(playerCamera.position, handle.transform.position);
            }
        }
        updatedHandlesList.Add(nearestHandle);
        distance = Vector3.Distance(playerCamera.position, zRotationHandles[0].transform.position);
        nearestHandle = zRotationHandles[0];
        foreach (GameObject handle in zRotationHandles)
        {
            if (Vector3.Distance(playerCamera.position, handle.transform.position) <= distance)
            {
                nearestHandle = handle;
                distance = Vector3.Distance(playerCamera.position, handle.transform.position);
            }
        }
        updatedHandlesList.Add(nearestHandle);
        /******************************/

        /*** Finding nearest translation handles for each axis ***/
        if (Vector3.Distance(playerCamera.position, xTranslationHandles[0].transform.position) < Vector3.Distance(playerCamera.position, xTranslationHandles[1].transform.position))
        {
            updatedHandlesList.Add(xTranslationHandles[0]);
        }
        else
        {
            updatedHandlesList.Add(xTranslationHandles[1]);
        }
        if (Vector3.Distance(playerCamera.position, yTranslationHandles[0].transform.position) < Vector3.Distance(playerCamera.position, yTranslationHandles[1].transform.position))
        {
            updatedHandlesList.Add(yTranslationHandles[0]);
        }
        else
        {
            updatedHandlesList.Add(yTranslationHandles[1]);
        }
        if (Vector3.Distance(playerCamera.position, zTranslationHandles[0].transform.position) < Vector3.Distance(playerCamera.position, zTranslationHandles[1].transform.position))
        {
            updatedHandlesList.Add(zTranslationHandles[0]);
        }
        else
        {
            updatedHandlesList.Add(zTranslationHandles[1]);
        }
        /******************************/

        /*** Finding nearest scale handle ***/
        distance = Vector3.Distance(playerCamera.position, scaleHandles[0].transform.position);
        nearestHandle = scaleHandles[0];

        foreach (GameObject handle in scaleHandles)
        {
            if (Vector3.Distance(playerCamera.position, handle.transform.position) <= distance)
            {
                nearestHandle = handle;
                distance = Vector3.Distance(playerCamera.position, handle.transform.position);
            }
        }
        updatedHandlesList.Add(nearestHandle);
        /******************************/

        return updatedHandlesList;
    }

    private void UpdateLabelsPosition(List<GameObject> handles)
    {
        for (int i = 0; i < handles.Count; i++)
        {
            labelsContainer.transform.GetChild(i).GetComponent<SolverHandler>().TransformOverride = null;
            labelsContainer.transform.GetChild(i).position = handles[i].transform.position;
            labelsContainer.transform.GetChild(i).GetComponent<SolverHandler>().TransformOverride = handles[i].transform;
        }
    }

    private void SaveOriginalTransformValues()
    {
        //originalObjectTransform = new TransformStruct(transform.position, transform.rotation, transform.parent.localScale);
        originalObjectTransform = new TransformStruct(projectedObject.position, projectedObject.localPosition, transform.rotation, transform.parent.localScale);

        originalSceneScale = miniScene.lossyScale;
    }

    /// <summary>
    /// Set the labels visibility
    /// </summary>
    /// <param name="flag"> The flag which toggle the visibility. true = labels are shown. false = labels are hidden</param>
    public void SetLabelsVisibility(bool flag)
    {
        switch (flag)
        {
            case true:
                labelsContainer.SetActive(true);
                miniSceneScaleLabel.SetActive(true);
                allSet = true;
                UpdateAllLabels(projectedObject.position, projectedObject.localPosition, projectedObject.rotation, projectedObject.localScale, miniScene.lossyScale);
                break;
            case false:
                labelsContainer.SetActive(false);
                miniSceneScaleLabel.SetActive(false);
                allSet = false;
                break;
        }
    }

    public void DisableLabels()
    {
        if (labelsContainer.activeSelf)
        {
            labelsContainer.SetActive(false);
        }
    }

    public void EnableLabels()
    {
        if (!labelsContainer.activeSelf)
        {
            labelsContainer.SetActive(true);
        }
    }
}
