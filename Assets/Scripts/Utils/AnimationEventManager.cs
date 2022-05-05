using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationEventManager : MonoBehaviour
{
    public PlatformLocomotion platformLocomotionControls;

    public void ResetPosition()
    {
        platformLocomotionControls.ResetPosition();
    }
}
