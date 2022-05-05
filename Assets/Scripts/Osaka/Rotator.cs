/************************************************************************************
* 
* Class Purpose: controller for every blade rotation and event handler from the RotatorsManager
*
************************************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotator : MonoBehaviour
{
    #region Fields

    //Speed of the animator -> speed of rotation
    private float animatorSpeed = 1f;

    //Semiperiod of the blade
    private float semiperiod = 0f;

    //Animator of the blade with the rotation animation
    private Animator anim;

    //Start delay
    private float delay;

    private bool forwardedInitialization;

    private DigitalClockManagerForOsaka clockManager;

    private Coroutine initPlayCoroutine;
    private Coroutine decreaseTimeCoroutine;
    private Coroutine increaseTimeCoroutine;

    #endregion

    #region Properties

    public float Semiperiod
    {
        get => semiperiod;
        set => semiperiod = value;
    }

    public float Delay
    {
        get => delay;
        set => delay = value;
    }

    public bool ForwardedInitialization
    {
        get => forwardedInitialization;
        set => forwardedInitialization = value;
    }

    #endregion
    /*
     * Setting all values received from the RotatorsManager to the animator parameters
     */
    void Start()
    {
        anim = GetComponent<Animator>();
        animatorSpeed = speedFromPeriod(semiperiod*2);
        anim.SetFloat("Speed", animatorSpeed);
        clockManager = GameObject.Find("TimeManager").GetComponent<DigitalClockManagerForOsaka>();
        decreaseTimeCoroutine = StartCoroutine(decreaseTimeAnimatorSpeed());
        increaseTimeCoroutine = StartCoroutine(increaseTimeAnimatorSpeed());

        if (forwardedInitialization)
        {
            initPlayCoroutine = StartCoroutine(StartForwarding());
        } else
        {
            initPlayCoroutine = StartCoroutine(StartWithDelay(delay));
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (clockManager.decreaseExpMult != 1 && decreaseTimeCoroutine != null)
        {
            decreaseTimeCoroutine = StartCoroutine(decreaseTimeAnimatorSpeed());
        } else if (clockManager.decreaseExpMult != 1 && increaseTimeCoroutine != null)
        {
            increaseTimeCoroutine = StartCoroutine(increaseTimeAnimatorSpeed());
        }
    }

    #region Coroutines

    /*
     * Start the rotation of the blade after a delay
     * @param {float} seconds - Second of delay
     * @out IENumerator
     */
    IEnumerator StartWithDelay(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        anim.SetBool("AnimStatus", true);
    }

    IEnumerator StartForwarding()
    {
        yield return null;
    }

    IEnumerator decreaseTimeAnimatorSpeed()
    {
        while (true)
        {
            if (clockManager.decreaseExpMult != 1)
            {
                anim.SetFloat("Speed", animatorSpeed * clockManager.decreaseExpMult * -1);
            } else
            {
                anim.SetFloat("Speed", animatorSpeed);
            }
            yield return null;
        }
    }

    IEnumerator increaseTimeAnimatorSpeed()
    {
        while (true)
        {
            if (clockManager.increaseExpMult != 1)
            {
                anim.SetFloat("Speed", animatorSpeed * clockManager.increaseExpMult);
            }
            else
            {
                anim.SetFloat("Speed", animatorSpeed);
            }
            yield return null;
        }
    }

    #endregion

    #region Utils methods

    /*
     * Calculator of the rotation speed from the rotation period (in seconds)
     * @param {float} period - The rotation period
     * @out {float} - The speed
     */
    private float speedFromPeriod(float period)
    {
        return (1f / period);
    }

    #endregion
}
