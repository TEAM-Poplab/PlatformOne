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

public class UIManager : Singleton<UIManager>
{
    //[SerializeField] private Canvas _quitMenu;
    [SerializeField] private LoadingButtonMeshSequenceController _exitButton;
    [SerializeField] private LoadingButtonMeshSequenceController _lightChangeButton;
    [SerializeField] private GameObject[] clockButtons;
    [SerializeField] private TextMeshProUGUI lightChangeButtonText;
    [SerializeField] private LoadingButtonMeshSequenceController _clockButton;
    [SerializeField] private TextMeshProUGUI clockButtonText;
    [SerializeField] private GameObject digitalClockObject;

    private bool isExitButtonActive = false;
    private bool isLightChangeButtonActive = false;
    private bool isClockButtonActive = false;

    public bool IsExitButtonActive
    {
        get { return isExitButtonActive; }
        set { isExitButtonActive = value; }
    }

    public bool IsLightChangeButtonActive
    {
        get { return isLightChangeButtonActive; }
        set { isLightChangeButtonActive = value; }
    }

    public bool IsClockButtonActive
    {
        get { return isClockButtonActive; }
        set { isClockButtonActive = value; }
    }


    // Start is called before the first frame update
    void Start()
    {
        //DontDestroyOnLoad(gameObject);
        _exitButton.fillAmount = 0;
        _lightChangeButton.fillAmount = 0;
        _clockButton.fillAmount = 0;
        lightChangeButtonText.SetText(GameManager.Instance.LightStatus == GameManager.GameLight.DAY ? "Reality off" : "Reality on");
        clockButtonText.SetText(digitalClockObject.activeSelf ? "Clock off" : "Clock on");
    }

    // Update is called once per frame
    void Update()
    {
        //if (IsExitButtonActive)
        //{
        //    _exitButton.fillAmount += 1.0f / 2.0f * Time.deltaTime;

        //    if (_exitButton.fillAmount == 1)
        //    {
        //        GameManager.Instance.QuitGame();
        //    }
        //} else
        //{
        //    _exitButton.fillAmount = 0;
        //}

        if (IsLightChangeButtonActive)
        {
            _lightChangeButton.fillAmount += 1.0f / 2.0f * Time.deltaTime;
            Debug.Log("Loading button");
            Debug.Log("Fill in manager"+_lightChangeButton.fillAmount);

            if (Mathf.Clamp(_lightChangeButton.fillAmount, 0f, 1f) == 1f)
            {
                Debug.Log("Button loaded");
                switch (GameManager.Instance.LightStatus)
                {
                    case GameManager.GameLight.DAY:
                        GameManager.Instance.SetDaylight(false);
                        digitalClockObject.SetActive(false);
                        clockButtonText.SetText(digitalClockObject.activeSelf ? "Clock off" : "Clock on");
                        break;
                    case GameManager.GameLight.NIGHT:
                        GameManager.Instance.SetDaylight(true);
                        break;
                }
                lightChangeButtonText.SetText(GameManager.Instance.LightStatus == GameManager.GameLight.DAY ? "Reality off" : "Reality on");
                //_lightChangeButton.fillAmount = 0; //to avoid immediate continuous switching while the menu is still opened
                _lightChangeButton.transform.parent.gameObject.SetActive(false); //hiding the button after it has changed the scene light
                IsLightChangeButtonActive = false;
            }
        }
        else
        {
            _lightChangeButton.fillAmount = 0;
        }

        if (IsClockButtonActive)
        {
            _clockButton.fillAmount += 1.0f / 2.0f * Time.deltaTime;
            Debug.Log("Loading button");
            Debug.Log("Fill in manager" + _clockButton.fillAmount);

            if (Mathf.Clamp(_clockButton.fillAmount, 0f, 1f) == 1f)
            {
                Debug.Log("Button loaded");
                switch (digitalClockObject.activeSelf)
                {
                    case true:
                        digitalClockObject.SetActive(false);
                        break;
                    case false:
                        digitalClockObject.SetActive(true);
                        break;
                }
                clockButtonText.SetText(digitalClockObject.activeSelf ? "Clock off" : "Clock on");
                //_lightChangeButton.fillAmount = 0; //to avoid immediate continuous switching while the menu is still opened
                _clockButton.transform.parent.gameObject.SetActive(false); //hiding the button after it has changed the scene light
                IsClockButtonActive = false;
            }
        }
        else
        {
            _clockButton.fillAmount = 0;
        }

        if (IsExitButtonActive)
        {
            _exitButton.fillAmount += 1.0f / 2.0f * Time.deltaTime;
            Debug.Log("Fill in manager" + _exitButton.fillAmount);

            if (Mathf.Clamp(_exitButton.fillAmount, 0f, 1f) == 1f)
            {
                Debug.Log("Exit button loaded");
                ScenesManager.Instance.LoadLevelSelectionScene();
                _exitButton.transform.parent.gameObject.SetActive(false); //hiding the button after it has changed the scene light
                IsExitButtonActive = false;
                ScenesManager.Instance.ActivateScene();
            }
        } else
        {
            _exitButton.fillAmount = 0;
        }
    }

    public void SwapClockButtonState()
    {
        isClockButtonActive = isClockButtonActive == true ? false : true;
    }

    public void ShowClockButtons()
    {
        foreach (GameObject button in clockButtons)
        {
            button.SetActive(true);
        }
    }

    public void HideClockButtons()
    {
        foreach (GameObject button in clockButtons)
        {
            button.SetActive(false);
        }
    }
}
