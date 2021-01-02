using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace ProceduralCities.CitiesCreation
{
    public class CityGenerator : MonoBehaviour
    {
        [Title("Camera :")] 
        [SerializeField] private Camera camera;
        [SerializeField] private Vector3 cameraOffset;
        
        [Title("City Parameters :")]
        [FormerlySerializedAs("terrainSize")]
        [SerializeField,MinValue(1f)] private Vector2Int numberOfTower;
        //[SerializeField] private bool checkOverlap;

        [Title("Tower Parameters :")]
        [SerializeField,MinValue(1f)] private float2 towerHeight; [PropertySpace]
        [SerializeField,MinValue(1f)] private float2x2 towerSize; [PropertySpace]
        [SerializeField,MinValue(0f)] private float2x2 spaceBetweenTower;
        
        [Title("Tower Parameters :")]
        [SerializeField] private Material _tempMat;
        [SerializeField,MinValue(float.Epsilon)] private float materialDivider = 1f;
        [ShowInInspector, ReadOnly] private int NumberOfChildren => transform.childCount;

        //Private :
        private TowerGenerator[,] _towerGenerators;

        private Vector3[,] _towers;
        private Vector3[,] _spacings;
        
        
        private void Start() => CreateCities();
        

        #region City Creation

        [TitleGroup("Methods :")]
        [Button(ButtonSizes.Medium)]
        public void CreateCities()
        {
            DestroyAllChildren();
            StartCoroutine(CreateTowers());
        }
        private IEnumerator CreateTowers()
        {
            
            int index = 0;
            _towers = new Vector3[numberOfTower.x, numberOfTower.y];
            _spacings = new Vector3[numberOfTower.x, numberOfTower.y];
            for (int x = 0; x < numberOfTower.x; x ++)
            {
                
                for (int y = 0; y < numberOfTower.y; y ++)
                {
                    _towers[x,y] = new Vector3
                    {
                        x = Random.Range(towerSize.c0.x,towerSize.c1.x),
                        y = Random.Range(towerHeight.x, towerHeight.y),
                        z = Random.Range(towerSize.c0.y,towerSize.c1.y)
                    };
                    _spacings[x, y] = new Vector3
                    {
                        x = Random.Range(spaceBetweenTower.c0.x,spaceBetweenTower.c1.x),
                        y = 0f,
                        z = Random.Range(spaceBetweenTower.c0.y,spaceBetweenTower.c1.y)
                    };
                }
            }

            yield return null;
            StartCoroutine(CreateCity());
        }

        private IEnumerator CreateCity()
        {
            TowerGenerator towerReference = new GameObject("Tower", new [] {typeof(TowerGenerator)}).GetComponent<TowerGenerator>();
            towerReference.gameObject.isStatic = true;
            
            
            Vector3 firstOffset = transform.position;
            _towerGenerators = new TowerGenerator[numberOfTower.x, numberOfTower.y];
            
            Vector3[][] cumulatedPos = new Vector3[numberOfTower.x][];
            for (int index = 0; index < numberOfTower.x; index++)
            {
                cumulatedPos[index] = new Vector3[numberOfTower.y];
            }

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

                    if (x == numberOfTower.x-1 && z == numberOfTower.y-1)
                    {
                        var cameraPos = ((pos-transform.position)/2f)+cameraOffset;
                        cameraPos.y += towerHeight.y;
                        camera.transform.position = cameraPos;
                        var quad = GameObject.CreatePrimitive(PrimitiveType.Quad).transform;
                        
                        cameraPos.y -= towerHeight.y;
                        quad.position = cameraPos;
                        var scale = pos - transform.position;
                        scale.y = scale.z;
                        scale.z = 1f;
                        quad.localScale = scale;
                        quad.Rotate(Vector3.right,90f);
                        quad.SetParent(transform);
                    }
                }
            }
            
            DestroyImmediate(towerReference.gameObject,false);
            yield return new WaitForEndOfFrame();
        }

        #endregion

        #region Coroutine
        

        #endregion
        
        [TitleGroup("Methods :")]
        [Button(ButtonSizes.Medium)]
        private void DestroyAllChildren()
        {
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
    }
}