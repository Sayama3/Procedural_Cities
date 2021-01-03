using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;
using System.Linq;
using TMPro;

namespace ProceduralCities.CitiesCreation
{
    [ExecuteAlways]
    public class CityGenerator : MonoBehaviour
    {
        [Title("General Parameter :")] 
        [SerializeField] private bool generateOnStart;
        [SerializeField] private bool stopAtEachBuilding;
        [SerializeField, Range(0, 1),ShowIf("stopAtEachBuilding")] private float waitEachBuilding;
        [SerializeField] private bool stopAtEachRow;
        [SerializeField, Range(0, 1),ShowIf("stopAtEachRow")] private float waitEachRow;
        
        [Title("Camera :")] 
        [SerializeField] private Camera camera;
        [SerializeField] private Vector3 cameraOffset;
        
        [Title("City Parameters :")]
        [FormerlySerializedAs("terrainSize")]
        [SerializeField,MinValue(1f)] private Vector2Int numberOfTower;
        //[SerializeField] private bool checkOverlap;

        [Title("Tower Parameters :")] 
        [SerializeField] private bool useCurve;
        [SerializeField,MinValue(1f),HideIf("useCurve")] private float2 towerHeight; [PropertySpace]
        [SerializeField,ShowIf("useCurve")] private AnimationCurve towerHeightCurve = new AnimationCurve(new [] {new Keyframe(0,50,0,0),new Keyframe(1,120,0,0) }); [PropertySpace]
        [SerializeField,MinValue(1f)] private float2x2 towerSize; [PropertySpace]
        [SerializeField,MinValue(0f)] private float2x2 spaceBetweenTower;
        
        [Title("Tower Parameters :")]
        [SerializeField] private Material _tempMat;
        [SerializeField,MinValue(float.Epsilon)] private float materialDivider = 1f;
        [SerializeField,MinValue(float.Epsilon)] private float groundMaterialTilling = 1f;
        [ShowInInspector, ReadOnly] private int NumberOfChildren => transform.childCount;

        //Private :
        private TowerGenerator[,] _towerGenerators;

        private Vector3[,] _towers;
        private Vector3[,] _spacings;
        
        
        private void Start()
        {
            if (generateOnStart) CreateCities();
        }

        #region City Creation

        [TitleGroup("Methods :")]
        [Button(ButtonSizes.Medium,ButtonStyle.FoldoutButton)]
        public void CreateCities()
        {
            DestroyAllChildren();
            StartCoroutine(CreateTowers());
        }
        private IEnumerator CreateTowers()
        {
            
            int index = 0;
            int[] X = new int[numberOfTower.x];
            int[] Z = new int[numberOfTower.y];
            int[] Xspace = new int[numberOfTower.x];
            int[] Zspace = new int[numberOfTower.y];

            float xoff = 0f;
            float zoff = 0f;
            
            for (int i = 0; i < numberOfTower.x; i++)
            {
                X[i] = (int)Random.Range(towerSize.c0.x, towerSize.c1.x);
                Xspace[i] = (int)Random.Range(spaceBetweenTower.c0.x,spaceBetweenTower.c1.x);
                xoff += X[i] + Xspace[i];
            }
            for (int i = 0; i < numberOfTower.y; i++)
            {
                Z[i] = (int)Random.Range(towerSize.c0.y,towerSize.c1.y);
                Zspace[i] = (int)Random.Range(spaceBetweenTower.c0.y,spaceBetweenTower.c1.y);
                zoff += Z[i] + Zspace[i];
            }
            yield return null;
            

            _towers = new Vector3[numberOfTower.x, numberOfTower.y];
            _spacings = new Vector3[numberOfTower.x, numberOfTower.y];
            
            for (int x = 0; x < numberOfTower.x; x ++)
            {
                for (int z = 0; z < numberOfTower.y; z ++)
                {
                    _towers[x,z] = new Vector3
                    {
                        x = X[x],
                        y = (int) (useCurve
                            ? towerHeightCurve.Evaluate(Random.value)
                            : Random.Range(towerHeight.x, towerHeight.y)),
                        z = Z[z]
                    };
                    _spacings[x, z] = new Vector3
                    {
                        x = Xspace[x],
                        y = 0,
                        z = Zspace[z]
                    };
                }
            }

            yield return null;
            StartCoroutine(CreateCity(xoff,zoff,stopAtEachBuilding,stopAtEachRow));
        }

        /// <summary>
        /// Function creating every building with parameters previously created.
        /// </summary>
        /// <param name="xSize">The estimated size of the city in X (don't right if you don't know)</param>
        /// <param name="zSize">The estimated size of the city in Z (don't right if you don't know)</param>
        /// <param name="stopAtEachBuilding">
        ///     Whether or not we end the frame after each building is created or not<br />
        ///     Enabling this option greatly increases the FPS but also greatly increases the city creation time.<br />
        ///     For example, for a city of 10.000 buildings (100x100), it will take a 10.000 frames. Assuming 0.02s per frame, it will take 200s equivalent to 3 minutes and 20 seconds.
        /// </param>
        /// <param name="stopAtEachRow">
        ///     Whether or not we end the frame after each row of building is created or not<br />
        ///     Enabling this option increases the FPS but also increases the city creation time.<br />
        ///     For example, for a city of 10.000 buildings (100x100), it will take a 100 frames. Assuming 0.02s per frame, it will take 100s equivalent to 1 minutes and 40 seconds.
        ///     (note that the number of row is describe by the x component.)
        /// </param>
        /// <returns></returns>
        private IEnumerator CreateCity(float xSize = 0f, float zSize = 0f,bool stopAtEachBuilding = false, bool stopAtEachRow = false)
        {
            //Positioning the camera in the center of the city
            var cameraPos = transform.position + cameraOffset;
            cameraPos.y += towerHeight.y;
            camera.transform.position = cameraPos;
            camera.farClipPlane = (numberOfTower.x * numberOfTower.y)*2f;
            
            //Creating the ground
            var quad = GameObject.CreatePrimitive(PrimitiveType.Quad).transform;
            Material material = (quad.GetComponent<MeshRenderer>().material = _tempMat);
            material.SetVector("_BaseMap_ST", new Vector4(groundMaterialTilling, groundMaterialTilling, 0, 0));
            
            quad.name = "Ground";
            quad.position = transform.position;
            var scale = new Vector3(xSize,zSize,1f);
            quad.localScale = scale;
            quad.Rotate(Vector3.right,90f);
            quad.SetParent(transform);
            
            yield return null;
            
            TowerGenerator towerReference = new GameObject("Tower", new [] {typeof(TowerGenerator)}).GetComponent<TowerGenerator>();
            towerReference.gameObject.isStatic = true;
            
            
            Vector3 firstOffset = transform.position - new Vector3(xSize/2f,0,zSize/2f);
            _towerGenerators = new TowerGenerator[numberOfTower.x, numberOfTower.y];
            
            Vector3[][] cumulatedPos = new Vector3[numberOfTower.x][];
            for (int index = 0; index < numberOfTower.x; index++)
            {
                cumulatedPos[index] = new Vector3[numberOfTower.y];
            }
            
            yield return null;

            for (int x = 0; x < numberOfTower.x; x++)
            {
                for (int z = 0; z < numberOfTower.y; z++)
                {
                    Vector3 currentOffset;
                    
                    if (x == 0)
                    {
                        if (z == 0)
                        {
                            currentOffset = firstOffset;
                        }
                        else
                        {
                            currentOffset = new Vector3
                            {
                                x = firstOffset.x,
                                y = firstOffset.y,
                                z = cumulatedPos[x][z - 1].z
                            };
                        }
                    }
                    else if (z == 0)
                    {
                        currentOffset = new Vector3
                        {
                            x = cumulatedPos[x - 1][z].x,
                            y = firstOffset.y,
                            z = firstOffset.z
                        };
                    }
                    else
                    {
                        currentOffset = new Vector3
                        {
                            x = cumulatedPos[x - 1][z].x,
                            y = firstOffset.y,
                            z = cumulatedPos[x][z - 1].z
                        };

                    }
                    
                    Vector3 size = _towers[x, z];
                    Vector3 halfSize = new Vector3(size.x / 2f, 0, size.z / 2f);
                    Vector3 spacing = _spacings[x, z];
                    
                    Vector3 pos = currentOffset + halfSize +_spacings[x, z];
                    cumulatedPos[x][z] = pos + halfSize;
                    
                    _towerGenerators[x,z] = Instantiate(towerReference, pos, Quaternion.identity, transform);
                    _towerGenerators[x, z].name += " - (" + x + "," + z + ")";
                    _towerGenerators[x,z].Initialize(_towers[x, z],_tempMat,materialDivider);
                    
                    if(stopAtEachBuilding) yield return waitEachBuilding <= Time.deltaTime? null:new WaitForSeconds(waitEachBuilding);
                }
                
                if(stopAtEachRow) yield return waitEachRow <= Time.deltaTime? null:new WaitForSeconds(waitEachRow);
            }

            yield return null;

            DestroyImmediate(towerReference.gameObject,false);
        }

        #endregion

        [TitleGroup("Methods :")]
        [Button(ButtonSizes.Medium)]
        public void DestroyAllChildren()
        {
            StopAllCoroutines();
            
            if (NumberOfChildren > 0)
            {
                List<GameObject> children = new List<GameObject>();
                for (int i = 0; i < NumberOfChildren; i++)
                {
                    children.Add(transform.GetChild(i).gameObject);
                }

                foreach (var child in children)
                {
                    DestroyImmediate(child, false);
                }
            }
        }
        
        public void SetWaitRow(bool value) => stopAtEachRow = value;
        public void SetWaitBuilding(bool value) => stopAtEachBuilding = value;
        public void SetWaitRow(float value) => waitEachRow = value;
        public void SetWaitBuilding(float value) => waitEachBuilding = value;
        public void SetWaitRow(TextMeshProUGUI text) => text.text = waitEachRow.ToString("F4");
        public void SetWaitBuilding(TextMeshProUGUI text) => text.text = waitEachBuilding.ToString("F4");
        



    }

}