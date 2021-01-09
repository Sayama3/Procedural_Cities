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
        

        private void Update()
        {
            if(Input.GetKeyDown(screenshotKey))
            {
                //File.WriteAllBytes(path, I360Render.Capture(2048, true, camera, true));
                var date = DateTime.Now;
                ScreenCapture.CaptureScreenshot(
                    "Screenshot - " + new Guid().ToString());
            }
        }
    }
}
