using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LevelSelector : MonoBehaviour
{
    public string levelName;

    private LevelSelectorManager lsm;

    private void Start()
    {
        lsm = GameObject.Find("LevelsMenu").GetComponent<LevelSelectorManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        lsm.levelSelection.Invoke(levelName);
        GetComponent<TMP_Text>().faceColor = new Color32(205, 38, 83, 255);
    }
}