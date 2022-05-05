/************************************************************************************
* 
* Class Purpose: the class controls any interaction with the analog clock
*
************************************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class ClockCollisionDetection : MonoBehaviour
{
    private GameObject hourHand;
    private AimConstraint hourHandConstraint;
    private GameObject handColliderGO = null;
    private void Awake()
    {
        hourHand = transform.Find("Hours").gameObject;
        hourHandConstraint = hourHand.GetComponent<AimConstraint>();
    }
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        //Setting aim contstraint source
        if (handColliderGO == null)
        {
            handColliderGO = GameObject.FindGameObjectWithTag("Clock");
            ConstraintSource source = new ConstraintSource();
            source.sourceTransform = handColliderGO.transform;
            source.weight = 1;
            hourHandConstraint.AddSource(source);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Clock")
        {
                hourHandConstraint.constraintActive = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Clock")
        {
            hourHandConstraint.constraintActive = false;
        }
    }

    //Whenever disabled while interacting with it, che clock kept following the finger event though it was outside the collider.
    //This way we ensure to stop any interaction when the clock disappear
    private void OnDisable()
    {
        hourHandConstraint.constraintActive = false;
    }
}
