using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using Bolt;
using Ludiq;

public class LevelSelectorManager : MonoBehaviour
{
    public List<LevelSelector> levels;
    public LevelSelectionEvent levelSelection;
    public GameObject levelButton;


    private string _currentLevelNameSelected = "";

    public string CurrentLevelSelected
    {
        get => _currentLevelNameSelected;
        private set => _currentLevelNameSelected = value;
    }

    // Start is called before the first frame update
    void Start()
    {
        if (levelSelection == null)
            levelSelection = new LevelSelectionEvent();

        levelSelection.AddListener(UpdateCurrentLevel);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void UpdateCurrentLevel(string levelName)
    {
        foreach (LevelSelector ls in levels)
        {
            if (ls.levelName == _currentLevelNameSelected)
            {
                ls.GetComponent<TMP_Text>().faceColor = new Color32(255, 255, 255, 255);
            }
        }

        _currentLevelNameSelected = levelName;
        Debug.Log("Current level selected: " + _currentLevelNameSelected);

        Variables.Object(levelButton).Set("Toggleable", true);
    }

    public void LoadSelectedLevel()
    {
        ScenesManager.Instance.LoadLevel(_currentLevelNameSelected);
    }
}

public class LevelSelectionEvent : UnityEvent<string>
{

}
