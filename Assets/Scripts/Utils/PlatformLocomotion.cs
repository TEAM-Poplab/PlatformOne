using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ludiq;
using Bolt;

public class PlatformLocomotion : MonoBehaviour
{
    #region Properties and public fields
    [Header("Controls")]
    public GameObject forwardButton;
    public GameObject backwardButton;
    public GameObject rightwardButton;
    public GameObject leftwardButton;
    public GameObject rightforwardButton;
    public GameObject rightbackwardButton;
    public GameObject leftforwardButton;
    public GameObject leftbackwardButton;
    public GameObject resetButton;

    [Header("Materials"), Space(10)]
    public Material realityOffStationaryMaterial;
    public Material realityOffMovingMaterial;
    public Material realityOnStationaryMaterial;
    public Material realityOnMovingMaterial;

    [Header("Settings"), Space(10)]
    public float motionStep = 0.5f;
    public float inertiaWhenStopping = 2f;
    //private float motionStep;
    [Space(4)]
    public List<GameObject> otherMoveableObjects;
    #endregion

    #region Fields
    private float timeMultiplier = 1f;  //Used when a button is being long pressed to increase motion speed
    private float timer = 0;

    private GameObject playerArea;
    private GameObject platform;
    private GameObject platformMesh;
    private GameObject playspace;
    private GameObject dock;
    private GameObject blackScreen;

    private Vector3 platformStartPosition = Vector3.zero;
    private Vector3 playspaceStartPosition = Vector3.zero;
    private Vector3 dockStartPosition = Vector3.zero;
    private List<Vector3> moveableObjectsStartPosition = new List<Vector3>();
    private Vector3 platformPreviousPosition = Vector3.zero;
    private Vector3 platformPreviousDirection = Vector3.zero;

    Coroutine stop;
    float timeMultiplierInCoroutine;

    private bool isResetEnabled = false;
    private bool isMoving = false;
    #endregion

    #region Unity methods
    private void Awake()
    {
        playerArea = GameObject.Find("MixedRealityPlayspace/PlayerArea");
        platform = GameObject.Find("GuardianCenter/Platform");
        platformMesh = platform.transform.GetChild(0).gameObject;
        playspace = GameObject.Find("MixedRealityPlayspace");
        dock = GameObject.Find("GuardianCenter/Dock");
        blackScreen = GameObject.Find("GuardianCenter/BlackScreen");
        dockStartPosition = GameObject.Find("DockPlaceholder").transform.position;

        //motionStep = Mathf.Sqrt(motionStep);
    }


    // Start is called before the first frame update
    void Start()
    {
        platformStartPosition = platform.transform.position;
        playspaceStartPosition = playspace.transform.position;

        foreach(GameObject go in otherMoveableObjects)
        {
            moveableObjectsStartPosition.Add(go.transform.position);
        }

        UIManagerForUserMenuMRTKWithoutButtons.Instance.dockIsVisible.AddListener(SaveDockPosition);
        GameManager.Instance.OnDayLightSet.AddListener(lightOnMaterialChange);
        GameManager.Instance.OnNightLightSet.AddListener(lightOffMaterialChange);
    }

    // Update is called once per frame
    void Update()
    {
        if ((bool)Variables.Object(forwardButton).Get("Pressed"))
        {
            MoveForward();
            isResetEnabled = true;
            isMoving = true;
            CheckLongPressure();
            platformPreviousDirection = -playerArea.transform.up;
        }
        else if ((bool)Variables.Object(backwardButton).Get("Pressed"))
        {
            MoveBackward();
            isResetEnabled = true;
            isMoving = true;
            CheckLongPressure();
            platformPreviousDirection = playerArea.transform.up;
        }
        else if ((bool)Variables.Object(leftwardButton).Get("Pressed"))
        {
            MoveLeftward();
            isResetEnabled = true;
            isMoving = true;
            CheckLongPressure();
            platformPreviousDirection = -playerArea.transform.right;
        }
        else if ((bool)Variables.Object(rightwardButton).Get("Pressed"))
        {
            MoveRightward();
            isResetEnabled = true;
            isMoving = true;
            CheckLongPressure();
            platformPreviousDirection = playerArea.transform.right;
        }
        else if ((bool)Variables.Object(rightforwardButton).Get("Pressed"))
        {
            MoveRightForward();
            isResetEnabled = true;
            isMoving = true;
            CheckLongPressure();
            platformPreviousDirection = playerArea.transform.right + -playerArea.transform.up;
        }
        else if ((bool)Variables.Object(rightbackwardButton).Get("Pressed"))
        {
            MoveRightBackward();
            isResetEnabled = true;
            isMoving = true;
            CheckLongPressure();
            platformPreviousDirection = playerArea.transform.right + playerArea.transform.up;
        }
        else if ((bool)Variables.Object(leftforwardButton).Get("Pressed"))
        {
            MoveLeftForward();
            isResetEnabled = true;
            isMoving = true;
            CheckLongPressure();
            platformPreviousDirection = -playerArea.transform.right + -playerArea.transform.up;
        }
        else if ((bool)Variables.Object(leftbackwardButton).Get("Pressed"))
        {
            MoveLeftBackward();
            isResetEnabled = true;
            isMoving = true;
            CheckLongPressure();
            platformPreviousDirection = -playerArea.transform.right + playerArea.transform.up;
        }
        else if ((bool)Variables.Object(resetButton).Get("Pressed") && isResetEnabled)
        {
            blackScreen.GetComponent<Animator>().SetTrigger("Fade");
            isResetEnabled = false;
            isMoving = false;
        } else
        {
            if (timeMultiplier > 1 && stop == null)
            {
                stop = StartCoroutine(smoothStop());
            }
            timer = 0;
            timeMultiplier = 1f;
        }

        if (isMoving)
        {
            switch (GameManager.Instance.LightStatus)
            {
                case GameManager.GameLight.DAY:
                    platformMesh.GetComponent<MeshRenderer>().material = realityOnMovingMaterial;
                    break;
                case GameManager.GameLight.NIGHT:
                    platformMesh.GetComponent<MeshRenderer>().material = realityOffMovingMaterial;
                    break;
            }
        }

    }
    #endregion

    #region Motion methods
    /// <summary>
    /// Moves the platform forward. The motion step is determined by the time, the step defined by user and the step increased only when the button is long pressed
    /// </summary>
    private void MoveForward()
    {
        platformPreviousPosition = platform.transform.position;
        platform.transform.position += (-playerArea.transform.up) * motionStep * timeMultiplier * Time.deltaTime;
        var platformMotionDiff = (platform.transform.position - platformPreviousPosition).normalized * motionStep * timeMultiplier * Time.deltaTime;
        playspace.transform.position += platformMotionDiff;
        dock.transform.position += platformMotionDiff;

        foreach (GameObject go in otherMoveableObjects)
        {
            go.transform.position += platformMotionDiff;
        }
    }

    /// <summary>
    /// Moves the platform backward. The motion step is determined by the time, the step defined by user and the step increased only when the button is long pressed
    /// </summary>
    private void MoveBackward()
    {
        platformPreviousPosition = platform.transform.position;
        platform.transform.position += playerArea.transform.up * motionStep * timeMultiplier * Time.deltaTime;
        var platformMotionDiff = (platform.transform.position - platformPreviousPosition).normalized * motionStep * timeMultiplier * Time.deltaTime;
        playspace.transform.position += platformMotionDiff;
        dock.transform.position += platformMotionDiff;

        foreach (GameObject go in otherMoveableObjects)
        {
            go.transform.position += platformMotionDiff;
        }
    }

    /// <summary>
    /// Moves the platform leftward. The motion step is determined by the time, the step defined by user and the step increased only when the button is long pressed
    /// </summary>
    private void MoveLeftward()
    {
        platformPreviousPosition = platform.transform.position;
        platform.transform.position += -playerArea.transform.right * motionStep * timeMultiplier * Time.deltaTime;
        var platformMotionDiff = (platform.transform.position - platformPreviousPosition).normalized * motionStep * timeMultiplier * Time.deltaTime;
        playspace.transform.position += platformMotionDiff;
        dock.transform.position += platformMotionDiff;

        foreach (GameObject go in otherMoveableObjects)
        {
            go.transform.position += platformMotionDiff;
        }
    }

    /// <summary>
    /// Moves the platform rightward. The motion step is determined by the time, the step defined by user and the step increased only when the button is long pressed
    /// </summary>
    private void MoveRightward()
    {
        platformPreviousPosition = platform.transform.position;
        platform.transform.position += playerArea.transform.right * motionStep * timeMultiplier * Time.deltaTime;
        var platformMotionDiff = (platform.transform.position - platformPreviousPosition).normalized * motionStep * timeMultiplier * Time.deltaTime;
        playspace.transform.position += platformMotionDiff;
        dock.transform.position += platformMotionDiff;

        foreach (GameObject go in otherMoveableObjects)
        {
            go.transform.position += platformMotionDiff;
        }
    }

    /// <summary>
    /// Moves the platform rightforward. The motion step is determined by the time, the step defined by user and the step increased only when the button is long pressed
    /// </summary>
    /// <remarks>Combines forward and rightward methods</remarks>
    private void MoveRightForward()
    {
        MoveRightward();
        MoveForward();
    }

    /// <summary>
    /// Moves the platform rightbackward. The motion step is determined by the time, the step defined by user and the step increased only when the button is long pressed
    /// </summary>
    /// <remarks>Combines backward and rightward methods</remarks>
    private void MoveRightBackward()
    {
        MoveRightward();
        MoveBackward();
    }

    /// <summary>
    /// Moves the platform leftward. The motion step is determined by the time, the step defined by user and the step increased only when the button is long pressed
    /// </summary>
    /// <remarks>Combines forward and leftward methods</remarks>
    private void MoveLeftForward()
    {
        MoveLeftward();
        MoveForward();
    }

    /// <summary>
    /// Moves the platform leftbackward. The motion step is determined by the time, the step defined by user and the step increased only when the button is long pressed
    /// </summary>
    /// <remarks>Combines backward and leftward methods</remarks>
    private void MoveLeftBackward()
    {
        MoveLeftward();
        MoveBackward();
    }

    private IEnumerator smoothStop()
    {
        //var motionDir = platformPreviousDirection.normalized * motionStep * timeMultiplier * inertiaWhenStopping;
        //var startPosition = platform.transform.position;
        //var targetPosition = motionDir + startPosition;
        //Vector3 previousPosition = platform.transform.position;
        ////float t = Time.deltaTime * inertiaWhenStopping;
        //float t = 0;
        //float timeElapsed = 0;
        //float lerpDuration = 1f;
        //while (timeElapsed < lerpDuration)
        //{
        //    platform.transform.position = Vector3.Lerp(startPosition, targetPosition, t);
        //    playspace.transform.position += platform.transform.position - previousPosition;
        //    dock.transform.position += platform.transform.position - previousPosition;
        //    foreach (GameObject go in otherMoveableObjects)
        //    {
        //        go.transform.position += platform.transform.position - previousPosition;
        //    }
        //    previousPosition = platform.transform.position;
        //    timeElapsed += Time.deltaTime;
        //    yield return null;
        //}

        Vector3 previousPosition = platform.transform.position;
        float timeMultiplierStartValue = timeMultiplier;
        timeMultiplierInCoroutine = timeMultiplierStartValue;
        float timeElapsed = 0;
        while (timeElapsed < 1f)
        {
            platform.transform.position += platformPreviousDirection.normalized * motionStep * timeMultiplierInCoroutine * Time.deltaTime;
            playspace.transform.position += platform.transform.position - previousPosition;
            dock.transform.position += platform.transform.position - previousPosition;
            foreach (GameObject go in otherMoveableObjects)
            {
                go.transform.position += platform.transform.position - previousPosition;
            }
            previousPosition = platform.transform.position;
            timeElapsed += Time.deltaTime;
            timeMultiplierInCoroutine = Mathf.Lerp(timeMultiplierStartValue, 1, Mathf.Sin(timeElapsed*Mathf.PI*0.5f));
            yield return null;
        }
        stop = null;
    }

    /// <summary>
    /// Reset the platform position to the world center (as if the scene was just loaded), cancelling any motion applied
    /// </summary>
    public void ResetPosition()
    {
        platform.transform.position = platformStartPosition;
        playspace.transform.position = playspaceStartPosition;

        if (UIManagerForUserMenuMRTKWithoutButtons.Instance.IsDockActive)
        {
            dock.transform.position = dockStartPosition;
        }
        
        for(int i = 0; i < otherMoveableObjects.Count; i++)
        {
            otherMoveableObjects[i].transform.position = moveableObjectsStartPosition[i];
        }

        switch (GameManager.Instance.LightStatus)
        {
            case GameManager.GameLight.DAY:
                platformMesh.GetComponent<MeshRenderer>().material = realityOnStationaryMaterial;
                break;
            case GameManager.GameLight.NIGHT:
                platformMesh.GetComponent<MeshRenderer>().material = realityOffStationaryMaterial;
                break;
        }
    }
    #endregion

    #region Util methods
    private void SaveDockPosition()
    {
        dockStartPosition = dock.transform.localPosition;
    }

    private void lightOffMaterialChange()
    {
        if (platform.transform.position != platformStartPosition)
        {
            platformMesh.GetComponent<MeshRenderer>().material = realityOffMovingMaterial;
        } else
        {
            platformMesh.GetComponent<MeshRenderer>().material = realityOffStationaryMaterial;
        }
    }

    private void lightOnMaterialChange()
    {
        if (platform.transform.position != platformStartPosition)
        {
            platformMesh.GetComponent<MeshRenderer>().material = realityOnMovingMaterial;
        } else
        {
            platformMesh.GetComponent<MeshRenderer>().material = realityOnStationaryMaterial;
        }
    }

    /// <summary>
    /// Check if the button is being pressed: if so, after a time step, the long pressure multiplier is incremented
    /// </summary>
    private void CheckLongPressure()
    {
        if (stop != null)
        {
            StopCoroutine(stop);
            stop = null;
            timer = 2f;
            timeMultiplier = timeMultiplierInCoroutine;
        }

        if (timer > 2f)
        {
            timeMultiplier += Time.deltaTime;
        } else
        {
            timer += Time.deltaTime;
        }
    }
    #endregion
}
