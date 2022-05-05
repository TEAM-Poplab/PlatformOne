using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRLegsIK : MonoBehaviour
{
    private Animator animator;
    public Vector3 FootOffset;
    [Range(0, 1)]
    public float rightfootposweight = 1;
    [Range(0, 1)]
    public float rightfootrotweight = 1;
    [Range(0, 1)]
    public float leftfootposweight = 1;
    [Range(0, 1)]
    public float leftfootrotweight = 1;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    private void OnAnimatorIK(int layerIndex)
    {
        Vector3 RightFootPos = animator.GetIKPosition(AvatarIKGoal.RightFoot);
        RaycastHit hit;

        bool hasHit = Physics.Raycast(RightFootPos + Vector3.up, Vector3.down, out hit);
        if(hasHit)
        {
            animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, rightfootposweight);
            animator.SetIKPosition(AvatarIKGoal.RightFoot, hit.point+ FootOffset);

            Quaternion RightFootRotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(transform.forward, hit.normal), hit.normal);
            animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, rightfootrotweight);
            animator.SetIKRotation(AvatarIKGoal.RightFoot, RightFootRotation);
        }
        else
        {
            animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 0);
        }

        Vector3 LeftFootPos = animator.GetIKPosition(AvatarIKGoal.LeftFoot);
       
        hasHit = Physics.Raycast(LeftFootPos + Vector3.up, Vector3.down, out hit);
        if (hasHit)
        {
            animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, leftfootposweight);
            animator.SetIKPosition(AvatarIKGoal.LeftFoot, hit.point + FootOffset);

            Quaternion LeftFootRotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(transform.forward, hit.normal), hit.normal);
            animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, leftfootrotweight);
            animator.SetIKRotation(AvatarIKGoal.LeftFoot, LeftFootRotation);
        }
        else
        {
            animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 0);
        }
    }
}
