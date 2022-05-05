using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CloudsManager : MonoBehaviour
{
    public AnimationCurve cloudsOpacityAnimation;

    public GameObject[] clouds;

    private CustomLightManager lmScript;

    // Start is called before the first frame update
    void Start()
    {
        lmScript = GetComponent<CustomLightManager>();
        GameManager.Instance.OnDayLightSet.AddListener(OnDaySet);
        GameManager.Instance.OnNightLightSet.AddListener(OnNightSet);
        
        foreach (GameObject cloudsGO in clouds)
        {
            cloudsGO.transform.parent.gameObject.SetActive(false);
        }

        //if (lmScript.useCustomLight)
        //{
        //    clouds.GetComponent<MeshRenderer>().material.color = Color.white;
        //}
            

        cloudsOpacityAnimation.preWrapMode = WrapMode.Once;
        cloudsOpacityAnimation.postWrapMode = WrapMode.Once;
    }

    // Update is called once per frame
    void Update()
    {
        if (lmScript.useCustomLight)
        {
            foreach (GameObject cloudsGO in clouds)
            {
                cloudsGO.GetComponent<MeshRenderer>().material.SetColor("_Color", Color.Lerp(Color.black, Color.white, cloudsOpacityAnimation.Evaluate((lmScript.TimeOfDay - lmScript.dawnTime) / (lmScript.duskTime - lmScript.dawnTime))));
            }
        }
    }

    private void OnNightSet()
    {
        foreach (GameObject cloudsGO in clouds)
        {
            cloudsGO.transform.parent.gameObject.SetActive(false);
        }
    }

    private void OnDaySet()
    {
        foreach (GameObject cloudsGO in clouds)
        {
            cloudsGO.transform.parent.gameObject.SetActive(true);
        };
    }
}
