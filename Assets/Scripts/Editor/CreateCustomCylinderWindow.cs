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
        private CustomCylinderSettings preview;
        
        private float height = 3;
        private float radius = 1;
        
        [MenuItem(itemName: "GameObject/Custom Cylinder", isValidateFunction: false, priority: 20)]
        public static void Init()
        {
            var window = EditorWindow.GetWindow<CreateCustomCylinderWindow>("Create custom cylinder");
            window.Show();
            window.OnCreate();
        }

        public void OnCreate()
        {
            preview = CreateCylinder().GetComponent<CustomCylinderSettings>();
            preview.gameObject.name = "Custom cylinder preview";
        }
        
        private void OnGUI()
        {
            height = EditorGUILayout.FloatField("Height", height);
            radius = EditorGUILayout.FloatField("Radius", radius);
            preview.VisibleMeshHeight = height;
            preview.VisibleMeshRadius = radius;
            preview.ColliderHeight = height;
            preview.ColliderRadius = radius;
            bool create = GUILayout.Button("Create");
            if (create)
            {
                CreateCylinder();
            }
        }

        private void OnDestroy()
        {
            DestroyImmediate(preview.gameObject);
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