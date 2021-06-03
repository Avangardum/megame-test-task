using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;

namespace MegameTestTask.Editor
{
    public class CreateCustomCylinderWindow : EditorWindow
    {
        private const int VerticesInCircle = 100;
        
        private float height;
        private float radius;
        
        [MenuItem(itemName: "GameObject/Custom Cylinder", isValidateFunction: false, priority: 20)]
        public static void Init()
        {
            var window = EditorWindow.GetWindow<CreateCustomCylinderWindow>("Create custom cylinder");
            window.Show();
        }

        private void OnGUI()
        {
            height = EditorGUILayout.FloatField("Height", height);
            radius = EditorGUILayout.FloatField("Radius", radius);
            bool create = GUILayout.Button("Create");
            if (create)
            {
                CreateCylinder();
            }
        }

        private GameObject CreateCylinder()
        {
            GameObject cylinder = new GameObject("Cylinder");
            var meshFilter = cylinder.AddComponent<MeshFilter>();
            var meshRenderer = cylinder.AddComponent<MeshRenderer>();
            var meshCollider = cylinder.AddComponent<MeshCollider>();

            Mesh mesh = new Mesh();
            float angleStep = 2 * Mathf.PI / VerticesInCircle;
            
            Vector3[] bottomCircle = new Vector3[VerticesInCircle];
            for (int i = 0; i < VerticesInCircle; i++)
            {
                float angle = angleStep * i;
                bottomCircle[i] = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * radius;
            }
            Vector3 bottomCircleCenter = Vector3.zero;

            Vector3[] topCircle = bottomCircle.Select(x => x + Vector3.up * height).ToArray();
            Vector3 topCircleCenter = Vector3.up * height;

            List<Vector3> vertices = new List<Vector3>();
            vertices.AddRange(bottomCircle);
            vertices.AddRange(topCircle);
            vertices.Add(bottomCircleCenter);
            vertices.Add(topCircleCenter);
            mesh.vertices = vertices.ToArray();

            
            List<int> triangles = new List<int>();

            int bottomCircleCenterIndex = VerticesInCircle * 2;
            int topCircleCenterIndex = bottomCircleCenterIndex + 1;
            
            // Draw the top circle
            for (int i = 0; i < VerticesInCircle; i++)
            {
                var a = i;
                var b = i == 0 ? VerticesInCircle - 1 : i - 1;
                var c = bottomCircleCenterIndex;
                triangles.AddRange(new [] {a, b, c, c, b, a});
            }
            
            // Draw the bottom circle
            for (int i = VerticesInCircle; i < VerticesInCircle * 2; i++)
            {
                var a = i;
                var b = i == VerticesInCircle ? VerticesInCircle * 2 - 1 : i - 1;
                var c = topCircleCenterIndex;
                triangles.AddRange(new [] {a, b, c, c, b, a});
            }
            
            // Draw sides
            for (int i = 0; i < VerticesInCircle; i++)
            {
                /*
                 * d---c
                 * |  /|
                 * | / |
                 * |/  |
                 * b---a
                 */
                var a = i;
                var b = i == 0 ? VerticesInCircle - 1 : i - 1;
                var c = i + VerticesInCircle;
                var d = c == VerticesInCircle ? VerticesInCircle * 2 - 1 : c - 1;
                triangles.AddRange(new [] {a, b, c, c, b, a});
                triangles.AddRange(new [] {b, d, c, c, d, b});
            }

            mesh.triangles = triangles.ToArray();


            meshFilter.mesh = mesh;
            meshRenderer.material = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Material.mat");
            meshCollider.sharedMesh = mesh;
            meshCollider.convex = true;
            EditorUtility.SetDirty(cylinder);
            return cylinder;
        }
    }
}