using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CloudsManagerForOsaka : MonoBehaviour
{
    public AnimationCurve cloudsOpacityAnimation;

    public GameObject[] clouds;

    private CustomLightManagerForOsaka lmScript;
    private DigitalClockManagerForOsaka dcmScript;

    private float TimeOfDay;
    private float prevTimeOfDay;
    private float stepTimeOfDay;

    private float cumulativeDeltaTime;

    // Start is called before the first frame update
    void Start()
    {
        lmScript = GetComponent<CustomLightManagerForOsaka>();
        dcmScript = GetComponent<DigitalClockManagerForOsaka>();
        TimeOfDay = lmScript.TimeOfDay;
        //GameManager.Instance.OnDayLightSet.AddListener(OnDaySet);
        //GameManager.Instance.OnNightLightSet.AddListener(OnNightSet);

        foreach (GameObject cloudsGO in clouds)
        {
            cloudsGO.transform.parent.gameObject.SetActive(true);
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
        //if (TimeOfDay == prevTimeOfDay)
        //{
        //    stepTimeOfDay = Mathf.Lerp(TimeOfDay, TimeOfDay + ((Mathf.FloorToInt(dcmScript.minutesPerCycle) / 60) * 100f / 6000f), cumulativeDeltaTime);
        //    foreach (GameObject cloudsGO in clouds)
        //    {
        //        cloudsGO.GetComponent<MeshRenderer>().material.SetColor("_Color", Color.Lerp(Color.black, Color.white, cloudsOpacityAnimation.Evaluate((stepTimeOfDay - lmScript.dawnTime) / (lmScript.duskTime - lmScript.dawnTime))));
        //    }
        //    cumulativeDeltaTime += Time.deltaTime;
        //}
        //else
        //{
        //    cumulativeDeltaTime = 0;
        //    prevTimeOfDay = TimeOfDay;
        //}

        //TimeOfDay = dcmScript.CurrentTime;
        TimeOfDay = lmScript.TimeOfDay;
        var t = Mathf.InverseLerp(lmScript.dawnTime, lmScript.duskTime, TimeOfDay);

        foreach (GameObject cloudsGO in clouds)
        {
            cloudsGO.GetComponent<MeshRenderer>().material.SetColor("_Color", Color.Lerp(Color.black, Color.white, cloudsOpacityAnimation.Evaluate(t)));
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
