/************************************************************************************
* 
* Class Purpose: class to control the zoom controller, determing the rotation direction
*                   and calculate the scale
*
************************************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZoomController : MonoBehaviour
{
    public PlatformDockPosition platformDockPosition;
    [Tooltip("The scale factor per angle unit to scale the mini scene to")] public float scalingFactorPerAngleUnit = 0.01f;

    private Quaternion startAngle;
    private Quaternion currentAngle;
    private Quaternion previousAngle;

    private Vector3 center;
    private Vector3 initDirVector;
    private Vector3 currentDirVector;
    //private Vector3 previousDirVector;

    private GameObject grabbedHandle;
    private bool grabbed = false;

    private Coroutine rotatingCoroutine;

    private AudioSource audioSource;

    //float deltaAngle = 0;
    //float angle = 0;

    // Start is called before the first frame update
    void Start()
    {
        center = transform.position;
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        //if (grabbed)
        //{
        //    angle = Vector3.SignedAngle(previousDirVector, currentDirVector, transform.up);
        //    platformDockPosition.ChangeScale(scalingFactorPerAngleUnit * angle, true);

        //    deltaAngle += angle;
        //    if (deltaAngle > 5f || deltaAngle < -5f || deltaAngle == 0)
        //    {
        //        audioSource.Play();
        //        deltaAngle = 0;
        //    }

        //    previousDirVector = currentDirVector;
        //}
    }

    /* 
     * Called when a zoom controller handle is being grabbed
     */
    public void OnRotationStarted()
    {   
        grabbed = true;
        rotatingCoroutine = StartCoroutine(Rotating());
    }

    /* 
     * Called when a zoom controller handle is being released
     */
    public void OnRotationEnded()
    {
        grabbed = false;
        grabbedHandle = null;
        //StopCoroutine(rotatingCoroutine);
    }

    /*
     * Coroutine which runs while the user grab any handle and keep the zoom controller rotating.
     * After the handle's release, the coroutine is stopped
     */
    //IEnumerator Rotating()
    //{
    //    Debug.Log("Rotating coroutine started");
    //    float currentRotation;
    //    previousAngle = startAngle;
    //    while(true)
    //    {
    //        if (!grabbed)
    //        {
    //            Debug.Log("Rotating coroutine stopped");
    //            yield break;
    //        }

    //        if (currentAngle.eulerAngles.x > previousAngle.eulerAngles.x)
    //        {
    //            //Counterclockwise rotation
    //            currentRotation = currentAngle.eulerAngles.y - previousAngle.eulerAngles.y;
    //            //currentRotation = Mathf.DeltaAngle(currentAngle.eulerAngles.y, previousAngle.eulerAngles.y);
    //            platformDockPosition.ChangeScale(-scalingFactorPerAngleUnit*currentRotation, true);
    //        } else
    //        {
    //            //clockwise rotation
    //            currentRotation = previousAngle.eulerAngles.y - currentAngle.eulerAngles.y;
    //            //currentRotation = Mathf.DeltaAngle(currentAngle.eulerAngles.y, previousAngle.eulerAngles.y);
    //            platformDockPosition.ChangeScale(scalingFactorPerAngleUnit * currentRotation, true);
    //        }
    //        previousAngle = currentAngle;
    //        currentAngle = transform.rotation;
    //        yield return null;
    //    }
    //}

    //Problems on Android???
    IEnumerator Rotating()
    {
        Debug.Log("Coroutine started!");
        Vector3 previousDirVector = initDirVector;
        float angle;
        float deltaAngle = 0;
        while(true)
        {
            if (!grabbed)
            {
                Debug.Log("Rotating coroutine stopped!");
                previousDirVector = Vector3.zero;
                currentDirVector = Vector3.zero;
                yield break;
            }
            Debug.Log("calculating angle");
            angle = SignedAngle(previousDirVector, currentDirVector, transform.up);
            Debug.Log($"Angle: {angle}");
            Debug.Log("Applying scale change");
            platformDockPosition.ChangeScale(scalingFactorPerAngleUnit * angle, true);

            Debug.Log("Playing sound");
            deltaAngle += angle;
            if (deltaAngle > 5f || deltaAngle < -5f || deltaAngle == 0)
            {
                audioSource.Play();
                deltaAngle = 0;
            }

            Debug.Log("Setting variables");
            previousDirVector = currentDirVector;
            SetCurrentDirVector(grabbedHandle);
            yield return null;
        }
    }

    public void SetInitDirVector(GameObject handleInitDirVector)
    {
        initDirVector = handleInitDirVector.transform.position - center;
        grabbedHandle = handleInitDirVector;
    }

    public void SetCurrentDirVector(GameObject handleCurrentDirVector)
    {
        currentDirVector = handleCurrentDirVector.transform.position - center;
    }

    private float SignedAngle(Vector3 a, Vector3 b, Vector3 n)
    {
        return Vector3.Angle(a,b) * Mathf.Sign(Vector3.Dot(n, Vector3.Cross(a, b)));
    }
}
