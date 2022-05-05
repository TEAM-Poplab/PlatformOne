/************************************************************************************
* 
* Class Purpose: implements the Singleton paradigm for game controllers
*
************************************************************************************/

using System.Collections;
using System.Collections.Generic;
//using UnityEditorInternal;
using UnityEngine;

public class Singleton<T> : MonoBehaviour where T: Singleton <T>
{
    private static T _instance;

    public static T Instance
    {
        get
        {
            return _instance;
        }
    }

    public static bool isInitialized
    {
        get
        {
            return _instance != null;
        }
    }

    protected virtual void Awake()
    {
        if (_instance != null)
        {
            Debug.LogErrorFormat("[Singleton] is trying to instantiate a second instance of singleton class {0}", GetType().Name);
        } else
        {
            _instance = (T)this;
        }
    }

    protected virtual void OnDestroy()
    {
        if (_instance = (T)this)
            _instance = null;
    }
}
