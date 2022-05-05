using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CameraBehavior : MonoBehaviour
{
    [HideInInspector]
    public Animation _animation;
    public UnityEvent animationFinished;

    // Start is called before the first frame update
    void Start()
    {
        _animation = GetComponent<Animation>();
    }

    public void PlayAnimation()
    {
        _animation.Play("CameraAnimation");
    }

    public void animationHasEnded()
    {
        animationFinished.Invoke();
    }
}
