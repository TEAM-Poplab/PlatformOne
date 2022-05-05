using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshVisualizer : MonoBehaviour
{
    public List<MeshRenderer> toggleableRenderers;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            foreach (MeshRenderer mr in toggleableRenderers)
            {
                mr.enabled = true;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            foreach (MeshRenderer mr in toggleableRenderers)
            {
                mr.enabled = false;
            }
        }
    }
}
