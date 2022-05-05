/************************************************************************************
* 
* Class Purpose: MonoBehavior class which handles a material changing event according to
*   the scene light status, allowing to choose which material assign to the corresponding
*   GameObject
*
************************************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SkinnedMeshRenderer))]
public class ChangeMaterialsEventHandlerSkinnedMesh : MonoBehaviour
{
    [Tooltip("The daylight material to assing to the object when the scene light is in day status")]
    public Material dayMaterial;

    [Tooltip("The nightlight material to assing to the object when the scene light is in night status")]
    public Material nightMaterial;

    public void ChangeMaterial()
    {
        switch (GameManager.Instance.LightStatus)
        {
            case GameManager.GameLight.DAY:
                GetComponent<SkinnedMeshRenderer>().material = dayMaterial;
                break;
            case GameManager.GameLight.NIGHT:
                GetComponent<SkinnedMeshRenderer>().material = nightMaterial;
                break;
        }
    }

    //It is used in order to force changing for some situations in our project (e.g: when a mesh becomes visible again
    //    when player enters in the corresponding area)
    private void OnBecameVisible()
    {
        ChangeMaterial();
    }
}
