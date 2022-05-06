using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Utils class which allows the saving of the geometry status for whatever use
/// </summary>
public class GeometryStatusSaver : Singleton<GeometryStatusSaver>
{
    /// <summary>
    /// Struct to save a copy of the Transfrom values (Transform are handled by reference, but we need copies)
    /// </summary>
    public struct SavedTransform
    {
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;

        public SavedTransform(Vector3 pos, Quaternion rot, Vector3 scale)
        {
            position = pos;
            rotation = rot;
            this.scale = scale;
        }
    }

    //First argument for any dictionary MUST BE the geometry origin center, NOT the geometry centroid!
    Dictionary<GameObject, float> sliderSavedValue = new Dictionary<GameObject, float>();
    Dictionary<GameObject, SavedTransform> savedTransform = new Dictionary<GameObject, SavedTransform>();
    Dictionary<GameObject, Vector3> savedBoxExtends = new Dictionary<GameObject, Vector3>();
    Dictionary<GameObject, Vector3> savedMiniSceneScale = new Dictionary<GameObject, Vector3>();
    Dictionary<GameObject, GameObject> savedProjectedGeometry = new Dictionary<GameObject, GameObject>();
    Dictionary<GameObject, GameObject> savedSliderObject = new Dictionary<GameObject, GameObject>();

    /// <summary>
    /// Saves the values of the slider
    /// </summary>
    /// <param name="obj">The object whose slider value is going to be saved</param>
    /// <param name="val">The slider value to save</param>
    public void SaveSliderValue(GameObject obj, int val)
    {
        if(!sliderSavedValue.ContainsKey(obj))
        {
            sliderSavedValue.Add(obj, val);
        } else
        {
            sliderSavedValue[obj] = val;
        }

    }

    /// <summary>
    /// Retrieve the currently saved slider value
    /// </summary>
    /// <param name="obj">The object which we need to retrieve the slider value for</param>
    /// <returns>The saved slider value</returns>
    public float GetSliderValue(GameObject obj)
    {
        float returnValue = 0;
        sliderSavedValue.TryGetValue(obj, out returnValue);
        return returnValue == 0 ? 0 : returnValue;
    }

    /// <summary>
    /// Saves the Transform values of the object
    /// </summary>
    /// <param name="obj">The object whose Transform value is going to be saved</param>
    /// <param name="pos">The position to save</param>
    /// <param name="rot">The rotation (Quaternion) to save</param>
    /// <param name="scale">The scale to save</param>
    public void SaveTransformValue(GameObject obj, Vector3 pos, Quaternion rot, Vector3 scale)
    {
        if(!savedTransform.ContainsKey(obj))
        {
            savedTransform.Add(obj, new SavedTransform(pos, rot, scale));
        } else
        {
            savedTransform[obj] = new SavedTransform(pos, rot, scale);
        }

    }

    /// <summary>
    /// Retrieve the currently saved Transform value
    /// </summary>
    /// <param name="obj">The object which we need to retrieve the Transform values for</param>
    /// <returns>The <c>SavedTransform</c> structure which holds the saved Transform values</returns>
    public SavedTransform GetTransformValue(GameObject obj)
    {
        SavedTransform returnValue = new SavedTransform(Vector3.zero, Quaternion.identity, Vector3.zero);
        savedTransform.TryGetValue(obj, out returnValue);
        return returnValue;
    }

    /// <summary>
    /// Saves the BoxExtends values of the object
    /// </summary>
    /// <param name="obj">The object whose Transform value is going to be saved</param>
    /// <param name="ext">The Bxtends to save</param>
    public void SaveBoxExtends(GameObject obj, Vector3 ext)
    {
        if (!savedBoxExtends.ContainsKey(obj))
        {
            savedBoxExtends.Add(obj, ext);
        }
        else
        {
            savedBoxExtends[obj] = ext;
        }
    }

    /// <summary>
    /// Retrieve the currently saved BoxExtends values
    /// </summary>
    /// <param name="obj">The object which we need to retrieve the BoxExtends values for</param>
    /// <returns>The saved BoxExtends values</returns>
    public Vector3 GetBoxExtends(GameObject obj)
    {
        Vector3 returnValue = Vector3.one;
        savedBoxExtends.TryGetValue(obj, out returnValue);
        return returnValue;
    }

    /// <summary>
    /// Saves the current scale of the mini scene (the scene on the platform dock)
    /// </summary>
    /// <param name="obj">The object in the mini scene</param>
    /// <param name="scale">The mini scene scale value to save</param>
    public void SaveMiniSceneScale(GameObject obj, Vector3 scale)
    {
        if (!savedMiniSceneScale.ContainsKey(obj))
        {
            savedMiniSceneScale.Add(obj, scale);
        }
        else
        {
            savedMiniSceneScale[obj] = scale;
        }
    }


    /// <summary>
    /// Retrieve the mini scene scale for a givent object (the object which was in that mene scene)
    /// </summary>
    /// <param name="obj">The object which we need to get the mini scene scale for</param>
    /// <returns>The mini scene scale</returns>
    public Vector3 GetMiniSceneScale(GameObject obj)
    {
        Vector3 returnValue = Vector3.one;
        savedMiniSceneScale.TryGetValue(obj, out returnValue);
        return returnValue;
    }

    /// <summary>
    /// Saves the projected geometry of the current platform dock geometry
    /// </summary>
    /// <param name="dockGeometry">The docked geometry whose projected geometry needs to be saved</param>
    /// <param name="projectedGeometry">The projected geometry to save</param>
    public void SaveProjectedGeometry(GameObject dockGeometry, GameObject projectedGeometry)
    {
        if (!savedProjectedGeometry.ContainsKey(dockGeometry))
        {
            savedProjectedGeometry.Add(dockGeometry, projectedGeometry);
        } else
        {
            savedProjectedGeometry[dockGeometry] = projectedGeometry;
        }
    }

    /// <summary>
    /// Retrieve the projected geometry associated to the given object
    /// </summary>
    /// <param name="dockGeometry">The object which we need to get the projected geometry for</param>
    /// <returns>The reference to the projected geometry</returns>
    public GameObject GetProjectedGeometry(GameObject dockGeometry)
    {
        GameObject returnValue = new GameObject();
        savedProjectedGeometry.TryGetValue(dockGeometry, out returnValue);
        return returnValue;
    }

    /// <summary>
    /// Saves the slider related to the object
    /// </summary>
    /// <param name="dockGeometry">The object whose slider is going to be saved</param>
    /// <param name="slider">The slider to save</param>
    public void SaveSliderObject(GameObject dockGeometry, GameObject slider)
    {
        if (!savedSliderObject.ContainsKey(dockGeometry))
        {
            savedSliderObject.Add(dockGeometry, slider);
        }
        else
        {
            savedSliderObject[dockGeometry] = slider;
        }
    }

    /// <summary>
    /// Retrieve the saved slider for an object
    /// </summary>
    /// <param name="dockGeometry">The object which we need to retrieve the slider for</param>
    /// <returns>The saved slider</returns>
    public GameObject GetSliderObject(GameObject dockGeometry)
    {
        GameObject returnValue = new GameObject();
        savedSliderObject.TryGetValue(dockGeometry, out returnValue);
        return returnValue;
    }

    /// <summary>
    /// Remove the slider object for a given geometry
    /// </summary>
    /// <param name="dockGeometry">The geometry whose slider we want to delete</param>
    public void RemoveSliderObject(GameObject dockGeometry)
    {
        savedSliderObject.Remove(dockGeometry);
    }

    /// <summary>
    /// Removes all saved values and reference for a givend geoemtry
    /// </summary>
    /// <param name="obj">The geometry whose saved values we want to delete</param>
    public void RemoveAllObjectValues(GameObject obj)
    {
        if (savedTransform.ContainsKey(obj))
        {
            savedTransform.Remove(obj);
        }
        if (savedBoxExtends.ContainsKey(obj))
        {
            savedBoxExtends.Remove(obj);
        }
        if (sliderSavedValue.ContainsKey(obj))
        {
            sliderSavedValue.Remove(obj);
        }
        if (savedMiniSceneScale.ContainsKey(obj))
        {
            savedMiniSceneScale.Remove(obj);
        }
        if(savedProjectedGeometry.ContainsKey(obj))
        {
            savedProjectedGeometry.Remove(obj);
        }
    }
}
