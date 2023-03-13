using System;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

namespace ProceduralCities.Tester
{
    public class QuadFiller : MonoBehaviour
    {
        [SerializeField] private Point quadPoints = new Point(z:0);
        [SerializeField] private Color _colorLine = Color.green;
        [SerializeField] private Color _colorSphere = Color.red;
        [SerializeField,ReadOnly] private List<GameObject> quads;
        [SerializeField, MinValue(0),MaxValue(1),ReadOnly] private Vector2 offset = new Vector2(.5f, .5f);

        [Button()]
        public void DrawQuads()
        {
            if (quads != null)
            {
                if(quads.Count >0)
                {
                    for (int i = quads.Count - 1; i >= 0; i--)
                    {
                        Destroy(quads[i]);
                    }
                }
                quads.Clear();
            }
            quads = new List<GameObject>();
            var refObj = GameObject.CreatePrimitive(PrimitiveType.Quad);
            DestroyImmediate(refObj.GetComponent<Collider>(),false);
            
            for (int y = quadPoints.LeftDown.y; y < quadPoints.RightUpper.y; y++)
            {
                for (int x = quadPoints.LeftDown.x; x < quadPoints.RightUpper.x; x++)
                {
                    Transform trans= transform;
                    quads.Add(
                        Instantiate(refObj, trans.TransformPoint(x + offset.x, y + offset.y, quadPoints.Z), Quaternion.LookRotation(-trans.forward,trans.up),transform));
                }
            }
            DestroyImmediate(refObj,false);
        }
        
        private void OnDrawGizmos()
        {
            var previous = quadPoints.LeftDown;
            Gizmos.color = _colorSphere;
            Gizmos.DrawWireSphere(transform.TransformPoint(previous),0.5f);
            for (int i = 1; i < quadPoints.Points.Length; i++)
            {
                var current = quadPoints.Points[i];
                Gizmos.color = _colorSphere;
                Gizmos.DrawWireSphere(transform.TransformPoint(current),0.5f);
                Gizmos.color = _colorLine;
                Gizmos.DrawLine(transform.TransformPoint(previous),transform.TransformPoint(current));
                previous = current;
            }
            Gizmos.DrawLine(transform.TransformPoint(quadPoints.LeftDown),transform.TransformPoint(previous));
        }
    }

    [Serializable]
    public struct Point
    {

        [SerializeField] private int z;
        [SerializeField,MinValue(1)] private int multiplier;
        [SerializeField] private Vector2Int leftDown;
        [SerializeField] private Vector2Int rightUpper;

        public Point(int z = 0)
        {
            this.z = z;
            multiplier = 1;
            leftDown = new Vector2Int(-1,-1);
            rightUpper = new Vector2Int(1, 1);
        }

        public Point(int z, Vector2Int leftDown, Vector2Int rightUpper)
        {
            this.z = z;
            this.multiplier = 1;
            this.leftDown = leftDown;
            this.rightUpper = rightUpper;
        }

        public int Z => z;
        public Vector3Int LeftDown => new Vector3Int(leftDown.x*multiplier, leftDown.y*multiplier, z);
        public Vector3Int RightDown => new Vector3Int(rightUpper.x*multiplier, leftDown.y*multiplier, z);
        public Vector3Int RightUpper => new Vector3Int(rightUpper.x*multiplier, rightUpper.y*multiplier, z);
        public Vector3Int LeftUpper => new Vector3Int(leftDown.x*multiplier, rightUpper.y*multiplier, z);

        /// <summary>
        /// Array with :<br />
        ///  - LeftDown <br />
        ///  - RightDown <br />
        ///  - RightUpper <br />
        ///  - LeftUpper <br />
        /// </summary>
        public Vector3Int[] Points => new[] {LeftDown, RightDown, RightUpper, LeftUpper};
    }
}
