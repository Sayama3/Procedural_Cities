using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralCities.CameraManager
{
    public class CameraChanger : MonoBehaviour
    {
        #region Variable

        [Header("Parameter Camera")] [SerializeField]
        private List<Camera> _listCameras = new List<Camera>();

        [SerializeField, Min(0)] private int _index = 0;

        private int Index
        {
            get => _index;
            set
            {
                if (value != _index)
                {
                    ApplyIndex(value);
                    _index = value;
                    OnCameraChange?.Invoke(currentCamera);
                }
            }
        }

        [Header("Parameter Button")] [SerializeField]
        private KeyCode _inputAvant = KeyCode.Mouse0;

        [SerializeField] private KeyCode _inputArriere = KeyCode.Mouse1;

        #endregion

        #region Action

        public Action<Camera> OnCameraChange;

        #endregion

        #region Getter/Setter

        public Camera currentCamera => _listCameras[Index];

        #endregion

        private void Awake()
        {
            foreach (Camera camera in _listCameras)
            {
                camera.gameObject.SetActive(false);
            }

            _listCameras[_index].gameObject.SetActive(true);
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(_inputAvant))
                Index = (Index + 1 >= _listCameras.Count) ? 0 : Index + 1;

            if (Input.GetKeyDown(_inputArriere))
                Index = (Index - 1 < 0) ? _listCameras.Count - 1 : Index - 1;
        }

        void ApplyIndex(int index)
        {
            _listCameras[_index].gameObject.SetActive(false);
            _listCameras[index].gameObject.SetActive(true);
        }
    }
}