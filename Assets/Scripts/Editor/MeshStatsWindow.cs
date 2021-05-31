using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace MegameTestTask.Editor
{
    public class MeshStatsWindow : EditorWindow
    {
        private const int NameFieldWidth = 150;
        private const int VertexCountFieldWidth = 80;
        private const int PolygonCountFieldWidth = 90;
        private const int UsesInSceneFieldWidth = 85;
        private const int VertexSumInSceneFieldWidth = 120;
        private const int ReadableFieldWidth = 60;
        private const int UVLightmapFieldWidth = 80;

        private List<SingleMeshStats> _meshStats;
        private Vector2 scrollPosition;
        
        [MenuItem("Window/Mesh Stats")]
        public static void Init()
        {
            var window = EditorWindow.GetWindow<MeshStatsWindow>();
            window.UpdateData();
            window.Show();
        }

        private void OnGUI()
        {
            UpdateData();
            
            GUILayout.BeginHorizontal();
                GUILayout.Label("Name", GUILayout.Width(NameFieldWidth));
                GUILayout.Label("Vertex count", GUILayout.Width(VertexCountFieldWidth));
                GUILayout.Label("Polygon count", GUILayout.Width(PolygonCountFieldWidth));
                GUILayout.Label("Uses in scene", GUILayout.Width(UsesInSceneFieldWidth));
                GUILayout.Label("Vertex sum in scene", GUILayout.Width(VertexSumInSceneFieldWidth));
                GUILayout.Label("Readable", GUILayout.Width(ReadableFieldWidth));
                GUILayout.Label("UV Lightmap", GUILayout.Width(UVLightmapFieldWidth));
            GUILayout.EndHorizontal();
            
            var meshStats = GetMeshStats();
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);
            foreach (var singleMeshStats in meshStats)
            {
                GUILayout.BeginHorizontal();
                
                    GUILayout.Label(singleMeshStats.Name, GUILayout.Width(NameFieldWidth));
                    GUILayout.Label(singleMeshStats.VertexCount.ToString(), GUILayout.Width(VertexCountFieldWidth));
                    GUILayout.Label(singleMeshStats.PolygonCount.ToString(), GUILayout.Width(PolygonCountFieldWidth));
                    GUILayout.Label(singleMeshStats.UsesInScene.ToString(), GUILayout.Width(UsesInSceneFieldWidth));
                    GUILayout.Label(singleMeshStats.VertexSumInScene.ToString(), GUILayout.Width(VertexSumInSceneFieldWidth));

                    EditorGUI.BeginDisabledGroup(!singleMeshStats.HasImporter);
                        
                        bool isReadablePreviousValue = singleMeshStats.IsReadable;
                        bool isReadableNewValue =
                            GUILayout.Toggle(isReadablePreviousValue, string.Empty, GUILayout.Width(ReadableFieldWidth));
                        if (isReadablePreviousValue != isReadableNewValue)
                        {
                            singleMeshStats.Importer.isReadable = isReadableNewValue;
                            AssetDatabase.ImportAsset(singleMeshStats.Path, ImportAssetOptions.ForceUpdate);
                            UpdateData();
                        }

                        bool uvLightmapPreviousValue = singleMeshStats.UVLightmap;
                        bool uvLightmapNewValue =
                            GUILayout.Toggle(uvLightmapPreviousValue, string.Empty, GUILayout.Width(UVLightmapFieldWidth));
                        if (uvLightmapPreviousValue != uvLightmapNewValue)
                        {
                            singleMeshStats.Importer.generateSecondaryUV = uvLightmapNewValue;
                            AssetDatabase.ImportAsset(singleMeshStats.Path, ImportAssetOptions.ForceUpdate);
                            UpdateData();
                        }
                        
                    EditorGUI.EndDisabledGroup();
                    
                GUILayout.EndHorizontal();
            }
            GUILayout.EndScrollView();
        }

        public void UpdateData()
        {
            _meshStats = GetMeshStats();
        }

        private List<SingleMeshStats> GetMeshStats()
        {
            var meshStats = new List<SingleMeshStats>();
            var meshFilters = FindObjectsOfType<MeshFilter>();
            foreach (var meshFilter in meshFilters)
            {
                var thisMeshStats = meshStats.SingleOrDefault(x => x.SharedMesh == meshFilter.sharedMesh);
                if (thisMeshStats != null)
                {
                    thisMeshStats.UsesInScene++;
                }
                else
                {
                    var newMeshStats = new SingleMeshStats();
                    newMeshStats.SharedMesh = meshFilter.sharedMesh;
                    newMeshStats.Name = newMeshStats.SharedMesh.name;
                    newMeshStats.VertexCount = newMeshStats.SharedMesh.vertexCount;
                    newMeshStats.PolygonCount = newMeshStats.SharedMesh.triangles.Length;
                    newMeshStats.UsesInScene = 1;
                    newMeshStats.IsReadable = newMeshStats.SharedMesh.isReadable;
                    newMeshStats.Path = AssetDatabase.GetAssetPath(newMeshStats.SharedMesh);
                    newMeshStats.Importer = (ModelImporter)ModelImporter.GetAtPath(newMeshStats.Path);
                    newMeshStats.HasImporter = newMeshStats.Importer != null;
                    if (newMeshStats.HasImporter)
                    {
                        newMeshStats.UVLightmap = newMeshStats.Importer.generateSecondaryUV;
                    }
                    meshStats.Add(newMeshStats);
                }
            }

            return meshStats;
        }
        
        private class SingleMeshStats
        {
            public Mesh SharedMesh;
            public string Name;
            public int VertexCount;
            public int PolygonCount;
            public int UsesInScene;
            public bool IsReadable;
            public bool UVLightmap;
            public ModelImporter Importer;
            public string Path;
            public bool HasImporter;
            
            public int VertexSumInScene => VertexCount * UsesInScene;
        }
    }
}