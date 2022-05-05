using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineRendererTextureAnimation : MonoBehaviour
{
    public float speedMultiplier = 1f;
    private float count = 0;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        GetComponent<LineRenderer>().material.SetTextureOffset("_MainTex", new Vector2(count, 0));
        count += (0.006f*speedMultiplier);
    }
}
