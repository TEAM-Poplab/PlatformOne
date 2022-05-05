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

public class UIManagerForUserMenuMRTK : Singleton<UIManager>
{
    //[SerializeField] private Canvas _quitMenu;
    [SerializeField] private ButtonConfigHelper lightChangeButtonText;
    [SerializeField] private ButtonConfigHelper clockButtonText;
    [SerializeField] private GameObject digitalClockObject;
    [SerializeField] private ButtonConfigHelper dockButtonText;
    [SerializeField] private GameObject dockObject;
    [SerializeField] private LoadingButtonMeshSequenceController _userMenuButton;
    [SerializeField] private TextMeshProUGUI userMenuText;
    [SerializeField] private GameObject userMenu;

    private List<GameObject> dockedGameobjects = new List<GameObject>();
    private Vector3 dockPreviousPosition = Vector3.zero;
    private Vector3 dockPreviousScale = Vector3.one;

    private bool isUserMenuActive = false;
    private bool isDockActive = false;

    public bool IsUserMenuActive
    {
        get { return isUserMenuActive; }
        set { isUserMenuActive = value; }
    }

    // Start is called before the first frame update
    void Start()
    {
        _userMenuButton.fillAmount = 0;
        lightChangeButtonText.MainLabelText = GameManager.Instance.LightStatus == GameManager.GameLight.DAY ? "World on" : "World off";
        clockButtonText.MainLabelText = digitalClockObject.activeSelf ? "Clock on" : "Clock off";
        userMenuText.SetText(userMenu.activeSelf ? "Hide user menu" : "Show user menu");
        HideDock();
        dockButtonText.MainLabelText = isDockActive ? "Dock on" : "Dock off";
    }

    // Update is called once per frame
    void Update()
    {
        if (IsUserMenuActive)
        {
            _userMenuButton.fillAmount += 1.0f / 2.0f * Time.deltaTime;

            if (Mathf.Clamp(_userMenuButton.fillAmount, 0f, 1f) == 1f)
            {
                switch (userMenu.activeSelf)
                {
                    case true:
                        userMenu.SetActive(false);
                        break;
                    case false:
                        userMenu.SetActive(true);
                        userMenu.GetComponent<RadialView>().enabled = true;
                        break;
                }
                userMenuText.SetText(userMenu.activeSelf ? "Hide user menu" : "Show user menu");
                _userMenuButton.transform.parent.gameObject.SetActive(false); //hiding the button after it has changed the scene light
                IsUserMenuActive = false;
            }
        }
        else
        {
            _userMenuButton.fillAmount = 0;
        }
    }

    public void RealityHandler()
    {
        switch (GameManager.Instance.LightStatus)
        {
            case GameManager.GameLight.DAY:
                GameManager.Instance.SetDaylight(false);
                digitalClockObject.SetActive(false);
                clockButtonText.MainLabelText = digitalClockObject.activeSelf ? "Clock on" : "Clock off";
                //lightChangeButtonText.color = new Color32(128, 128, 128, 255);
                break;
            case GameManager.GameLight.NIGHT:
                GameManager.Instance.SetDaylight(true);
                //lightChangeButtonText.color = new Color32(255, 255, 255, 255);
                break;
        }
        lightChangeButtonText.MainLabelText = GameManager.Instance.LightStatus == GameManager.GameLight.DAY ? "World on" : "World off";
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

        dockButtonText.MainLabelText = isDockActive ? "Dock on" : "Dock off";
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
        clockButtonText.MainLabelText = digitalClockObject.activeSelf ? "Clock on" : "Clock off";
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
}
