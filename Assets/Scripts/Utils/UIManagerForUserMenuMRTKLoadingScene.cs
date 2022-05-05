/************************************************************************************
* 
* Class Purpose: singleton class which controls any UI related events
*
************************************************************************************/

using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;
using Microsoft.MixedReality.Toolkit.Experimental.UI;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit;

public class UIManagerForUserMenuMRTKLoadingScene : Singleton<UIManagerForUserMenuMRTKLoadingScene>
{
    //[SerializeField] private Canvas _quitMenu;
    private GameObject diagnostics = null;
    private Vector3 guardianCenterPosition;

    // Start is called before the first frame update
    void Start()
    {
        guardianCenterPosition = GameObject.Find("GuardianCenter").transform.position;
    }

    private void Update()
    {
        if (diagnostics == null)
        {
            diagnostics = GameObject.Find("MixedRealityPlayspace/Diagnostics");
            diagnostics.SetActive(false);
        }
    }

    public void DiagnosticHandler()
    {
        switch (diagnostics.activeSelf)
        {
            case true:
                diagnostics.SetActive(false);
                break;
            case false:
                diagnostics.SetActive(true);
                break;
        }
    }
}
