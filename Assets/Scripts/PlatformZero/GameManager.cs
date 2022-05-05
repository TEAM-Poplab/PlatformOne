/************************************************************************************
* 
* Class Purpose: singleton class which controls any game related event
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

public class GameManager : Singleton<GameManager>
{
    //[SerializeField] private Beautify beautifyScript;
    [SerializeField]
    [Tooltip("Beautify Profile for lights off scene")]
    private BeautifyProfile beautifyProfileLightsOff;

    [SerializeField]
    [Tooltip("Beautify Profile for lights on scene")]
    private BeautifyProfile beautifyProfileLightsOn;

    public GameObject fingerColliderPrefab;
    public GameObject lightChangeMenuObject;
    //public float lightMenuDelay = 4f;

    public bool useAnalogClock = true;

    public Material outlineMaterial;

    [Header("Day status events")]
    //Events called when there is a scene light status change
    public UnityEvent OnDayLightSet;
    public UnityEvent OnNightLightSet;
    //public UnityEvent OnAnyLightSet;

    //The camera which attach the Beautify effect to
    private GameObject _centerCameraAnchor;

    private GameObject _indexTip;

    //The directional light component of the directional light object in the scene
    private Light _directionalLight;
    private Light _pointLight;

    //The private status field
    private GameLight _lightStatus;

    private CustomLightManager lmScript;
    private float _timer = 0f;

    //The property related to the _status field, it defines the current game light status
    public GameLight LightStatus
    {
        get
        {
            return _lightStatus;
        }
    }

    public enum GameLight
    {
        DAY,
        NIGHT
    }

    private Vector3 dim;

    // Start is called before the first frame update
    void Start()
    {
        //DontDestroyOnLoad(gameObject);
        _directionalLight = GameObject.Find("Directional Light").GetComponent<Light>();
        //_pointLight = GameObject.Find("Point Light").GetComponent<Light>();
        RenderSettings.sun = _directionalLight;
        _lightStatus = GameLight.NIGHT;
        GameObject.Find("MixedRealityPlayspace")?.transform.Translate(new Vector3(0, 1, 0), Space.World);
        lmScript = GameObject.Find("TimeManager").GetComponent<CustomLightManager>();

        if (lmScript.useCustomLight)
        {
            _directionalLight.gameObject.SetActive(false);
            //GameObject.Find("LightMenu")?.gameObject.SetActive(false);
            float startTimeOfDay = lmScript.HoursMinutesToFloat(DateTime.Now.Hour, DateTime.Now.Minute);
            if (startTimeOfDay > lmScript.dawnTime && startTimeOfDay < lmScript.duskTime)
            {
                _lightStatus = GameLight.DAY;
                OnDayLightSet.Invoke();
            }
        }

        //Hand outline
        var hand = GameObject.Find("OVRHandPrefab_Left");
        hand.AddComponent<Outline>();
        hand.GetComponent<Outline>().OutlineColor = new Color(0.2627451f, 0.3411765f, 0.6784314f, 1f);
        hand.GetComponent<Outline>().OutlineMode = Outline.Mode.SilhouetteOnly;
        hand.GetComponent<Outline>().enabled = true;
        hand = GameObject.Find("OVRHandPrefab_Right");
        hand.AddComponent<Outline>();
        hand.GetComponent<Outline>().OutlineColor = new Color(0.2627451f, 0.3411765f, 0.6784314f, 1f);
        hand.GetComponent<Outline>().OutlineMode = Outline.Mode.SilhouetteOnly;
        hand.GetComponent<Outline>().enabled = true;

        //lightChangeMenuObject.SetActive(false);
        //OVRManager.display.RecenterPose();

        GameObject.Find("MixedRealityPlayspace/MRTK-Quest_OVRCameraRig(Clone)").GetComponent<OVRManager>();
    }

    // Update is called once per frame
    void Update()
    {
        //if(_timer >= lightMenuDelay && lightChangeMenuObject.activeSelf == false)
        //{
        //    lightChangeMenuObject.SetActive(true);
        //}

        if (_centerCameraAnchor == null)
        {
            _centerCameraAnchor = GameObject.Find("MixedRealityPlayspace/MRTK-Quest_OVRCameraRig(Clone)/TrackingSpace/CenterEyeAnchor");
            SetCameraWithBeautify(_centerCameraAnchor, beautifyProfileLightsOff);
        }

        if (useAnalogClock)
        {
            if (_indexTip == null)
            {
                _indexTip = GameObject.Find("MixedRealityPlayspace/MRTK-Quest_OVRCameraRig(Clone)/TrackingSpace/RightHandAnchor/OVRHandPrefab_Right/Bones/Hand_Start/Hand_Index1/Hand_Index2/Hand_Index3/Hand_IndexTip");
                if (_indexTip != null)
                {
                    GameObject fingerCollider = Instantiate(fingerColliderPrefab);
                    fingerCollider.transform.SetParent(_indexTip.transform, false);
                }
            }
        }

        _timer += Time.deltaTime;

        //Debug.Log(OVRManager.boundary.GetDimensions(OVRBoundary.BoundaryType.OuterBoundary));
    }

    public void QuitGame()
    {
        Debug.LogWarning("Quitting Game...");
        Application.Quit();
    }

    /*
     * Attaching and setting the Beautify effect on the main camera which is created on runtime
     * @param {GameObject} cameraGO - The camera which the effect will be applied to
     * @param {BeautifyProfile} profile - The Beautify profile applied to the camera
     */
    private void SetCameraWithBeautify(GameObject cameraGO, BeautifyProfile profile)
    {
        if (!cameraGO.GetComponent<Beautify>())
            cameraGO.AddComponent<Beautify>();

        Beautify bf = cameraGO.GetComponent<Beautify>();
        bf.profile = profile;
        bf.quality = BEAUTIFY_QUALITY.BestPerformance;
    }

    /*
     * Change the scene light status and set any light change event related
     * @param {GameLight} status - The status the scene light will be set to
     */
    protected void OnLightChange(GameLight status)
    {
        if (status == GameLight.NIGHT)
        {
            //if (_directionalLight.gameObject.activeSelf)
            //    _directionalLight.GetComponent<Animator>().SetBool("IsDay", false);
            _lightStatus = GameLight.NIGHT;
            _directionalLight.gameObject.SetActive(true);
            SetCameraWithBeautify(_centerCameraAnchor, beautifyProfileLightsOff);
            RenderSettings.sun = _directionalLight;
            lmScript.useCustomLight = false;
            OnNightLightSet.Invoke();
        } else
        {
            //if (_directionalLight.gameObject.activeSelf)
            //    _directionalLight.GetComponent<Animator>().SetBool("IsDay", true);
            _lightStatus = GameLight.DAY;
            _directionalLight.gameObject.SetActive(false);
            SetCameraWithBeautify(_centerCameraAnchor, beautifyProfileLightsOn);   //shadows are cpu consuming, so any other effect will be desabled for better performance
            lmScript.useCustomLight = true;
            OnDayLightSet.Invoke();
        }
        //OnAnyLightSet.Invoke();
    }

    /*
     * Public method to change the scene light and its status
     * @param {bool} val - If true it sets a day light, if false it sets a night light
     */
    public void SetDaylight(bool val)
    {
        if (val)
            OnLightChange(GameLight.DAY);
        else
            OnLightChange(GameLight.NIGHT);
    }

    public void OnLightChangeMaterialForGHObjects()
    {
        string[] ghObjects = { "ObjectA", "ObjectB", "ObjectC" };

        foreach (string obj in ghObjects)
        {
            GameObject[] objectToChange = GameObject.FindGameObjectsWithTag(obj);

            foreach (GameObject objs in objectToChange)
            {
                objs?.GetComponent<ChangeMaterialsEventHandler>().ChangeMaterial();
            }
            
        }
    }
}
