using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.TerrainAPI;
using UnityEngine.AI;

[CustomEditor(typeof(NavMeshConverter))]
public class NavMeshConverterEditor : Editor
{
    public override void OnInspectorGUI()
    {
        if (GUILayout.Button("Create Mesh"))
        {
            NavMeshTriangulation triangles = NavMesh.CalculateTriangulation();
            Mesh mesh = new Mesh();
            mesh.vertices = triangles.vertices;
            mesh.triangles = triangles.indices;
            GameObject obj = new GameObject();
            obj.AddComponent<MeshFilter>().mesh = mesh;
            obj.AddComponent<MeshRenderer>();

            MeshUtility.Optimize(mesh);

            AssetDatabase.CreateAsset(mesh, "Assets/Meshes/Osaka/MeshFromNavMesh.asset");
            AssetDatabase.SaveAssets();
        }
    }
}
