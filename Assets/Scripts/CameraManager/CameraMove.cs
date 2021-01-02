using System;
using UnityEngine;
using UnityEngine.Events;

namespace ProceduralCities.CameraManager
{
    [RequireComponent(typeof(Camera), typeof(AudioListener))]
    public class CameraMove : MonoBehaviour
    {
        #region Variables

        [Header("Editor :")] [SerializeField] private bool _hideCursor;
        [SerializeField] private bool drawGizmo;
        [SerializeField, Min(1f)] private float _gizmoLenght = 1f;

        [Header("Sensibility :")] [SerializeField, Min(0.1f)]
        private float _mouseWheelSensibility = 50f;

        [SerializeField, Min(0.1f)] private float _globalSensibility = 1f;
        [SerializeField] private Vector2 _sensibility = Vector2.one;

        [Header("Clamp :")] [SerializeField] private Vector2 _clampX = Vector2.zero;
        [SerializeField] Vector2 _clampY = new Vector2(-180, 180);

        [Header("Zoom :")] public bool useZoom = true;
        [SerializeField] private Vector2 _zoomClamp = new Vector2(10, 60);

        [Header("Event :")] public UnityEvent<Vector3> OnAngleChange;

        [Header("Cursor :")] 
        [SerializeField] private CursorLockMode defaultLockMode = CursorLockMode.Locked;

        private Camera _camera;
        private Vector3 _eulerAngle = new Vector3();


        #endregion

        #region Methods

        private void Start()
        {
            #region Initialization

            _camera = GetComponent<Camera>();

            _eulerAngle = transform.rotation.eulerAngles;

            if (_clampX != Vector2.zero)
            {
                _clampX.x += _eulerAngle.y;
                _clampX.y += _eulerAngle.y;
            }

            if (_clampY != Vector2.zero)
            {
                _clampY.x -= _eulerAngle.x;
                _clampY.y -= _eulerAngle.x;
            }

            if (_hideCursor)
            {
                Cursor.lockState = defaultLockMode;
                Cursor.visible = false;
            }

            #endregion

            drawGizmo = false;
        }


        #region Camera Action Methods

        private void CameraZoom(float mouseWheel)
        {
            mouseWheel = -mouseWheel * _mouseWheelSensibility;
            if (mouseWheel != 0)
            {
                if (_camera.orthographic)
                {
                    _camera.orthographicSize = _zoomClamp != Vector2.zero
                        ? Mathf.Clamp(_camera.orthographicSize + mouseWheel, _zoomClamp.x, _zoomClamp.y)
                        : _camera.orthographicSize + mouseWheel;
                }
                else
                {
                    _camera.fieldOfView = _zoomClamp != Vector2.zero
                        ? Mathf.Clamp(_camera.fieldOfView + mouseWheel, _zoomClamp.x, _zoomClamp.y)
                        : _camera.fieldOfView + mouseWheel;
                }
            }
        }

        private void CameraMovement(float inputX, float inputY)
        {
            _eulerAngle.y = _clampX != Vector2.zero
                ? Mathf.Clamp(_eulerAngle.y + inputX * _globalSensibility * _sensibility.x, _clampX.x, _clampX.y)
                : _eulerAngle.y + inputX * _globalSensibility * _sensibility.x;
            _eulerAngle.x = _clampY != Vector2.zero
                ? Mathf.Clamp(_eulerAngle.x - inputY * _globalSensibility * _sensibility.y, _clampY.y * -1,
                    _clampY.x * -1)
                : _eulerAngle.x - inputY * _globalSensibility * _sensibility.y;
            if (_camera.transform.rotation != Quaternion.Euler(_eulerAngle))
            {
                _camera.transform.rotation = Quaternion.Euler(_eulerAngle);
                OnAngleChange?.Invoke(_eulerAngle);
            }
        }

        #endregion

        #region Input Handling

#if ENABLE_INPUT_SYSTEM

    public void OnCameraMovement(InputAction.CallbackContext ctx)
    {
        var move = ctx.ReadValue<Vector2>();
        CameraMovement(move.x, move.y);
    }

    public void OnZoom(InputAction.CallbackContext ctx)
    {
        if (useZoom)
        {
            var zoom = ctx.ReadValue<float>();
            CameraZoom(zoom);
        }
    }

#elif ENABLE_LEGACY_INPUT_MANAGER

        [Header("Input Button Name :")] [SerializeField]
        private string horizontalInputMain = "Mouse X";

        [SerializeField] private string horizontalInputSecondary = "Horizontal";
        [Space] [SerializeField] private string verticalInputMain = "Mouse Y";
        [SerializeField] private string verticalInputSecondary = "Vertical";

        [Space] [SerializeField] private string toggleCursorInput = "Fire2";
        [SerializeField] private CursorLockMode alternateCursorMode = CursorLockMode.Confined;

        private Transform target;

        private void Update()
        {
            void CameraMover()
            {
                var inputX = Input.GetAxisRaw(horizontalInputMain);
                if (inputX == 0 && horizontalInputSecondary != null)
                {
                    inputX = Input.GetAxisRaw(horizontalInputSecondary);
                }

                var inputY = Input.GetAxisRaw(verticalInputMain);
                if (inputY == 0 && verticalInputSecondary != null)
                {
                    inputY = Input.GetAxisRaw(verticalInputSecondary);
                }

                CameraMovement(inputX, inputY);
                if (useZoom)
                {
                    var mouseWheel = Input.GetAxisRaw("Mouse ScrollWheel");
                    CameraZoom(mouseWheel);
                }
            }

            if(!string.IsNullOrEmpty(toggleCursorInput))
            {
                if (Input.GetButtonDown(toggleCursorInput)) _hideCursor = !_hideCursor;
                
                if (_hideCursor)
                {
                    Cursor.lockState = defaultLockMode;
                    Cursor.visible = false;
                    CameraMover();
                }
                else
                {
                    Cursor.lockState = alternateCursorMode;
                    Cursor.visible = true;
                }
            }
            else
                CameraMover();
            
        }

#endif

        #endregion

        #endregion

        #region Gizmos

        private void OnDrawGizmos()
        {
            if (drawGizmo)
            {
                Vector2 clampX = _clampX;
                if (clampX != Vector2.zero)
                {
                    Gizmos.color = Color.red;
                    int signMin = (int) (Mathf.Sign(clampX.x) == 0 ? 1 : Mathf.Sign(clampX.x));
                    int signMax = (int) (Mathf.Sign(clampX.y) == 0 ? 1 : Mathf.Sign(clampX.y));

                    clampX.x = (Mathf.Abs(clampX.x) % 180f) * signMin;
                    clampX.y = (Mathf.Abs(clampX.y) % 180f) * signMax;

                    Vector3 min = Mathf.Abs(clampX.x) <= 90
                        ? Vector3.Lerp(transform.forward, signMin * transform.right, Mathf.Abs(clampX.x) / 90)
                        : Vector3.Lerp(signMin * transform.right, -transform.forward, (Mathf.Abs(clampX.x) - 90) / 90);
                    Vector3 max = Mathf.Abs(clampX.y) <= 90
                        ? Vector3.Lerp(transform.forward, signMax * transform.right, Mathf.Abs(clampX.y) / 90)
                        : Vector3.Lerp(signMax * transform.right, -transform.forward, (Mathf.Abs(clampX.y) - 90) / 90);

                    Ray rayMin = new Ray(transform.position, min.normalized);
                    Ray rayMax = new Ray(transform.position, max.normalized);
                    Gizmos.DrawLine(rayMin.origin, rayMin.GetPoint(_gizmoLenght));
                    Gizmos.DrawLine(rayMax.origin, rayMax.GetPoint(_gizmoLenght));
                    Gizmos.DrawLine(rayMin.GetPoint(_gizmoLenght), rayMax.GetPoint(_gizmoLenght));
                }

                Vector2 clampY = _clampY;
                if (clampY != Vector2.zero)
                {
                    Gizmos.color = Color.blue;
                    int signMin = (int) (Mathf.Sign(clampY.x) == 0 ? 1 : Mathf.Sign(clampY.x));
                    int signMax = (int) (Mathf.Sign(clampY.y) == 0 ? 1 : Mathf.Sign(clampY.y));
                    clampY.x = (Mathf.Abs(clampY.x) % 180f) * signMin;
                    clampY.y = (Mathf.Abs(clampY.y) % 180f) * signMax;

                    Vector3 min = Mathf.Abs(clampY.x) <= 90
                        ? Vector3.Lerp(transform.forward, signMin * transform.up, Mathf.Abs(clampY.x) / 90)
                        : Vector3.Lerp(signMin * transform.up, -transform.forward, (Mathf.Abs(clampY.x) - 90) / 90);
                    Vector3 max = Mathf.Abs(clampY.y) <= 90
                        ? Vector3.Lerp(transform.forward, signMax * transform.up, Mathf.Abs(clampY.y) / 90)
                        : Vector3.Lerp(signMax * transform.up, -transform.forward, (Mathf.Abs(clampY.y) - 90) / 90);


                    Ray rayMin = new Ray(transform.position, min.normalized);
                    Ray rayMax = new Ray(transform.position, max.normalized);
                    Gizmos.DrawLine(rayMin.origin, rayMin.GetPoint(_gizmoLenght));
                    Gizmos.DrawLine(rayMax.origin, rayMax.GetPoint(_gizmoLenght));
                    Gizmos.DrawLine(rayMin.GetPoint(_gizmoLenght), rayMax.GetPoint(_gizmoLenght));
                }
            }
        }

        #endregion
    }
}