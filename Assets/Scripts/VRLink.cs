using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class VRMap

{
    public Transform VRTarget;
    public Transform RigTarget;
    public Vector3 PositionOffset;
    public Vector3 RotationOffset;

    public void Map()
    {
        RigTarget.position = VRTarget.TransformPoint(PositionOffset);
        RigTarget.rotation = VRTarget.rotation * Quaternion.Euler(RotationOffset);
    }
}
public class VRLink : MonoBehaviour
{
    public VRMap Head;
    public VRMap LeftHand;
    public VRMap RightHand;

    public Transform HeadConstraint;
    public Vector3 HeadBodyOffset;

    // Start is called before the first frame update
    void Start()
    {
        HeadBodyOffset = transform.position - HeadConstraint.position;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = HeadConstraint.position + HeadBodyOffset;
        //transform.forward = Vector3.ProjectOnPlane(HeadConstraint.up,Vector3.up).normalized;

        Head.Map();
        LeftHand.Map();
        RightHand.Map();
    }
}
