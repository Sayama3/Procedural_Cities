using System;
using System.IO;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ProceduralCities
{
    public class Screenshot360 : MonoBehaviour
    {
        [SerializeField] private string path;
        [SerializeField] private KeyCode screenshotKey = KeyCode.F10;
        [SerializeField, Required] private Camera camera;
        private string _keyScreen = "screenNumber";

        private void Start()
        {
            if (!PlayerPrefs.HasKey(_keyScreen)) PlayerPrefs.SetInt(_keyScreen,0);
        }

        private void Update()
        {
            if(Input.GetKeyDown(screenshotKey))
            {
                //File.WriteAllBytes(path, I360Render.Capture(2048, true, camera, true));
                PlayerPrefs.GetInt(_keyScreen);
                var date = DateTime.Now;
                ScreenCapture.CaptureScreenshot(
                    "Screenshot - " + PlayerPrefs.GetInt(_keyScreen).ToString("000 000") + ".png");
                PlayerPrefs.SetInt(_keyScreen, PlayerPrefs.GetInt(_keyScreen) + 1);
            }
        }
    }
}
