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
        private const int VerticesCountFieldWidth = 90;
        private const int PolygonCountFieldWidth = 90;
        private const int UsesInSceneFieldWidth = 85;
        private const int VerticesSumInSceneFieldWidth = 130;
        private const int ReadableFieldWidth = 60;
        private const int UVLightmapFieldWidth = 80;
        private const int FilteringAndSortingLabelWidth = 100;
        private const int FilteringStringFieldWidth = 200;
        private const int FilteringMinMaxLabelWidth = 30;
        private const int FilteringIntFieldWidth = 50;
        private const int ToggleWidth = 15;
        private const int EnumPopupWidth = 160;

        private const int UpdateDataEveryXFrames = 10;

        private static GUIStyle labelLikeButtonStyle;

        private List<SingleMeshStats> _meshStats;
        private Vector2 scrollPosition;

        private bool filterByName;
        private bool filterByVerticesCount;
        private bool filterByPolygonsCount;
        private bool filterByUsesInScene;
        private bool filterByVerticesSumInScene;
        
        private string nameFilter = string.Empty;
        private int minVerticesCount;
        private int maxVerticesCount;
        private int minPolygonsCount;
        private int maxPolygonsCount;
        private int minUsesInScene;
        private int maxUsesInScene;
        private int minVerticesSumInScene;
        private int maxVerticesSumInScene;
        private RequiredBool requiredReadable;
        private RequiredBool requiredUVLightmap;

        private SortingMode sortingMode;
        private SortingOrder sortingOrder;
        
        [MenuItem("Window/Mesh Stats")]
        public static void Init()
        {
            var window = EditorWindow.GetWindow<MeshStatsWindow>("Mesh stats");
            window.UpdateData();
            window.Show();
            
            labelLikeButtonStyle = new GUIStyle();
            var border = labelLikeButtonStyle.border;
            border.left = 0;
            border.top = 0;
            border.right = 0;
            border.bottom = 0;
            labelLikeButtonStyle.normal.textColor = Color.Lerp(Color.white, Color.black, 0.2f);
        }

        private void OnGUI()
        {
            DrawTable();
            EditorGUILayout.Space();
            DrawFiltering();
            EditorGUILayout.Space();
            DrawSorting();
            
            
            void DrawTable()
            { 
                if (Time.renderedFrameCount % UpdateDataEveryXFrames == 0)
                {
                    UpdateData();
                }
                
                GUILayout.BeginHorizontal();
                    GUILayout.Label("Name", EditorStyles.boldLabel, GUILayout.Width(NameFieldWidth));
                    GUILayout.Label("Vertices count", EditorStyles.boldLabel, GUILayout.Width(VerticesCountFieldWidth));
                    GUILayout.Label("Polygons count", EditorStyles.boldLabel, GUILayout.Width(PolygonCountFieldWidth));
                    GUILayout.Label("Uses in scene", EditorStyles.boldLabel, GUILayout.Width(UsesInSceneFieldWidth));
                    GUILayout.Label("Vertices sum in scene", EditorStyles.boldLabel, GUILayout.Width(VerticesSumInSceneFieldWidth));
                    GUILayout.Label("Readable", EditorStyles.boldLabel, GUILayout.Width(ReadableFieldWidth));
                    GUILayout.Label("UV Lightmap", EditorStyles.boldLabel, GUILayout.Width(UVLightmapFieldWidth));
                GUILayout.EndHorizontal();
                
                var meshStats = GetMeshStats();
                FilterMeshStats();
                SortMeshStats();
                scrollPosition = GUILayout.BeginScrollView(scrollPosition);
                foreach (var singleMeshStats in meshStats)
                {
                    GUILayout.BeginHorizontal();
                        bool focus = GUILayout.Button(singleMeshStats.Name, labelLikeButtonStyle, GUILayout.Width(NameFieldWidth));
                        if (focus)
                        {
                            Selection.activeGameObject = FindObjectsOfType<MeshFilter>()
                                .First(x => x.sharedMesh == singleMeshStats.SharedMesh)
                                .gameObject;
                            SceneView.FrameLastActiveSceneView();
                        }
                        GUILayout.Label(singleMeshStats.VerticesCount.ToString(), GUILayout.Width(VerticesCountFieldWidth));
                        GUILayout.Label(singleMeshStats.PolygonsCount.ToString(), GUILayout.Width(PolygonCountFieldWidth));
                        GUILayout.Label(singleMeshStats.UsesInScene.ToString(), GUILayout.Width(UsesInSceneFieldWidth));
                        GUILayout.Label(singleMeshStats.VerticesSumInScene.ToString(), GUILayout.Width(VerticesSumInSceneFieldWidth));

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


                void FilterMeshStats()
                {
                    if (filterByName)
                        meshStats = meshStats
                            .Where(x => x.Name.ToLower().Contains(nameFilter.ToLower()))
                            .ToList();
                    if (filterByVerticesCount)
                    {
                        meshStats = meshStats
                            .Where(x => x.VerticesCount >= minVerticesCount)
                            .Where(x => x.VerticesCount <= maxVerticesCount)
                            .ToList();
                    }
                    if (filterByPolygonsCount)
                    {
                        meshStats = meshStats
                            .Where(x => x.PolygonsCount >= minPolygonsCount)
                            .Where(x => x.PolygonsCount <= maxPolygonsCount)
                            .ToList();
                    }
                    if (filterByUsesInScene)
                    {
                        meshStats = meshStats
                            .Where(x => x.UsesInScene >= minUsesInScene)
                            .Where(x => x.UsesInScene <= maxUsesInScene)
                            .ToList();
                    }
                    if (filterByVerticesSumInScene)
                    {
                        meshStats = meshStats
                            .Where(x => x.VerticesSumInScene >= minVerticesSumInScene)
                            .Where(x => x.VerticesSumInScene <= maxVerticesSumInScene)
                            .ToList();
                    }
                    if (requiredReadable != RequiredBool.Any)
                    {
                        bool requiredValue = requiredReadable == RequiredBool.True ? true : false;
                        meshStats = meshStats
                            .Where(x => x.IsReadable == requiredValue)
                            .ToList();
                    }
                    if (requiredUVLightmap != RequiredBool.Any)
                    {
                        bool requiredValue = requiredUVLightmap == RequiredBool.True ? true : false;
                        meshStats = meshStats
                            .Where(x => x.UVLightmap == requiredValue)
                            .ToList();
                    }
                }

                void SortMeshStats()
                {
                    switch (sortingMode)
                    {
                        case SortingMode.Name:
                            meshStats.Sort((x, y) => string.Compare(x.Name, y.Name));
                            break;
                        case SortingMode.VerticesCount:
                            meshStats.Sort((x, y) => x.VerticesCount.CompareTo(y.VerticesCount));
                            break;
                        case SortingMode.PolygonsCount:
                            meshStats.Sort((x, y) => x.PolygonsCount.CompareTo(y.PolygonsCount));
                            break;
                        case SortingMode.UsesInScene:
                            meshStats.Sort((x, y) => x.UsesInScene.CompareTo(y.UsesInScene));
                            break;
                        case SortingMode.VerticesSumInScene:
                            meshStats.Sort((x, y) => x.VerticesSumInScene.CompareTo(y.VerticesSumInScene));
                            break;
                        case SortingMode.Readable:
                            meshStats.Sort((x, y) => x.IsReadable.CompareTo(y.IsReadable));
                            break;
                        case SortingMode.UVLightmap:
                            meshStats.Sort((x, y) => x.UVLightmap.CompareTo(y.UVLightmap));
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    if (sortingOrder == SortingOrder.Descending)
                        meshStats.Reverse();
                }
            }

            void DrawFiltering()
            {
                EditorGUILayout.LabelField("Filtering", EditorStyles.boldLabel);
                EditorGUILayout.BeginHorizontal();
                    filterByName = EditorGUILayout.Toggle(filterByName, GUILayout.Width(ToggleWidth));
                    EditorGUILayout.LabelField("Name", GUILayout.Width(FilteringAndSortingLabelWidth));
                    EditorGUI.BeginDisabledGroup(!filterByName);
                        nameFilter = EditorGUILayout.TextField(nameFilter, GUILayout.Width(FilteringStringFieldWidth));
                    EditorGUI.EndDisabledGroup();
                EditorGUILayout.EndHorizontal();
                DrawMinMaxRange("Vertices count", ref minVerticesCount, ref maxVerticesCount, ref filterByVerticesCount);
                DrawMinMaxRange("Polygons count", ref minPolygonsCount, ref maxPolygonsCount, ref filterByPolygonsCount);
                DrawMinMaxRange("Uses in scene", ref minUsesInScene, ref maxUsesInScene, ref filterByUsesInScene);
                DrawMinMaxRange("Vertices sum in scene", ref minVerticesSumInScene, ref maxVerticesSumInScene, ref filterByVerticesSumInScene);
                DrawRequiredBoolLine("Readable", ref requiredReadable);
                DrawRequiredBoolLine("UV lightmap", ref requiredUVLightmap);


                void DrawMinMaxRange(string label, ref int min, ref int max, ref bool isFilteringActive)
                {
                    EditorGUILayout.BeginHorizontal();
                        isFilteringActive = EditorGUILayout.Toggle(isFilteringActive, GUILayout.Width(ToggleWidth));
                        EditorGUILayout.LabelField(label, GUILayout.Width(FilteringAndSortingLabelWidth));
                        EditorGUI.BeginDisabledGroup(!isFilteringActive);
                            EditorGUILayout.LabelField("Min", GUILayout.Width(FilteringMinMaxLabelWidth));
                            min = EditorGUILayout.IntField(min, GUILayout.Width(FilteringIntFieldWidth));
                            EditorGUILayout.LabelField("Max", GUILayout.Width(FilteringMinMaxLabelWidth));
                            max = EditorGUILayout.IntField(max, GUILayout.Width(FilteringIntFieldWidth));
                        EditorGUI.EndDisabledGroup();
                    EditorGUILayout.EndHorizontal();
                }

                void DrawRequiredBoolLine(string label, ref RequiredBool requiredBool)
                {
                    EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField(label, GUILayout.Width(FilteringAndSortingLabelWidth));
                        requiredBool = (RequiredBool)EditorGUILayout.EnumPopup(requiredBool, GUILayout.Width(EnumPopupWidth));
                    EditorGUILayout.EndHorizontal();
                }
            }

            void DrawSorting()
            {
                EditorGUILayout.LabelField("Sorting", EditorStyles.boldLabel);
                EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Mode", GUILayout.Width(FilteringAndSortingLabelWidth));
                    sortingMode = (SortingMode) EditorGUILayout.EnumPopup(sortingMode, GUILayout.Width(EnumPopupWidth));
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Order", GUILayout.Width(FilteringAndSortingLabelWidth));
                    sortingOrder = (SortingOrder) EditorGUILayout.EnumPopup(sortingOrder, GUILayout.Width(EnumPopupWidth));
                EditorGUILayout.EndHorizontal();
            }
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
                    newMeshStats.VerticesCount = newMeshStats.SharedMesh.vertexCount;
                    newMeshStats.PolygonsCount = newMeshStats.SharedMesh.triangles.Length / 3;
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
            public int VerticesCount;
            public int PolygonsCount;
            public int UsesInScene;
            public bool IsReadable;
            public bool UVLightmap;
            public ModelImporter Importer;
            public string Path;
            public bool HasImporter;
            
            public int VerticesSumInScene => VerticesCount * UsesInScene;
        }
        
        private enum RequiredBool
        {
            Any = 0,
            True = 1,
            False = 2
        }
        
        private enum SortingMode
        {
            Name,
            VerticesCount,
            PolygonsCount,
            UsesInScene,
            VerticesSumInScene,
            Readable,
            UVLightmap
        }
        
        private enum SortingOrder
        {
            Ascending,
            Descending
        }
    }
}