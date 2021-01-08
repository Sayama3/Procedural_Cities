using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ProceduralCities.CitiesCreation
{
    public class TowerGenerator : MonoBehaviour
    {
        [SerializeField,ReadOnly]
        private Material _material;
        [SerializeField,ReadOnly]
        private Vector3 _size;

        //private BoxCollider box;

        [ShowInInspector, ReadOnly]
        private float ChildCount
        {
            get
            {
                int totalCount = 0;
                for (int i = 0; i < transform.childCount; i++)
                    totalCount += transform.GetChild(i).childCount;
                return totalCount;
            }
        }

        private static readonly int BaseMapSt = Shader.PropertyToID("_BaseMap_ST");

        //TODO: generate myself the mesh
        [ShowIf("@ChildCount == 0")]
        [Button(ButtonSizes.Medium,ButtonStyle.FoldoutButton)]
        public void Initialize(Vector3 size, Material material, float materialDivider = 1f)
        {
            _size = size;
            _material = new Material(material);
            CreateCube(size,materialDivider);
            
            // Creation of the collider if needed
            // box = gameObject.AddComponent<BoxCollider>();
            // box.center = new Vector3(0, size.y / 2f);
            // box.size = size;
        }
        
        private void CreateCube (Vector3 size, float matDiv)
        {
            //Drawing a cube this way
            //    .h------g
            //  .' |    .'|
            // e---+--f'  |
            // |   |  |   |
            // |  ,d--+---c
            // |.'  o | .' 
            // a------b'    

            Vector3 pos = transform.position;
            
            Vector3 a = pos + new Vector3(-size.x / 2f, 0, -size.z / 2f);
            Vector3 b = pos + new Vector3(size.x / 2f, 0, -size.z / 2f);
            Vector3 c = pos + new Vector3(size.x / 2f, 0, size.z / 2f);
            Vector3 d = pos + new Vector3(-size.x / 2f, 0, size.z / 2f);
            
            Vector3 e = pos + new Vector3(-size.x / 2f, size.y, -size.z / 2f);
            Vector3 f = pos + new Vector3(size.x / 2f, size.y, -size.z / 2f);
            Vector3 g = pos + new Vector3(size.x / 2f, size.y, size.z / 2f);
            Vector3 h = pos + new Vector3(-size.x / 2f, size.y, size.z / 2f);
            
            float xmax = 2f * size.y + 2f * size.x;
            float ymax = 2f * size.y + size.z;
            
            // Vector3[] vertices = {
            //     new Vector3(-size.x/2f, size.y, -size.z/2f),
            //     new Vector3(-size.x/2f, 0, -size.z/2f),
            //     new Vector3(size.x/2f, size.y, -size.z/2f),
            //     new Vector3(size.x/2f, 0, -size.z/2f),
            //
            //     new Vector3(-size.x/2f, 0, size.z/2f),
            //     new Vector3(size.x/2f, 0, size.z/2f),
            //     new Vector3(-size.x/2f, size.y, size.z/2f),
            //     new Vector3(size.x/2f, size.y, size.z/2f),
            //
            //     new Vector3(-size.x/2f, size.y, -size.z/2f),
            //     new Vector3(size.x/2f, size.y, -size.z/2f),
            //
            //     new Vector3(-size.x/2f, size.y, -size.z/2f),
            //     new Vector3(-size.x/2f, size.y, size.z/2f),
            //
            //     new Vector3(size.x/2f, size.y, -size.z/2f),
            //     new Vector3(size.x/2f, size.y, size.z/2f),
            // };
            //
            // int[] triangles = {
            //     0, 2, 1, // front
            //     1, 2, 3,
            //     4, 5, 6, // back
            //     5, 7, 6,
            //     6, 7, 8, //top
            //     7, 9 ,8, 
            //     1, 3, 4, //bottom
            //     3, 5, 4,
            //     1, 11,10,// left
            //     1, 4, 11,
            //     3, 12, 5,//right
            //     5, 12, 13
            // };
            //
            // Vector2[] uvs = {
            //     new Vector2(0, (size.y+size.z)/ymax),
            //     new Vector2((size.y)/xmax, (size.y+size.z)/ymax),
            //     new Vector2(0, (size.y)/ymax),
            //     new Vector2((size.y)/xmax, (size.y)/ymax),
            //
            //     new Vector2((size.y+size.x)/xmax, (size.y+size.z)/ymax),
            //     new Vector2((size.y+size.x)/xmax, (size.y)/ymax),
            //     new Vector2((size.y+size.x+size.y)/xmax, (size.y+size.z)/ymax),
            //     new Vector2((size.y+size.x+size.y)/xmax, (size.y)/ymax),
            //
            //     new Vector2(1, (size.y+size.z)/ymax),
            //     new Vector2(1, (size.y)/ymax),
            //
            //     new Vector2((size.y)/xmax, 1),
            //     new Vector2((size.y+size.x)/xmax, 1),
            //
            //     new Vector2((size.y)/xmax, 0),
            //     new Vector2((size.y+size.x)/xmax, 0),
            // };
            //
            // mesh.Clear ();
            // mesh.vertices = vertices;
            // mesh.triangles = triangles;
            // mesh.uv = uvs;
            // mesh.Optimize ();
            // mesh.RecalculateNormals ();
            CreateFace(a, f, Face.Back);
            CreateFace(d, g, Face.Forward);
            
            CreateFace(a,h,Face.Left);
            CreateFace(b,g,Face.Right);
            
            CreateFace(e,g,Face.Up);
            CreateFace(a,c,Face.Down);
            
            _material.SetVector(BaseMapSt, new Vector4(xmax/matDiv, ymax/matDiv, 0, 0));
        }

        private void CreateFace(Vector3 a, Vector3 b, Face face)
        {
            #region Setup

            Vector2 _a;
            Vector2 _b;
            float missingAxis;
            Vector3 normal;
            
            if(face == Face.Up || face ==  Face.Down)
            { 
                //removing y
                _a = new Vector2(a.x, a.z);
                _b = new Vector2(b.x, b.z);
                missingAxis = a.y;
                normal = face == Face.Up ? Vector3.up : Vector3.down;
            }
            else if(face == Face.Right || face ==  Face.Left)
            { 
                //removing x
                _a = new Vector2(a.z, a.y);
                _b = new Vector2(b.z, b.y);
                missingAxis = a.x;
                normal = face == Face.Right ? Vector3.right : Vector3.left;
            }
            else if(face == Face.Forward || face ==  Face.Back)
            {
                //removing z
                _a = new Vector2(a.x, a.y);
                _b = new Vector2(b.x, b.y);
                missingAxis = a.z;
                normal = face == Face.Forward ? Vector3.forward : Vector3.back;
            }
            else
                throw new ArgumentOutOfRangeException(nameof(face), face, null);

            #endregion

            GameObject quadTemp = GameObject.CreatePrimitive(PrimitiveType.Quad);
            DestroyImmediate(quadTemp.GetComponent<Collider>(),false);
            Transform parent;
            (parent = new GameObject(face.ToString()).transform).SetParent(transform);

            for (float y = _a.y; y < _b.y; y++)
            {
                for (float x = _a.x; x < _b.x; x++)
                {
                    Vector3 position;
                    Quaternion rotation;
                    var realX = x + .5f;
                    var realY = y + .5f;
                    if (face == Face.Up || face == Face.Down)
                    {
                        position = new Vector3(realX, missingAxis, realY);
                        rotation = Quaternion.LookRotation(-normal, face == Face.Up ? Vector3.forward : Vector3.back);
                    }
                    else if (face == Face.Right || face == Face.Left)
                    {
                        position = new Vector3(missingAxis, realY, realX);
                        rotation = Quaternion.LookRotation(-normal, Vector3.up);
                    }
                    else if (face == Face.Forward || face == Face.Back)
                    {
                        position = new Vector3(realX, realY, missingAxis);
                        rotation = Quaternion.LookRotation(-normal, Vector3.up);
                    }
                    else
                        throw new ArgumentOutOfRangeException(nameof(face), face, null);

                    var quad = Instantiate(quadTemp,position,rotation,parent);
                    quad.name = "Quad - " + face + " - ( " + x.ToString("F1") +" : " + y.ToString("F1") +" )";
                    
                    //TODO: set the material depending on the procedural generation
                    Material tempMat = new Material(_material);
                    quad.GetComponent<MeshRenderer>().material = tempMat;
                    tempMat.color = Random.ColorHSV();
                }
            }

            DestroyImmediate(quadTemp, false);
        }

        [Button(ButtonSizes.Medium, ButtonStyle.FoldoutButton)]
        private void HardReset()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(transform.GetChild(i).gameObject);
            }
            _size = Vector3.zero;
            _material = null;

            if (TryGetComponent(out Collider col))
                DestroyImmediate(col);
        }

        public enum Face
        {
            Up,
            Forward,
            Right,
            Down,
            Back,
            Left,
        }
    }
}
