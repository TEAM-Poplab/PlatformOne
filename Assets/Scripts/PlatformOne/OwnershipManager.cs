using Normal.Realtime;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

/// <summary>
/// General class to handle ownership for any object in Platform One
/// </summary>
[RequireComponent(typeof(RealtimeView))]
public class OwnershipManager : MonoBehaviour
{
    [Tooltip("0 for host, 1 for guest")] public int ownerID = 0;
    [Space(15)]
    [Header("Components management")]
    [Tooltip("Components to enable if the client ID is not the one specified")] public List<MonoBehaviour> notOwnerComponentsToEnable = new List<MonoBehaviour>();
    [Tooltip("Components to enable if the client ID is the owner specified")] public List<MonoBehaviour> ownerComponentsToEnable = new List<MonoBehaviour>();
    [FormerlySerializedAs("componentsToDisable")] [Tooltip("Components to disable if the client ID is not the one specified")] public List<MonoBehaviour> notOwnerComponentsToDisable = new List<MonoBehaviour>();
    [Tooltip("Components to disable if the client ID is the owner specified")] public List<MonoBehaviour> ownerComponentsToDisable = new List<MonoBehaviour>();
    [Space(15)]
    [Header("Events management")]
    [Tooltip("Event called when the client connects to the room. ONLY methods with no arguments can be called here. Called if the client is the one specified")] public UnityEvent onOwnerConnectToTheRoom = new UnityEvent();
    [Tooltip("Event called when the guest connects to the room. ONLY methods with no arguments can be called here. Called if the guest is not the one specified")] public UnityEvent onNotOwnerConnectToTheRoom = new UnityEvent();
    [Tooltip("Event called when the client disconnects from the room. ONLY methods with no arguments can be called here. Called if the client is the one specified")] public UnityEvent onOwnerDisconnectFromTheRoom = new UnityEvent();
    [Tooltip("Event called when the guest disconnects from the room. ONLY methods with no arguments can be called here. Called if the guest is not the one specified")] public UnityEvent onNotOwnerDisconnectFromTheRoom = new UnityEvent();

    GameObject normcoreManager;

    public enum Users
    {
        Host = 0,
        Guest = 1
    }
    private void Awake()
    {
        normcoreManager = GameObject.Find("NormcoreManager");
        normcoreManager.GetComponent<Realtime>().didConnectToRoom += NormcoreCore_didConnectToRoom;
        normcoreManager.GetComponent<Realtime>().didDisconnectFromRoom += NormcoreCore_didDisconnectFromRoom;
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
        if (normcoreManager.GetComponent<Realtime>().clientID != ownerID)
        {
            foreach(MonoBehaviour component in notOwnerComponentsToDisable)
            {
                component.enabled = false;
            }
            foreach (MonoBehaviour component in notOwnerComponentsToEnable)
            {
                component.enabled = true;
            }
            onNotOwnerConnectToTheRoom.Invoke();
        }

        if (normcoreManager.GetComponent<Realtime>().clientID == ownerID)
        {         
            GetComponent<RealtimeTransform>().RequestOwnership();
            foreach (MonoBehaviour component in ownerComponentsToDisable)
            {
                component.enabled = false;
            }
            foreach (MonoBehaviour component in ownerComponentsToEnable)
            {
                component.enabled = true;
            }
            onOwnerConnectToTheRoom.Invoke();
        }      
    }

    private void NormcoreCore_didDisconnectFromRoom(Realtime realtime)
    {
        if (normcoreManager.GetComponent<Realtime>().clientID != ownerID)
        {
            onNotOwnerDisconnectFromTheRoom.Invoke();
        }

        if (normcoreManager.GetComponent<Realtime>().clientID == ownerID)
        {
            onOwnerDisconnectFromTheRoom.Invoke();
        }
    }
}
