using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Normal.Realtime;
using UnityEngine.SceneManagement;

public class SyncSun : MonoBehaviour
{
    private void Awake()
    {
        //GameObject.Find("NormcoreManager").GetComponent<Realtime>().didConnectToRoom += SetLightUponConnection;
    }

    // Start is called before the first frame update
    void Start()
    {
       if (SceneManager.GetActiveScene().name == "Osaka")
       {
            var timeManager = GameObject.Find("TimeManager");
            timeManager.GetComponent<CustomLightManagerForOsaka>().ResetSunRotation();
            timeManager.GetComponent<LightSync>().SetLeastConnectedClientID(GameObject.Find("NormcoreManager").GetComponent<Realtime>().clientID);
            timeManager.GetComponent<LightSync>().RequestOwnership();
       }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void SetLightUponConnection(Realtime realtime)
    {
        if (!GetComponent<RealtimeTransform>().isOwnedLocallySelf)
        {
            GetComponent<RealtimeTransform>().RequestOwnership();
        }

        GameObject.Find("TimeManager").GetComponent<CustomLightManagerForOsaka>().ResetSunRotation();

        GetComponent<RealtimeTransform>().ClearOwnership();

        GetComponent<RealtimeTransform>().enabled = false;
    }
}
