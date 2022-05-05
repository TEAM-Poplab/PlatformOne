using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeometryStatusSaver : Singleton<GeometryStatusSaver>
{
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

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

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

    public float GetSliderValue(GameObject obj)
    {
        float returnValue = 0;
        sliderSavedValue.TryGetValue(obj, out returnValue);
        return returnValue == 0 ? 0 : returnValue;
    }

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
    
    public SavedTransform GetTransformValue(GameObject obj)
    {
        SavedTransform returnValue = new SavedTransform(Vector3.zero, Quaternion.identity, Vector3.zero);
        savedTransform.TryGetValue(obj, out returnValue);
        return returnValue;
    }

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

    public Vector3 GetBoxExtends(GameObject obj)
    {
        Vector3 returnValue = Vector3.one;
        savedBoxExtends.TryGetValue(obj, out returnValue);
        return returnValue;
    }

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

    public Vector3 GetMiniSceneScale(GameObject obj)
    {
        Vector3 returnValue = Vector3.one;
        savedMiniSceneScale.TryGetValue(obj, out returnValue);
        return returnValue;
    }

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

    public GameObject GetProjectedGeometry(GameObject dockGeometry)
    {
        GameObject returnValue = new GameObject();
        savedProjectedGeometry.TryGetValue(dockGeometry, out returnValue);
        return returnValue;
    }

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

    public GameObject GetSliderObject(GameObject dockGeometry)
    {
        GameObject returnValue = new GameObject();
        savedSliderObject.TryGetValue(dockGeometry, out returnValue);
        return returnValue;
    }

    public void RemoveSliderObject(GameObject dockGeometry)
    {
        savedSliderObject.Remove(dockGeometry);
    }

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
