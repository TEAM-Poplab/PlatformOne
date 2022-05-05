using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class manages the use of any MATKPointer in a single scene. This component works with MATKPointer and MUST be implemeneted whenever a scene use one or more MATKPointers
/// </summary>
public class MATKPointerManager : Singleton<MATKPointerManager>
{
    #region Fields
    private GameObject currentObjectOnFocus = null;
    private List<MATKPointer> pointersInScene = new List<MATKPointer>();
    private MATKPointer activePointerInScene = null;
    private Coroutine wait = null;
    #endregion

    /// <summary>
    /// The current object selected by the active MATKPointer
    /// </summary>
    public GameObject CurrentObjectOnFocus
    {
        get => currentObjectOnFocus;
        set => currentObjectOnFocus = value;
    }

    /// <summary>
    /// The current active MATKpointer in scene
    /// </summary>
    public MATKPointer ActivePointerInScene
    {
        get => activePointerInScene;
        set => activePointerInScene = value;
    }

    /// <summary>
    /// Used by a MTKPointer to register itself to the manager
    /// </summary>
    /// <param name="pointer">The pointer which wants to register</param>
    public void AddPointer(MATKPointer pointer)
    {
        pointersInScene.Add(pointer);
    }

    /// <summary>
    /// Used by a MTKPointer to unregister itself from the manager
    /// </summary>
    /// <param name="pointer">The pointer which wants to unregister</param>
    public void RemovePointer(MATKPointer pointer)
    {
        pointersInScene.Remove(pointer);
    }

    #region Unity methods
    private void Update()
    {
        if (activePointerInScene == null)
        {
            if (currentObjectOnFocus != null)
            {
                FreezeGeometries.Instance.GeometryOnLostFocus(currentObjectOnFocus);
                currentObjectOnFocus = null;
                //if (wait == null)
                //{
                //    StartCoroutine(WaitAndReset());
                //}
            }
        }
    }
    #endregion

    #region Coroutines
    private IEnumerator WaitAndReset()
    {
        yield return new WaitForSeconds(0.5f);
        FreezeGeometries.Instance.GeometryOnLostFocus(currentObjectOnFocus);
        currentObjectOnFocus = null;
        wait = null;
    }
    #endregion
}
