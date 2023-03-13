using System.Collections.Generic;
using UnityEngine;
using ProceduralCities.CitiesCreation;
using NaughtyAttributes;

namespace ProceduralCities
{
    public class GPULoader : MonoBehaviour
    {
        #region Singleton

        private static GPULoader _instance;

        public static GPULoader Instance => _instance;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(this);
                return;
            }

            _instance = this;
        }

        #endregion

        public Dictionary<TypeObj, List<List<ObjData>>> DicBatches = new Dictionary<TypeObj, List<List<ObjData>>>();

        void Update()
        {
            RenderBatches();
        }
        private void RenderBatches()
        {
            foreach (var batches in DicBatches)
            {
                foreach (var batch in batches.Value)
                {
                    Matrix4x4[] matrixs = new Matrix4x4[batch.Count];
                    for (int i = 0; i < batch.Count; i++)
                        matrixs[i] = batch[i].matrix;
                    Graphics.DrawMeshInstanced(batches.Key.mesh, 0, batches.Key.material, matrixs);
                }
            }
        }

        public void Clear() => DicBatches?.Clear();
        
    }

    public struct TypeObj : IEqualityComparer<TypeObj>
    {
        public TypeObj(Mesh m, Material mat)
        {
            mesh = m;
            material = mat;
        }
        public Mesh mesh;
        public Material material;
        public bool Equals(TypeObj x, TypeObj y)
        {
            return x.material == y.material && x.mesh == y.mesh;
        }

        public int GetHashCode(TypeObj obj)
        {
            return obj.GetHashCode();
        }
    }
}
