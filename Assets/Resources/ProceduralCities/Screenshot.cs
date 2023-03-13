using System;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;

namespace ProceduralCities
{
    public class Screenshot : MonoBehaviour
    {
        private const string PictureFolderKey = "PictureFolder";
        [SerializeField] private string path;
        [SerializeField] private KeyCode screenshotKey = KeyCode.F10;
        [SerializeField, Required] private Camera camera;

        public UnityEvent<string> StartSetPath;

        private void Awake()
        {
            if (!PlayerPrefs.HasKey(PictureFolderKey))
            {
                PlayerPrefs.SetString(PictureFolderKey, Environment.GetFolderPath(Environment.SpecialFolder.MyPictures));
            }

            this.path = PlayerPrefs.GetString(PictureFolderKey);
            if(StartSetPath != null) {
                StartSetPath.Invoke(this.path);
            }
        }

        private void Update()
        {
            if(Input.GetKeyDown(screenshotKey))
            {
                var date = DateTime.Now;
                ScreenCapture.CaptureScreenshot("Screenshot - " + date.ToString("dd-MM-yy_HH-mm-ss") + ".png");
            }
        }

        public void SetPath(string path)
        {
            if(System.IO.Directory.Exists(path))
            {
                this.path = path;
                PlayerPrefs.SetString(PictureFolderKey, path);
            }
            else
            {
                Debug.LogWarning($"The path {path} does not exist.");
            }
        }
    }
}
