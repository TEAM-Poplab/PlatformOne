using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractEvents : MonoBehaviour
{
    private float currentPosition;
    private float prevPosition;
    private LinearDrive ld;

    // Start is called before the first frame update
    void Start()
    {
        currentPosition = transform.position.y;
        prevPosition = currentPosition;
        ld = GetComponent<LinearDrive>();
    }

    // Update is called once per frame
    void Update()
    {
        currentPosition = transform.position.y;

        if (currentPosition != prevPosition)
        {
            ld.UpdateLinearMapping(transform);
            prevPosition = currentPosition;
        }
    }
}
