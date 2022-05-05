using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandInteraction : MonoBehaviour
{
    [SerializeField]
    private Cloth curtainObject;

    private GameObject _handPhysicsL;
    private GameObject _handPhysicsR;
    // Start is called before the first frame update
    void Start()
    {
        _handPhysicsL = GameObject.Find("ColliderCurtainL");
        _handPhysicsR = GameObject.Find("ColliderCurtainR");
        curtainObject.capsuleColliders = AssignColliders();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private CapsuleCollider[] AssignColliders()
    {
        List<CapsuleCollider> childrenCC = new List<CapsuleCollider>();
        foreach(Transform child in _handPhysicsL.transform)
        {
            childrenCC.Add(child.GetComponent<CapsuleCollider>());
        }
        foreach (Transform child in _handPhysicsR.transform)
        {
            childrenCC.Add(child.GetComponent<CapsuleCollider>());
        }
        return childrenCC.ToArray();
    }
}
