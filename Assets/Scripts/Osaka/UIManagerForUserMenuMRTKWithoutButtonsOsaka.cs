/************************************************************************************
* 
* Class Purpose: singleton class which controls any UI related events
*
************************************************************************************/

using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;
using Microsoft.MixedReality.Toolkit.Experimental.UI;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Diagnostics;
using Microsoft.MixedReality.Toolkit.Utilities;
using System;
using Bolt;
using Ludiq;
using Normal.Realtime;

public class UIManagerForUserMenuMRTKWithoutButtonsOsaka : Singleton<UIManagerForUserMenuMRTKWithoutButtonsOsaka>
{
    //[SerializeField] private Canvas _quitMenu;
    [SerializeField] private GameObject digitalClockObject;
    [SerializeField] private GameObject dockObject;
    [SerializeField] private TMP_Text dateText;
    [SerializeField] private GameObject AudioButton;
    [SerializeField] private GameObject FollowMeButton;
    [Tooltip("List of all buttons that only the guide user, i.e, the first user connected, should visualize")][SerializeField] private GameObject[] buttonsForGuideClient;
    [Tooltip("List of all developer menu's buttons that only the guide user, i.e, the first user connected, should visualize")] [SerializeField] private GameObject[] devMenuButtonsForGuideClient;
    private GameObject diagnostics = null;
    private GameObject playspace;

    private List<GameObject> dockedGameobjects = new List<GameObject>();
    private Vector3 dockPreviousPosition = Vector3.zero;
    private Vector3 dockPreviousScale = Vector3.one;

    private bool isDockActive = false;
    private bool isAudioActive = true;

    private Realtime normcoreCoreRT;
    private RealtimeAvatarManager normcoreCoreRAM;

    private void Awake()
    {
        SetDateLabel();
    }

    // Start is called before the first frame update
    void Start()
    {
        //HideDock();
        playspace = GameObject.Find("MixedRealityPlayspace");
        var normcore = GameObject.Find("NormcoreManager");
        normcoreCoreRT = normcore.GetComponent<Realtime>();
        normcoreCoreRAM = normcore.GetComponent<RealtimeAvatarManager>();
        normcoreCoreRT.didConnectToRoom += NormcoreCore_didConnectToRoom;
        normcoreCoreRAM.avatarDestroyed += NormcoreCoreRAM_avatarDestroyed;
        normcoreCoreRAM.avatarCreated += NormcoreCoreRAM_avatarCreated;
    }

    private void NormcoreCoreRAM_avatarCreated(RealtimeAvatarManager avatarManager, RealtimeAvatar avatar, bool isLocalAvatar)
    {
        if (!isLocalAvatar)
        {
            RealtimeAvatar rtavatar;
            normcoreCoreRAM.avatars.TryGetValue(0, out rtavatar);
            if (rtavatar != null && normcoreCoreRT.clientID != 0)
            {
                SetButtonsVisibility(false);
            }
        }
    }

    private void NormcoreCoreRAM_avatarDestroyed(RealtimeAvatarManager avatarManager, RealtimeAvatar avatar, bool isLocalAvatar)
    {
        if (!isLocalAvatar)
        {
            RealtimeAvatar rtavatar;
            normcoreCoreRAM.avatars.TryGetValue(0, out rtavatar);
            if (rtavatar == null)
            {
                SetButtonsVisibility(true);
            }
        }
    }

    private void NormcoreCore_didConnectToRoom(Realtime realtime)
    {
        if (normcoreCoreRT.clientID == 0)
        {
            SetButtonsVisibility(true);
        } else
        {
            SetButtonsVisibility(false);
        }
    }

    private void Update()
    {
        if (diagnostics == null)
        {
            diagnostics = GameObject.Find("MixedRealityPlayspace/Diagnostics");
            diagnostics.SetActive(false);
        }
    }

    public void RealityHandler()
    {

    }

    public void DockHandler()
    {
        switch (isDockActive)
        {
            case true:
                HideDock();
                //dockButtonText.color = new Color32(128, 128, 128, 255);
                break;
            case false:
                ShowDock();
                //dockButtonText.color = new Color32(255, 255, 255, 255);
                break;
        }
    }

    public void ClockHandler()
    {
        switch (digitalClockObject.activeSelf)
        {
            case true:
                digitalClockObject.SetActive(false);
                //clockButtonText.color = new Color32(128, 128, 128, 255);
                break;
            case false:
                digitalClockObject.SetActive(true);
                //clockButtonText.color = new Color32(255, 255, 255, 255);
                break;
        }
    }

    public void ExitHandler()
    {
        ScenesManager.Instance.LoadLevelSelectionScene();
        ScenesManager.Instance.ActivateScene();
    }

    private void HideDock()
    {
        dockObject.GetComponent<Orbital>().enabled = false;
        dockPreviousPosition = dockObject.transform.position;
        dockPreviousScale = dockObject.transform.localScale;
        dockObject.transform.position = Vector3.zero;
        dockObject.transform.localScale = new Vector3(.01f, .01f, .01f);
        isDockActive = false;
    }

    private void ShowDock()
    {
        dockObject.GetComponent<Orbital>().enabled = true;
        dockObject.transform.position = dockPreviousPosition;
        dockObject.transform.localScale = dockPreviousScale;
        isDockActive = true;
    }

    public void DiagnosticHandler()
    {
        switch (diagnostics.activeSelf)
        {
            case true:
                diagnostics.SetActive(false);
                break;
            case false:
                diagnostics.SetActive(true);
                break;
        }
    }

    /// <summary>
    /// Set the current date and time at Osaka in the welcome screen
    /// </summary>
    private void SetDateTimeLabel()
    {
        DateTime osakaTime = DateTime.UtcNow;
        osakaTime = osakaTime.AddHours(9);

        string meridian = osakaTime.Hour >= 12 ? "PM" : "AM";

        dateText.text = $"<size=150%>Osaka \n <size=100%> {osakaTime.Month}/{osakaTime.Day}/1970, {((osakaTime.Hour)%12).ToString("00")}:{osakaTime.Minute.ToString("00")} {meridian}";
    }

    /// <summary>
    /// Set the current date and time at Osaka in the welcome screen
    /// </summary>
    private void SetDateLabel()
    {
        DateTime osakaTime = DateTime.UtcNow;

        dateText.text = $"<size=150%>Osaka \n <size=100%> {osakaTime.Month}/{osakaTime.Day}/1970";
    }

    /// <summary>
    /// Instantly teleport the player to selected position
    /// </summary>
    /// <param name="position">Position where teleport the player to</param>
    public void Teleport(Transform position)
    {
        StartCoroutine(TeleportCoroutine(position));
    }

    IEnumerator TeleportCoroutine(Transform position)
    {
        yield return new WaitForSeconds(0.5f);
        Vector3 displacement;
        int id = (int) Variables.ActiveScene.Get("MyNormID");
        switch(id)
        {
            case 0: displacement = position.TransformPoint(0, 0, 1f);
                break;
            case 1: displacement = position.TransformPoint(-1f, 0, 0);
                break;
            case 2:
                displacement = position.TransformPoint(-1f, 0, 1f);
                break;
            case 3:
                displacement = position.TransformPoint(1f, 0, 0);
                break;
            case 4:
                displacement = position.TransformPoint(1f, 0, 1f);
                break;
            default: displacement = position.position;
                break;
        }
        playspace.transform.position = displacement;
        CustomEvent.Trigger(playspace, "Teleport", displacement);
    }

    /// <summary>
    /// Instantly teleport the player to selected position
    /// </summary>
    /// <param name="position">Position where teleport the player to</param>
    public void Teleport(Transform position, GameObject obj)
    {
        StartCoroutine(TeleportCoroutine(position, obj));
    }

    IEnumerator TeleportCoroutine(Transform position, GameObject obj)
    {
        yield return new WaitForSeconds(0.5f);
        Vector3 displacement;
        int id = (int)Variables.ActiveScene.Get("MyNormID");
        switch (id)
        {
            case 0:
                displacement = position.TransformPoint(0, 0, 1f);
                break;
            case 1:
                displacement = position.TransformPoint(-1f, 0, 0);
                break;
            case 2:
                displacement = position.TransformPoint(-1f, 0, 1f);
                break;
            case 3:
                displacement = position.TransformPoint(1f, 0, 0);
                break;
            case 4:
                displacement = position.TransformPoint(1f, 0, 1f);
                break;
            default:
                displacement = position.position;
                break;
        }
        playspace.transform.position = displacement;
        CustomEvent.Trigger(playspace, "Teleport", displacement);
        Destroy(obj);
    }

    public void SetAudioButton()
    {
        AudioButton.SetActive(true);
        AudioButton.transform.parent.GetComponent<GridObjectCollection>().UpdateCollection();
    }

    public void SetButtonsVisibility(bool isVisible)
    {
        foreach(GameObject button in buttonsForGuideClient)
        {
            button.SetActive(isVisible);
        }
        foreach (GameObject button in devMenuButtonsForGuideClient)
        {
            button.SetActive(isVisible);
        }
        if (normcoreCoreRT.clientID == 0)
        {
            FollowMeButton.SetActive(true);
        } else
        {
            FollowMeButton.SetActive(false);
        }
        buttonsForGuideClient[0].transform.parent.GetComponent<GridObjectCollection>().UpdateCollection();
        devMenuButtonsForGuideClient[0].transform.parent.GetComponent<GridObjectCollection>().UpdateCollection();
    }

    public void AudioHandler()
    {
        switch(isAudioActive)
        {
            case true:
                isAudioActive = false;
                GameObject.Find("NormcoreManager").GetComponent<RealtimeAvatarManager>().localAvatar.GetComponentInChildren<AudioSync>().SetAudio(isAudioActive);
                break;
            case false:
                isAudioActive = true;
                GameObject.Find("NormcoreManager").GetComponent<RealtimeAvatarManager>().localAvatar.GetComponentInChildren<AudioSync>().SetAudio(isAudioActive);
                break;
        }
    }
}
