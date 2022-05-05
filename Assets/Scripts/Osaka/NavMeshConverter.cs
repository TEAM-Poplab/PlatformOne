using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;


public class NavMeshConverter : MonoBehaviour
{
    public void CreateNavMeshGameObject()
    {
        NavMeshTriangulation triangles = NavMesh.CalculateTriangulation();
        Mesh mesh = new Mesh();
        mesh.vertices = triangles.vertices;
        mesh.triangles = triangles.indices;
        GameObject obj = new GameObject();
        obj.AddComponent<MeshFilter>().mesh = mesh;
        obj.AddComponent<MeshRenderer>();
    }
}
