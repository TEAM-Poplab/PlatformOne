using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : Singleton<CameraManager>
{
    [SerializeField] private Camera cinematicCamera;
    [SerializeField] private OVRCameraRig ovrPlayer;

    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(gameObject);
        ovrPlayer.gameObject.SetActive(false);
        cinematicCamera.gameObject.SetActive(true);
        cinematicCamera.gameObject.GetComponent<CameraBehavior>().PlayAnimation();
    }

    private void Awake()
    {
        cinematicCamera.gameObject.GetComponent<CameraBehavior>().animationFinished.AddListener(setPlayerActive);
    }

    void setPlayerActive()
    {
        ovrPlayer.gameObject.SetActive(true);
        cinematicCamera.gameObject.SetActive(false);
    }
}
