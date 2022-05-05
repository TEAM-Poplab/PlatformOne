using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LoadingButtonMeshSequenceController : MonoBehaviour
{
    //[HideInInspector]
    [Range(0,1)]
    public float fillAmount = 0;

    private float _fillAmount = 0;
    private float prevFillAmount;

    #region Private properties and fields
    private List<GameObject> _frames = new List<GameObject>();
    private GameObject _currentFrame;
    #endregion

    private void Awake()
    {
        Setup();
        _fillAmount = Mathf.Clamp(fillAmount, 0f, 1f);
        prevFillAmount = _fillAmount;
    }

    // Start is called before the first frame update
    void Start()
    {
        OnFillChange(_fillAmount);
    }

    // Update is called once per frame
    void Update()
    {
        _fillAmount = Mathf.Clamp(fillAmount, 0f, 1f);
        Debug.Log("Fill: "+_fillAmount);

        if (_fillAmount != prevFillAmount)
        {
            OnFillChange(_fillAmount);
            prevFillAmount = _fillAmount;
        }
    }

    private void Setup()
    {
        foreach (Transform child in transform)
        {
            _frames.Add(child.gameObject);
            child.gameObject.SetActive(false);
        }

        //_currentFrame = _frames[0];
        //_currentFrame.gameObject.SetActive(true);
    }

    public void OnFillChange(float fillAmount)
    {
        for(int i = 0; i < _frames.Count; i++)
        {
            if (i < (Mathf.FloorToInt(_frames.Count * fillAmount)))
            {
                _frames[i].SetActive(true);
            } else
            {
                _frames[i].SetActive(false);
            }   
        }
    }
}
