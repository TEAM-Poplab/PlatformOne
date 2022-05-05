using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class SaveMesh : MonoBehaviour
{
    public ParticleSystem pathParticleSystem;

    // Start is called before the first frame update
    void Start()
    {
        var obj = new GameObject("SavedMesh");
        obj.AddComponent<MeshFilter>();
        Mesh newMesh = new Mesh();
        newMesh.name = "Path";
        GetComponent<LineRenderer>().BakeMesh(newMesh);
        obj.GetComponent<MeshFilter>().mesh = newMesh;
        obj.transform.parent = transform;
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localRotation = Quaternion.identity;
        var shapeModule = pathParticleSystem.shape;
        shapeModule.shapeType = ParticleSystemShapeType.Mesh;
        shapeModule.mesh = newMesh;
        pathParticleSystem.Play();
        //obj.AddComponent<MeshRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
