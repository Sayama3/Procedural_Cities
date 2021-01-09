using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ProceduralCities.CitiesCreation
{
    public class TowerGenerator : MonoBehaviour
    {
        [SerializeField] 
        private Mesh _mesh;
        [SerializeField,ReadOnly]
        private Vector3 _size;

        [SerializeField, ReadOnly] 
        private TowerMaterialsAssembly towerMaterialsAssembly;
        GameObject quadTemp;

        private BoxCollider box;
        private GPULoader _gpuLoader;

        [ShowInInspector, ReadOnly] private float ChildCount => transform.childCount;

        private static readonly int BaseMapSt = Shader.PropertyToID("_BaseMap_ST");

        //TODO: generate myself the mesh
        [ShowIf("@ChildCount == 0")]
        [Button(ButtonSizes.Medium,ButtonStyle.FoldoutButton)]
        public void Initialize(Vector3 size,Mesh mesh, TowerMaterialsAssembly materials, float materialDivider = 1f, bool createCollider = false)
        {
            _size = size;
            _mesh = mesh;
            towerMaterialsAssembly = materials;
            _gpuLoader = GPULoader.Instance;
            
            CreateCube(size,materialDivider);
            if(createCollider)
            {
                //Creation of the collider if needed
                box = gameObject.AddComponent<BoxCollider>();
                box.center = new Vector3(0, size.y / 2f);
                box.size = size;
            }
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

            var faces = new Face[] {Face.Back, Face.Forward, Face.Left, Face.Right};
            var faceDoor = faces[Random.Range(0, faces.Length)];
            //List<bool> lits;
            // lits = new List<bool>((int) size.y);
            // for (int i = 0; i < lits.Capacity; i++)
            //     lits.Add(RandomBool);
            
            bool even = RandomBool;
            CreateFace(a, f, Face.Back,even,faceDoor==Face.Back);
            CreateFace(d, g, Face.Forward,even,faceDoor==Face.Forward);

            CreateFace(a, h, Face.Left,even,faceDoor==Face.Left);
            CreateFace(b, g, Face.Right,even,faceDoor==Face.Right);

            CreateFace(e, g, Face.Up,even);
            //CreateFace(a, c, Face.Down);
        }

        private void CreateFace(Vector3 a, Vector3 b, Face face,bool even, bool hasDoor = false,List<bool> windowsLits = null)
        {
            #region Setup

            Vector2 _a;
            Vector2 _b;
            float missingAxis;
            Vector3 normal;
            
            bool isRoof = face == Face.Up;
            
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

            //Set ProceduralFace Value
            
            float doorIndex = Random.Range(((int) 0), ((int) (_b.x - _a.x))-2) + _a.x+1;
            
            for (float y = _a.y; y < _b.y; y++)
            {
                           
                for (float x = _a.x; x < _b.x; x++)
                {
                    #region Get Matrix

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

                    //var quadName = "Quad - " + face + " - ( " + x.ToString("F1") + " : " + y.ToString("F1") + " )";
                    
                    ObjData obj = new ObjData(position, rotation, Vector3.one);
                    #endregion

                    #region Load Obj in GPULoader

                    Material mat = towerMaterialsAssembly.BackGroundMat;
                    if(!isRoof)
                    {
                        if (y < 2) //Ground
                        {
                            if (hasDoor && Math.Abs(doorIndex - x) < 0.1f)
                            {
                                mat = towerMaterialsAssembly.DoorMat;
                            }
                        }
                        else if (y < _b.y)
                        {
                            bool isEven = Mathf.RoundToInt(y - _a.y)% 2 == 0;
                            if ((isEven && even) || (!isEven && !even))
                            {
                                bool windowsLit = windowsLits?[Mathf.RoundToInt(y - _a.y)]??RandomBool;
                                if (windowsLit)
                                {
                                    mat = towerMaterialsAssembly.WindowsLitMat;
                                }
                                else
                                {
                                    mat = towerMaterialsAssembly.WindowsUnlitMat;
                                }
                            }
                        }
                    }
                    
                    //Set the matrix4x4 in the matrix corresponding to it's material and mesh
                    var tpObj = new TypeObj(_mesh, mat);
                    if (!_gpuLoader.DicBatches.ContainsKey(tpObj))
                    {
                        _gpuLoader.DicBatches.Add(tpObj, new List<List<ObjData>>());
                    }

                    if (_gpuLoader.DicBatches[tpObj].Count == 0 || _gpuLoader.DicBatches[tpObj][_gpuLoader.DicBatches[tpObj].Count-1].Count >= 1000)
                    {
                        _gpuLoader.DicBatches[tpObj].Add(new List<ObjData>());
                    }
                    _gpuLoader.DicBatches[tpObj][_gpuLoader.DicBatches[tpObj].Count-1].Add(obj);

                    #endregion
                    
                }
            }
        }

        [Button(ButtonSizes.Medium, ButtonStyle.FoldoutButton)]
        private void HardReset()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(transform.GetChild(i).gameObject);
            }
            _size = Vector3.zero;
            towerMaterialsAssembly = new TowerMaterialsAssembly();

            if (TryGetComponent(out Collider col))
                DestroyImmediate(col);
        }

        public static bool RandomBool => Random.value > 0.5f;
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

    public struct ObjData
     {
         public ObjData(Vector3 p, Quaternion r, Vector3 s)
         {
             pos = p;
             scale = s;
             rot = r;
         }
         public Vector3 pos;
         public Quaternion rot;
         public Vector3 scale;

         public Matrix4x4 matrix => Matrix4x4.TRS(pos, rot, scale);
     }
}
