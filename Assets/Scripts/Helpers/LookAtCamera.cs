using ProceduralCities.CameraManager;
using UnityEngine;
using Sirenix.OdinInspector;

namespace ProceduralCities.Helpers
{
    [ExecuteInEditMode]
    public class LookAtCamera : MonoBehaviour
    {

        [SerializeField, EnumToggleButtons] private Axis _axisThatLook = Axis.Z;

        [SerializeField] private bool _resetRotationBeforeApply = true;
        [SerializeField] private bool rotateInGame = false;

        [SerializeField, ShowIf("@_cameraChanger == null")]
        private Camera _camera;

        [SerializeField, ShowIf("@_camera == null")]
        private CameraChanger _cameraChanger;


        void Start()
        {
            SetCamera();
        }

        // Update is called once per frame
        void Update()
        {
            if (rotateInGame)
            {
                LookCurrentCamera();
            }
        }

        [Button(ButtonSizes.Medium)]
        private void SetCamera()
        {
            if (_cameraChanger != null)
            {
                _camera = _cameraChanger.currentCamera;
                _cameraChanger.OnCameraChange += cam => _camera = cam;
            }
            else if (_cameraChanger == null && _camera == null)
            {
                _camera = Camera.main;
                if (_camera == null)
                {
                    _camera = FindObjectOfType<Camera>();
                }
            }
        }


        [Button(ButtonSizes.Medium)]
        private void LookCurrentCamera()
        {
            if (_resetRotationBeforeApply)
                transform.rotation = Quaternion.identity;

            Vector3 axis = Vector3.zero;
            Vector3 direction = (_camera.transform.position - transform.position).normalized;
            direction = Vector3.Cross(-Vector3.Cross(direction, Vector3.up).normalized, Vector3.up);


            switch (_axisThatLook)
            {
                case Axis.X:
                    axis = transform.right;
                    break;
                case Axis.Y:
                    axis = transform.up;
                    break;
                case Axis.Z:
                    axis = transform.forward;
                    break;
            }

            transform.rotation = Quaternion.FromToRotation(axis, direction);

            // switch (_axisThatLook)
            // {
            //     case Axis.X:
            //         transform.up = Vector3.up;
            //         break;
            //     case Axis.Y:
            //         transform.forward = Vector3.down;
            //         break;
            //     case Axis.Z:
            //         transform.up = Vector3.up;
            //         break;
            // }
        }


    }

    public enum Axis
    {
        X = 0,
        Y = 1,
        Z = 2
    }
}