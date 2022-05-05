using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Experimental.UI;
using UnityEngine.Events;
using Microsoft.MixedReality.Toolkit.UI;
using Bolt;
using Ludiq;

public class PlatformDockPosition : MonoBehaviour
{
    [SerializeField]
    [Tooltip("The object that is currently docked in this position (can be null).")]
    private GameObject platformedObject = null;

    public Material materialWhenPlatformed;

    [Space(10)]
    [Header("Events")]
    public PlatformedEvent onObjectPlatformed;
    public PlatformedEvent onObjectPlatformedAfterFreeze;
    public UnityEvent onObjectUnplatformed;
    public PlatformedEvent onObjectPlatformedMS;
    public PlatformedEvent onObjectPlatformedMSAfterFreeze;
    public UnityEvent onObjectUnplatformedMS;

    private GameObject miniScene;
    private Vector3 originalMiniSceneScale;
    private Bounds fittingBounds;
    private Bounds triggerExit;
    private GameObject miniPlatform;

    public GameObject PlatformedObject
    {
        get => platformedObject;
        set => platformedObject = value;
    }

    public Bounds FittingBounds
    {
        get => fittingBounds;
    }

    public GameObject MiniScene
    {
        get => miniScene;
    }

    public bool IsOccupied => platformedObject != null;

    private void Awake()
    {
        onObjectPlatformed = new PlatformedEvent();
        onObjectUnplatformed = new UnityEvent();
        onObjectPlatformedAfterFreeze = new PlatformedEvent();
        onObjectPlatformedMS = new PlatformedEvent();
        onObjectUnplatformedMS = new UnityEvent();
        onObjectPlatformedMSAfterFreeze = new PlatformedEvent();
        miniScene = GameObject.Find("GuardianCenter/Dock/PlatformDockPosition/MiniScene");
        originalMiniSceneScale = miniScene.transform.localScale;
        fittingBounds = GameObject.Find("GuardianCenter/Dock/PlatformDockPosition/FittingBounds").GetComponent<SphereCollider>().bounds;
        triggerExit = GameObject.Find("GuardianCenter/Dock/PlatformDockPosition/TriggerExit").GetComponent<CapsuleCollider>().bounds;
        miniPlatform = transform.GetChild(0).gameObject;
    }

    /// <summary>
    /// Change the scale of the entire miniscene object, which includes the miniplatform, the platformed objects, the manipulation handles. The scale is multiplied by the scaling factor and additional scaling factor
    /// </summary>
    /// <param name="scalingFactor"> The value (proportional both for x, y and z) multiplied by the scale </param>
    /// <param name="additionalScalingFactor">Addition value multiplied by the scaling factor. Use it to reduce the influence of the scaling factor under specific conditions.
    /// If = 1, only scaling factor has influence in the scale change.
    /// If = 0, neither the scaling factor nor the additional scaling factor has influence and the miniscene scale is set to 0</param>
    public void ChangeScale(float scalingFactor, float additionalScalingFactor)
    {
        miniScene.transform.localScale *= scalingFactor * additionalScalingFactor;
    }

    /// <summary>
    /// Reduce the scale of the entire miniscene object, which includes the miniplatform, the platformed objects, the manipulation handles. The scale is reduce by the scaling factor for each dimension
    /// </summary>
    /// <param name="reduceBy">The factor subtracted from each scale dimension</param>
    public void ReduceScale(float reduceBy)
    {
        miniScene.transform.localScale -= new Vector3(reduceBy, reduceBy, reduceBy);
    }

    /// <summary>
    /// Change the scale of the entire miniscene object, which includes the miniplatform, the platformed objects, the manipulation handles. Proportionally add the scaling factor to each scale component
    /// </summary>
    /// <param name="scalingFactor"> The value (proportional both for x, y and z) added to the scale </param>
    /// <param name="useLimits"> If true, upper and lower limits of the scale will be enabled before appling the scale </param>
    public void ChangeScale(float scalingFactor, bool useLimits = false)
    {
        //Conditions to limit the scale of the mini scene to
        if (useLimits)
        {
            if ((platformedObject.transform.lossyScale.x + scalingFactor) <= 0.03f)
            {
                return;
            }
            
            if (!triggerExit.Contains(platformedObject.GetComponent<BoxCollider>().bounds.max))
            {
                if (scalingFactor > 0)
                {
                    return;
                }
            }

            if (!triggerExit.Contains(miniPlatform.transform.GetChild(0).gameObject.GetComponent<MeshRenderer>().bounds.max))
            {
                if (scalingFactor > 0)
                {
                    return;
                }
            }
        }

        miniScene.transform.localScale += new Vector3(scalingFactor, scalingFactor, scalingFactor);
    }

    /// <summary>
    /// Restore the original scale of the entire miniscene object, which includes the miniplatform, the platformed objects, the manipulation handles.
    /// Original scale is the scale of the miniscene at startup
    /// </summary>
    public void RestoreScale()
    {
        miniScene.transform.localScale = originalMiniSceneScale;
    }
}
