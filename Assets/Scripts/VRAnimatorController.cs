using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRAnimatorController : MonoBehaviour
{
    public float speedThreshold = 0.1f;
    [Range(0,1)]
    public float smoothing = 1;
    private Animator animator;
    private Vector3 previousPos;
    private VRLink vrRig;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        vrRig = GetComponent<VRLink>();
        previousPos = vrRig.Head.VRTarget.position;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 headsetSpeed = (vrRig.Head.VRTarget.position - previousPos) / Time.deltaTime;
        headsetSpeed.y = 0;

        Vector3 headsetLocalSpeed = transform.InverseTransformDirection(headsetSpeed);
        previousPos = vrRig.Head.VRTarget.position;


        float previousDirectionX = animator.GetFloat("DirectionX");
        float previousDirectionY = animator.GetFloat("DirectionY");

        animator.SetBool("IsMoving", headsetLocalSpeed.magnitude > speedThreshold);
        animator.SetFloat("DirectionX", Mathf.Lerp(previousDirectionX, Mathf.Clamp(headsetLocalSpeed.x, -1, 1),smoothing));
        animator.SetFloat("DirectionY", Mathf.Lerp(previousDirectionY, Mathf.Clamp(headsetLocalSpeed.z, -1, 1), smoothing));
    }
}
