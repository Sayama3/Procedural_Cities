﻿using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;
using System.Linq;

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
        [SerializeField] private bool useCurve;
        [SerializeField,MinValue(1f),HideIf("useCurve")] private float2 towerHeight; [PropertySpace]
        [SerializeField,ShowIf("useCurve")] private AnimationCurve towerHeightCurve = new AnimationCurve(new [] {new Keyframe(0,50,0,0),new Keyframe(1,120,0,0) }); [PropertySpace]
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
            int[] X = new int[numberOfTower.x];
            int[] Z = new int[numberOfTower.y];
            int[] Xspace = new int[numberOfTower.x];
            int[] Zspace = new int[numberOfTower.y];

            for (int i = 0; i < numberOfTower.x; i++)
            {
                X[i] = (int)Random.Range(towerSize.c0.x, towerSize.c1.x);
                Xspace[i] = (int)Random.Range(spaceBetweenTower.c0.x,spaceBetweenTower.c1.x);
            }
            for (int i = 0; i < numberOfTower.y; i++)
            {
                Z[i] = (int)Random.Range(towerSize.c0.y,towerSize.c1.y);
                Zspace[i] = (int)Random.Range(spaceBetweenTower.c0.y,spaceBetweenTower.c1.y);
            }
            

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
                        
                        cameraPos.y = transform.position.y;
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