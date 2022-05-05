using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <inheritdoc/>
/// <remarks>This class extension supports the handles creation around a mesh sequence object</remarks>
public class CustomBoundsControlMeshSequence : CustomBoundsControl
{
    protected override void DetermineTargetBounds()
    {
        //Box collider for mesh sequence object correctly formatted already exists, so we use it
        collider = target.GetComponent<BoxCollider>();
        collider.size = target.GetComponent<BoxCollider>().size;
        collider.center = target.GetComponent<BoxCollider>().center;

        Bounds bounds = collider.bounds;
        targetBounds.center = bounds.center;
        //targetBounds.size = bounds.size * boundsScale;
        targetBounds.size = Platformable.VectorOfMaxComponent(bounds.size) * boundsScale;
    }

    public override void InitBounds()
    {
        base.InitBounds();
        /// TODO: refix this lately
        handlesContainer.transform.Rotate(0, 55, 0);
    }

    public override void DestroyBounds()
    {
        foreach (GameObject handle in scaleHandlesList)
        {
            Destroy(handle);
        }

        foreach (GameObject handle in rotateHandlesList)
        {
            Destroy(handle);
        }

        foreach (GameObject handle in translateHandlesList)
        {
            //handle.GetComponent<MeshRenderer>().enabled = false;
            //handle.GetComponent<BoxCollider>().enabled = false;
            Destroy(handle);
        }

        Destroy(handlesContainer);

        scaleHandlesList = null;
        rotateHandlesList = null;
        translateHandlesList = null;
        handlesContainer = null;
    }
}
