using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomBoxDisplay : MonoBehaviour
{
    // GameObject used to display the box. Parented to the rig root
    [HideInInspector] public GameObject boxDisplay;
    [SerializeField] private Material displayMaterial;

    [HideInInspector] public bool isVisible = true;

    public void AddBoxDisplay(Transform parent, Vector3 currentBoundsExtents)
    {
        // This has to be cube even in flattened mode as flattened box display can still have a thickness of flattenAxisDisplayScale
        boxDisplay = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Object.Destroy(boxDisplay.GetComponent<Collider>());
        boxDisplay.name = "box display";
        boxDisplay.transform.localScale = currentBoundsExtents;
        boxDisplay.transform.parent = parent;
        boxDisplay.transform.localPosition = Vector3.zero;
        boxDisplay.GetComponent<MeshRenderer>().material = displayMaterial;
    }

    public void AddBoxDisplayWithInitRot(Transform parent, Vector3 currentBoundsExtents)
    {
        // This has to be cube even in flattened mode as flattened box display can still have a thickness of flattenAxisDisplayScale
        boxDisplay = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Object.Destroy(boxDisplay.GetComponent<Collider>());
        boxDisplay.name = "box display";
        boxDisplay.transform.localScale = currentBoundsExtents;
        boxDisplay.transform.parent = parent;
        boxDisplay.transform.localPosition = Vector3.zero;
        boxDisplay.transform.rotation = parent.rotation;
        boxDisplay.GetComponent<MeshRenderer>().material = displayMaterial;
    }

    public void DestroyBoxDisplay()
    {
        if (boxDisplay != null)
        {
            Destroy(boxDisplay);
            GeometryStatusSaver.Instance.SaveBoxExtends(gameObject, boxDisplay.transform.localScale);   //Used especially after unfreeze event
            boxDisplay = null;
        }
    }

    public void RotateBoxDisplay()
    {
        /// TODO: refix this lately
        boxDisplay.transform.Rotate(0, 55, 0);
    }

    public void DisableBoxDisplay()
    {
        if (boxDisplay.activeSelf)
        {
            boxDisplay.SetActive(false);
        }
    }

    public void EnableBoxDisplay()
    {
        if (!boxDisplay.activeSelf)
        {
            boxDisplay.SetActive(true);
        }
    }
}
