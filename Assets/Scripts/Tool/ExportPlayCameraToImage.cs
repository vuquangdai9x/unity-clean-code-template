using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Game.Tools
{
    public class ExportPlayCameraToImage : MonoBehaviour
    {
        public string saveFolderPathFromAssets = "/Games/TextureNew/PvPCards";
        public string filePrefix = "screenshot";
        public int fileCounter;
        public KeyCode screenshotKey;
        public Camera usedCamera;

        private void LateUpdate()
        {
            if (Input.GetKeyDown(screenshotKey))
            {
                Capture();
            }
        }

        public void Capture()
        {
            RenderTexture activeRenderTexture = RenderTexture.active;
            RenderTexture.active = usedCamera.targetTexture;

            usedCamera.Render();

            Texture2D image = new Texture2D(usedCamera.targetTexture.width, usedCamera.targetTexture.height);
            image.ReadPixels(new Rect(0, 0, usedCamera.targetTexture.width, usedCamera.targetTexture.height), 0, 0);
            image.Apply();
            RenderTexture.active = activeRenderTexture;

            byte[] bytes = image.EncodeToPNG();
            Destroy(image);

            string path = Application.dataPath + saveFolderPathFromAssets + "/" + filePrefix + fileCounter + ".png";
            File.WriteAllBytes(path, bytes);
            Debug.Log("Saved image to " + path);

            fileCounter++;
        }
    }
}