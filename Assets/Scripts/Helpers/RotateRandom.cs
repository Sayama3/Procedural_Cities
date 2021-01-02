using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ProceduralCities.Helpers
{
    [ExecuteInEditMode]
    public class RotateRandom : MonoBehaviour
    {
        [SerializeField] private bool _rotationLocal = true;

        [SerializeField, EnumToggleButtons] private SnapAxis _axis = SnapAxis.None;

        [SerializeField] private bool rotateInGame = false;

        private void Start()
        {
            if (rotateInGame) Rotate();
        }

        [Button(ButtonSizes.Medium)]
        private void Rotate()
        {
            var currentRotation = _rotationLocal ? transform.localRotation : transform.rotation;

            Vector3 rotation = new Vector3();

            rotation.x = _axis.HasFlag(SnapAxis.X) ? Random.Range(0.0f, 360.0f) : currentRotation.x;
            rotation.y = _axis.HasFlag(SnapAxis.Y) ? Random.Range(0.0f, 360.0f) : currentRotation.y;
            rotation.z = _axis.HasFlag(SnapAxis.Z) ? Random.Range(0.0f, 360.0f) : currentRotation.z;

            if (_rotationLocal)
            {
                transform.localRotation = Quaternion.Euler(rotation);
            }
            else
            {
                transform.rotation = Quaternion.Euler(rotation);
            }
        }
    }
}