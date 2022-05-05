/************************************************************************************
* 
* Class Purpose: singleton class which controls any game related event in Osaka
*
************************************************************************************/

using BeautifyEffect;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using BeautifyEffect;
using System;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine.Serialization;
using Microsoft.MixedReality.Toolkit.Teleport;
using Microsoft.MixedReality.Toolkit;
using prvncher.MixedReality.Toolkit.Input.Teleport;
using Normal.Realtime;
using Bolt;
using Ludiq;


public class GameManagerOsaka : Singleton<GameManagerOsaka>, IMixedRealityTeleportHandler
{
    //[SerializeField] private Beautify beautifyScript;
    [SerializeField] private BeautifyProfile profile;
    [SerializeField] private LoadingButtonMeshSequenceController _lightChangeButton;
    [SerializeField] private TextMeshProUGUI lightChangeButtonText;
    [SerializeField] private LoadingButtonMeshSequenceController _exitButton;

    [SerializeField]
    [FormerlySerializedAs("currentSkybox")]
    private Material daySkybox;

    [SerializeField]
    private Material nightSkybox;

    [SerializeField]
    private GameObject water;

    [SerializeField]
    private GameObject lightGameObject;

    public List<GameObject> meshes = new List<GameObject>();
    public Material realityOffMaterial;
    public Material realityOnMaterial;

    [SerializeField]
    private List<Transform> spawnPositions = new List<Transform>();

    private bool isLightChangeButtonActive = false;

    private Realtime normcoreCoreRT;
    private RealtimeAvatarManager normcoreCoreRAM;
    private GameObject playspace;
    public bool IsLightChangeButtonActive
    {
        get { return isLightChangeButtonActive; }
        set { isLightChangeButtonActive = value; }
    }

    private bool isExitButtonActive = false;
    public bool IsExitButtonActive
    {
        get { return isExitButtonActive; }
        set { isExitButtonActive = value; }
    }

    //The camera which attach the Beautify effect to
    private GameObject _centerCameraAnchor;

    //The private status field
    private OsakaGameLight _osakaLightStatus;

    //The property related to the _status field, it defines the current game light status
    public OsakaGameLight OsakaLightStatus
    {
        get
        {
            return _osakaLightStatus;
        }
    }

    public enum OsakaGameLight
    {
        DAY,
        NIGHT
    }

    private void Awake()
    {
        _osakaLightStatus = OsakaGameLight.DAY;
        if(SceneManager.GetActiveScene().name == "OsakaMaterialsTest")
        {
            GameObject.Find("TimeManager").GetComponent<DigitalClockManager>().StartAutoIncrease();
        }
        //_lightChangeButton.fillAmount = 0;
        //_exitButton.fillAmount = 0;
        //lightChangeButtonText.SetText(_osakaLightStatus == OsakaGameLight.DAY ? "Reality off" : "Reality on");

        //Normcore setting
        playspace = GameObject.Find("MixedRealityPlayspace");
        var normcore = GameObject.Find("NormcoreManager");
        normcoreCoreRT = normcore.GetComponent<Realtime>();
        normcoreCoreRAM = normcore.GetComponent<RealtimeAvatarManager>();
        normcoreCoreRT.didConnectToRoom += NormcoreCore_didConnectToRoom;
    }

    private void Start()
    {
        //normcoreCoreRAM.avatarDestroyed += NormcoreCoreRAM_avatarDestroyed;
        //normcoreCoreRAM.avatarCreated += NormcoreCoreRAM_avatarCreated;


    }

    // Update is called once per frame
    void Update()
    {
        if (_centerCameraAnchor == null)
        {
            _centerCameraAnchor = GameObject.Find("MixedRealityPlayspace/MRTK-Quest_OVRCameraRig(Clone)/TrackingSpace/CenterEyeAnchor");
            SetCameraWithBeautify(_centerCameraAnchor);
        }

        //if (IsLightChangeButtonActive)
        //{
        //    _lightChangeButton.fillAmount += 1.0f / 2.0f * Time.deltaTime;

        //    if (Mathf.Clamp(_lightChangeButton.fillAmount, 0f, 1f) == 1f)
        //    {
        //        switch (_osakaLightStatus)
        //        {
        //            case OsakaGameLight.DAY:
        //                _osakaLightStatus = OsakaGameLight.NIGHT;
        //                water.SetActive(false);
        //                lightGameObject.SetActive(false);
        //                RenderSettings.skybox = nightSkybox;
        //                foreach(GameObject go in meshes)
        //                {
        //                    go.GetComponent<MeshRenderer>().material = realityOffMaterial;
        //                }
        //                break;
        //            case OsakaGameLight.NIGHT:
        //                _osakaLightStatus = OsakaGameLight.DAY;
        //                water.SetActive(true);
        //                lightGameObject.SetActive(true);
        //                RenderSettings.skybox = daySkybox;
        //                foreach (GameObject go in meshes)
        //                {
        //                    go.GetComponent<MeshRenderer>().material = realityOnMaterial;
        //                }
        //                break;
        //        }
        //        lightChangeButtonText.SetText(_osakaLightStatus == OsakaGameLight.DAY ? "Reality off" : "Reality on");
        //        _lightChangeButton.transform.parent.gameObject.SetActive(false); //hiding the button after it has changed the scene light
        //        IsLightChangeButtonActive = false;
        //    }
        //}
        //else
        //{
        //    _lightChangeButton.fillAmount = 0;
        //}

        //if (IsExitButtonActive)
        //{
        //    _exitButton.fillAmount += 1.0f / 2.0f * Time.deltaTime;
        //    Debug.Log("Fill in manager" + _exitButton.fillAmount);

        //    if (Mathf.Clamp(_exitButton.fillAmount, 0f, 1f) == 1f)
        //    {
        //        Debug.Log("Exit button loaded");
        //        ScenesManager.Instance.LoadLevelSelectionScene();
        //        _exitButton.transform.parent.gameObject.SetActive(false); //hiding the button after it has changed the scene light
        //        IsExitButtonActive = false;
        //        ScenesManager.Instance.ActivateScene();
        //    }
        //}
        //else
        //{
        //    _exitButton.fillAmount = 0;
        //}
    }

    private void NormcoreCore_didConnectToRoom(Realtime realtime)
    {
        Vector3 displacement;
        if (normcoreCoreRT.clientID < spawnPositions.Count)
        {
             displacement = spawnPositions[normcoreCoreRT.clientID].position;
        } else
        {
            displacement = spawnPositions[normcoreCoreRT.clientID % spawnPositions.Count].position;
        }
        playspace.transform.position = displacement;
        CustomEvent.Trigger(playspace, "Teleport", displacement);
    }

    /*
     * Attaching and setting the Beautify effect on the main camera which is created on runtime
     * @param {GameObject} cameraGO - The camera which the effect will be applied to
     */
    private void SetCameraWithBeautify(GameObject cameraGO)
    {
        if (!cameraGO.GetComponent<Beautify>())
            cameraGO.AddComponent<Beautify>();

        Beautify bf = cameraGO.GetComponent<Beautify>();
        bf.profile = profile;
        bf.quality = BEAUTIFY_QUALITY.BestPerformance;
    }

    public void OnLightChange()
    {
        switch (_osakaLightStatus)
        {
            case OsakaGameLight.DAY:
                _osakaLightStatus = OsakaGameLight.NIGHT;
                water.SetActive(false);
                lightGameObject.SetActive(false);
                RenderSettings.skybox = nightSkybox;
                foreach (GameObject go in meshes)
                {
                    go.GetComponent<MeshRenderer>().material = realityOffMaterial;
                }
                break;
            case OsakaGameLight.NIGHT:
                _osakaLightStatus = OsakaGameLight.DAY;
                water.SetActive(true);
                lightGameObject.SetActive(true);
                RenderSettings.skybox = daySkybox;
                foreach (GameObject go in meshes)
                {
                    go.GetComponent<MeshRenderer>().material = realityOnMaterial;
                }
                break;
        }
    }

    #region TeleportSystem
    public void OnEscalatorEnter()
    {
        CoreServices.TeleportSystem.Disable();
        CoreServices.TeleportSystem.RegisterHandler<IMixedRealityTeleportHandler>(this);
    }

    public void OnEscalatorExit()
    {
        CoreServices.TeleportSystem.Enable();
        CoreServices.TeleportSystem.UnregisterHandler<IMixedRealityTeleportHandler>(this);
    }

    public void OnTeleportCanceled(TeleportEventData eventData)
    {

    }

    public void OnTeleportCompleted(TeleportEventData eventData)
    {

    }

    public void OnTeleportRequest(TeleportEventData eventData)
    {
        GameObject teleportPrefabRight = GameObject.Find("CustomTeleportPointer Right");
        GameObject teleportPrefabLeft = GameObject.Find("CustomTeleportPointer Left");

        if (teleportPrefabRight)
        {
            teleportPrefabRight.SetActive(false);
            CoreServices.TeleportSystem.RaiseTeleportCanceled(teleportPrefabRight.GetComponent<CustomTeleportPointer>(), teleportPrefabRight.GetComponent<CustomTeleportPointer>().TeleportHotSpot);
        } else
        {
            teleportPrefabLeft.SetActive(false);
            CoreServices.TeleportSystem.RaiseTeleportCanceled(teleportPrefabLeft.GetComponent<CustomTeleportPointer>(), teleportPrefabLeft.GetComponent<CustomTeleportPointer>().TeleportHotSpot);
        }
    }

    public void OnTeleportStarted(TeleportEventData eventData)
    {

    }
    #endregion
}
