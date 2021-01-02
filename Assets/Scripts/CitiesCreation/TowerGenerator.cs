using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ProceduralCities.CitiesCreation
{
    [RequireComponent(typeof(MeshRenderer),typeof(MeshFilter))]
    public class TowerGenerator : MonoBehaviour
    {
        private MeshRenderer _meshRenderer;
        private MeshFilter _meshFilter;
        //private BoxCollider box;
        private Mesh mesh;
        private Material _material;
        [SerializeField,ReadOnly]
        private Vector3 _size;

        private static readonly int BaseMapSt = Shader.PropertyToID("_BaseMap_ST");

        //TODO: generate myself the mesh
        public void Initialize(Vector3 size, Material material, float materialDivider = 1f)
        {
            _meshRenderer = GetComponent<MeshRenderer>();
            _meshFilter = GetComponent<MeshFilter>();
            _size = size;
            //transform.localScale = new Vector3(size.x, height, size.y);
            _material = new Material(material);
            _meshRenderer.material = _material;
            mesh = new Mesh();
            _meshFilter.mesh = mesh;
            
            CreateCube(size,materialDivider);
            //box = gameObject.AddComponent<BoxCollider>();
        }
        
        private void CreateCube (Vector3 size, float matDiv)
        {
            float xmax = 2f * size.y + 2f * size.x;
            float ymax = 2f * size.y + size.z;
            
            Vector3[] vertices = {
                new Vector3(-size.x/2f, size.y, -size.z/2f),
                new Vector3(-size.x/2f, 0, -size.z/2f),
                new Vector3(size.x/2f, size.y, -size.z/2f),
                new Vector3(size.x/2f, 0, -size.z/2f),

                new Vector3(-size.x/2f, 0, size.z/2f),
                new Vector3(size.x/2f, 0, size.z/2f),
                new Vector3(-size.x/2f, size.y, size.z/2f),
                new Vector3(size.x/2f, size.y, size.z/2f),

                new Vector3(-size.x/2f, size.y, -size.z/2f),
                new Vector3(size.x/2f, size.y, -size.z/2f),

                new Vector3(-size.x/2f, size.y, -size.z/2f),
                new Vector3(-size.x/2f, size.y, size.z/2f),

                new Vector3(size.x/2f, size.y, -size.z/2f),
                new Vector3(size.x/2f, size.y, size.z/2f),
            };

            int[] triangles = {
                0, 2, 1, // front
                1, 2, 3,
                4, 5, 6, // back
                5, 7, 6,
                6, 7, 8, //top
                7, 9 ,8, 
                1, 3, 4, //bottom
                3, 5, 4,
                1, 11,10,// left
                1, 4, 11,
                3, 12, 5,//right
                5, 12, 13
            };
            
            Vector2[] uvs = {
                new Vector2(0, (size.y+size.z)/ymax),
                new Vector2((size.y)/xmax, (size.y+size.z)/ymax),
                new Vector2(0, (size.y)/ymax),
                new Vector2((size.y)/xmax, (size.y)/ymax),

                new Vector2((size.y+size.x)/xmax, (size.y+size.z)/ymax),
                new Vector2((size.y+size.x)/xmax, (size.y)/ymax),
                new Vector2((size.y+size.x+size.y)/xmax, (size.y+size.z)/ymax),
                new Vector2((size.y+size.x+size.y)/xmax, (size.y)/ymax),

                new Vector2(1, (size.y+size.z)/ymax),
                new Vector2(1, (size.y)/ymax),

                new Vector2((size.y)/xmax, 1),
                new Vector2((size.y+size.x)/xmax, 1),

                new Vector2((size.y)/xmax, 0),
                new Vector2((size.y+size.x)/xmax, 0),
            };

            mesh.Clear ();
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.uv = uvs;
            mesh.Optimize ();
            mesh.RecalculateNormals ();
            
            _material.SetVector(BaseMapSt, new Vector4(xmax/matDiv, ymax/matDiv, 0, 0));
        }

        // [Button(ButtonSizes.Medium)]
        // public void CheckCollision()
        // {
        //     var colliders = Physics.OverlapBox(box.center, box.size/2f);
        //     foreach (var col in colliders)
        //     {
        //         if (col.name.Equals("Quad") || col.name.Equals(name) || col == null) continue;
        //         
        //         Debug.Log(name + " collide with " + col.name);
        //         //DestroyImmediate(col.gameObject);
        //     }
        //
        //     DestroyCollider();
        // }
        //
        // public void DestroyCollider()
        // {
        //     //Destroy(box);
        // }
    }
}
