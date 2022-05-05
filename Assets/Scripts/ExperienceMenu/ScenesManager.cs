/************************************************************************************
* 
* Class Purpose: singleton class which controls any scene loading related events e general
*           game status information
*
************************************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class ScenesManager : Singleton<ScenesManager>
{
    #region Private fields and gloabl variables
    //Game status enumeration data
    public enum GameState
    {
        PREGAME,
        RUNNING,
        PAUSED
    }

    List<AsyncOperation> _loadOperations;

    //Current game state field
    private GameState _currentGameState = GameState.PREGAME;

    //Current loaded level name field
    string _currentLevelName;

    AsyncOperation _lastSceneLoadingOperation;
    #endregion

    #region Public properties
    //Current game state property
    public GameState CurrentGameState
    {
        get { return _currentGameState; }
        private set { _currentGameState = value; }
    }

    //Current loaded level name property
    public string CurrentLevelName
    {
        get { return _currentLevelName; }
        private set { _currentLevelName = value; }
    }
    #endregion

    #region Unity engine methods
    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(gameObject);  //Ensuring the ScenesManager gameobject to be persistent through scenes switching
        _loadOperations = new List<AsyncOperation>();

        float[] freqs = OVRManager.display.displayFrequenciesAvailable;

        switch (freqs.Length)
        {
            case 2:
                OVRPlugin.systemDisplayFrequency = 72.0f;
                break;
            case 4:
                OVRPlugin.systemDisplayFrequency = 90.0f;
                break;
            default:
                OVRPlugin.systemDisplayFrequency = 72.0f;
                break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (_currentGameState == GameState.PREGAME)
        {
            return;
        }
    }
    #endregion

    #region Custom scene management methods
    /*
     * Called when a load operation is completed
     * @param {AsyncOperation} ao - The async operation id
     */
    void OnLoadOperationComplete(AsyncOperation ao)
    {
        if(_loadOperations.Contains(ao))
        {
            _loadOperations.Remove(ao);

            if(_loadOperations.Count == 0)
            {
                UpdateState(GameState.RUNNING);
            }
        }
    }

    /*
     * Called when a unload operation is completed
     * @param {AsyncOperation} ao - The async operation id
     */
    void OnUnloadOperationComplete(AsyncOperation ao)
    {
    }

    /*
     * It updates the game status and execute the proper actions for every status
     * @param {GameState} state - The new status of the game
     */
    void UpdateState(GameState state)
    {
        GameState previousGameState = _currentGameState;
        _currentGameState = state;

        switch(_currentGameState)
        {
            case GameState.PREGAME:
                break;
            case GameState.RUNNING:
                break;
            case GameState.PAUSED:
                break;
            default:
                break;
        }
    }

    /*
     * It loads a new level asynchronously, but it does not activate the new scene
     * @param {string} levelName - The name of the new loading level
     */
    public void LoadLevel(string levelName)
    {
        AsyncOperation ao = SceneManager.LoadSceneAsync(levelName);

        if(ao == null)
        {
            Debug.LogError("[SCENESMANAGER] unable to load level " + levelName + ". Retry again. If error persist, do try to reload the game.k");
            return;
        }

        ao.allowSceneActivation = false;

        ao.completed += OnLoadOperationComplete;
        _loadOperations.Add(ao);

        _lastSceneLoadingOperation = ao;

        _currentLevelName = levelName;
    }

    /*
    * It loads a new level by index asynchronously, but it does not activate the new scene
    * @param {int} index - The index of the new loading level
    */
    public void LoadLevelByIndex(int index)
    {
        AsyncOperation ao = SceneManager.LoadSceneAsync(index);
        string levelName = "LoadingScene";

        if (ao == null)
        {
            Debug.LogError("[SCENESMANAGER] unable to load level " + levelName + ". Retry again. If error persist, do try to reload the game.k");
            return;
        }

        ao.allowSceneActivation = false;

        ao.completed += OnLoadOperationComplete;
        _loadOperations.Add(ao);

        _lastSceneLoadingOperation = ao;

        _currentLevelName = levelName;
    }

    /*
     * It unloads a level asynchronously
     * @param {string} levelName - The name of the  unloading level
     */
    public void UnloadLevel(string levelName)
    {
        AsyncOperation ao = SceneManager.UnloadSceneAsync(levelName);
        ao.completed += OnUnloadOperationComplete;
    }

    /*
     * The method is designed to be used by other objects in order to activate the scene after it has been loaded
     */
    public void ActivateScene()
    {
        AsyncOperation ao = _lastSceneLoadingOperation;

        ao.allowSceneActivation = true;

        if (_currentLevelName == "PlatformOne")
        {
            QualitySettings.antiAliasing = 0;
        } else
        {
            QualitySettings.antiAliasing = 2;
        }
    }
    #endregion

    #region General methods
    /*
     * This method is called when the bush button in the boot scene is pressed. It only loads the main level
     */
    public void LoadMainLevel()
    {
        LoadLevel("PlatformWithHandTracking");
        //OVRManager.display.RecenterPose();
    }

    public void LoadOsakaLevel()
    {
        LoadLevel("Osaka");
    }

    public void LoadLevelSelectionScene()
    {
        //LoadLevel("LoadingScene");
        LoadLevelByIndex(0);
        //OVRManager.display.RecenterPose();
    }
    #endregion

    //The class for custom events. Based on two arguments UnityEvent.
    [System.Serializable] public class EventGameState : UnityEvent<GameState, GameState> { }
}
