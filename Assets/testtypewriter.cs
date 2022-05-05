using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KoganeUnityLib;
using System;

public class testtypewriter : MonoBehaviour
{
    public Animator fadeAnimator;
    public List<string> phrases;
    [HideInInspector]
    public string text = "";
    public float speed = 1f;
    public int delay = 0;

    //Update is called once per frame
    void Update()
    {
        //    if(Input.GetKeyDown("space"))
        //    {
        //        GetComponent<TMP_Typewriter>().Play(text, speed, null);
        //        fadeAnimator.SetTrigger("Play");
        //    }
    }
}
