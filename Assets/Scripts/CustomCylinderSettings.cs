using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace MegameTestTask
{
    public class CustomCylinderSettings : MonoBehaviour
    {
        private const int VerticesInCircle = 100;
        
        private float visibleMeshHeight = 3;
        private float visibleMeshRadius = 1;
        private float colliderHeight = 3;
        private float colliderRadius = 1;

        public float VisibleMeshHeight
        {
            get => visibleMeshHeight;
            set
            {
                float previousValue = visibleMeshHeight;
                visibleMeshHeight = value;
                if (visibleMeshHeight != previousValue)
                {
                    GenerateVisibleMesh();
                }
            }
        }
        
        public float VisibleMeshRadius
        {
            get => visibleMeshRadius;
            set
            {
                float previousValue = visibleMeshRadius;
                visibleMeshRadius = value;
                if (visibleMeshRadius != previousValue)
                {
                    GenerateVisibleMesh();
                }
            }
        }
        
        public float ColliderHeight
        {
            get => colliderHeight;
            set
            {
                float previousValue = colliderHeight;
                colliderHeight = value;
                if (colliderHeight != previousValue)
                {
                    GenerateCollider();
                }
            }
        }
        
        public float ColliderRadius
        {
            get => colliderRadius;
            set
            {
                float previousValue = colliderRadius;
                colliderRadius = value;
                if (colliderRadius != previousValue)
                {
                    GenerateCollider();
                }
            }
        }

        private MeshCollider meshCollider;
        private MeshRenderer meshRenderer;
        private MeshFilter meshFilter;

        public void Initialize(MeshCollider mc, MeshRenderer mr, MeshFilter mf, float height, float radius)
        {
            visibleMeshHeight = height;
            colliderHeight = height;
            visibleMeshRadius = radius;
            colliderRadius = radius;
            meshCollider = mc;
            meshRenderer = mr;
            meshFilter = mf;
            GenerateVisibleMesh();
            GenerateCollider();
            meshRenderer.material = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Material.mat");
        }
        
        private void GenerateVisibleMesh()
        {
            meshFilter.sharedMesh = GenerateMesh(VisibleMeshHeight, visibleMeshRadius);
            EditorUtility.SetDirty(gameObject);
        }

        private void GenerateCollider()
        {
            meshCollider.sharedMesh = GenerateMesh(colliderHeight, colliderRadius);
            meshCollider.convex = true;
            EditorUtility.SetDirty(gameObject);
        }
        
        private Mesh GenerateMesh(float height, float radius)
        {
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

            return mesh;
        }
    }
}