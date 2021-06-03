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
            var settings = cylinder.AddComponent<CustomCylinderSettings>();

            settings.Initialize(meshCollider, meshRenderer, meshFilter, height, radius);
            
            return cylinder;
        }
    }
}