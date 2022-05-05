using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Normal.Realtime;
using Microsoft.MixedReality.Toolkit.Utilities;
using UnityEngine.Events;

/// <summary>
/// The class handles the correct initialization of the scene according to the user who connects to the room
/// </summary>
public class NormcoreManager : MonoBehaviour
{
    [Tooltip("Spawn position for the guest user")] public Transform guestSpawnPosition;
    [Space(10)]
    public UnityEvent onGuestDisconnected;
    [Space(10)]
    [Header("Offline properties")]
    public List<GameObject> ObjectsWithRealtimeComponents = new List<GameObject>();

    private GameObject playspace;
    Vector3 displacement = Vector3.zero;

    private void Awake()
    {
        playspace = GameObject.Find("MixedRealityPlayspace");
        GetComponent<Realtime>().didConnectToRoom += NormcoreCore_didConnectToRoom;
        GetComponent<Realtime>().didDisconnectFromRoom += NormcoreManager_didDisconnectFromRoom;
        GetComponent<RealtimeAvatarManager>().avatarDestroyed += Normcore_avatarDestroyed;
        //StartCoroutine(WaitAndDisconnect());
    }

    private void NormcoreManager_didDisconnectFromRoom(Realtime realtime)
    {
        if (GetComponent<Realtime>().room.connectionState == Room.ConnectionState.Error)
        {
            Room_connectionStateChanged();
        }
    }

    private void Room_connectionStateChanged()
    {
        foreach (GameObject realtime in ObjectsWithRealtimeComponents)
        {
            if (realtime.GetComponent<SyncReality>())
            {
                Destroy(realtime.GetComponent<SyncReality>());
            }
            if (realtime.GetComponent<RealtimeTransform>())
            {
                Destroy(realtime.GetComponent<RealtimeTransform>());
            }
            if (realtime.GetComponent<OwnershipManager>())
            {
                Destroy(realtime.GetComponent<OwnershipManager>());
            }
            if (realtime.GetComponent<RealtimeView>())
            {
                Destroy(realtime.GetComponent<RealtimeView>());
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    private void NormcoreCore_didConnectToRoom(Realtime realtime)
    {
        if (GetComponent<Realtime>().clientID == 1)
        {
            playspace.transform.parent = guestSpawnPosition;
            playspace.transform.localPosition = Vector3.zero;
        }
        if (GetComponent<Realtime>().clientID == 0)
        {
            SetButtonsVisibility(true);
        }
        else
        {
            SetButtonsVisibility(false);
        }
        //StartCoroutine(WaitAndSync());
    }

    private void Normcore_avatarDestroyed(RealtimeAvatarManager avatarManager, RealtimeAvatar avatar, bool isLocalAvatar)
    {
        RealtimeAvatar vtr;
        avatarManager.avatars.TryGetValue(1, out vtr);
        if (vtr == avatar)
        {
            onGuestDisconnected.Invoke();
        }
    }

    private IEnumerator WaitAndSync()
    {
        yield return new WaitForSeconds(1.5f);
        playspace.transform.localPosition = Vector3.zero;
    }

    public void SetButtonsVisibility(bool isVisible)
    {
        foreach (GameObject button in UIManagerForUserMenuMRTKWithoutButtons.Instance.buttonsForDesigner)
        {
            button.SetActive(isVisible);
        }
        UIManagerForUserMenuMRTKWithoutButtons.Instance.buttonsForDesigner[0].transform.parent.GetComponent<GridObjectCollection>().UpdateCollection();
    }
}
