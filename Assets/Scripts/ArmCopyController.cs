using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmCopyController : MonoBehaviour
{
    [SerializeField]
    private Transform[] fromCopyTransforms;
    [SerializeField]
    private Transform[] toCopyTransforms;

    [Space(15)]
    [SerializeField]
    private Vector3 offset;
    

    private void LateUpdate()
    {
        for (int i = 0; i < fromCopyTransforms.Length && i < toCopyTransforms.Length; i++)
        {
            toCopyTransforms[i].rotation = fromCopyTransforms[i].rotation * Quaternion.Euler(offset.x, offset.y, offset.z);
        }
    }
}