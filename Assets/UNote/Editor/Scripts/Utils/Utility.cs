using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UNote.Editor
{
    internal enum ScreenshotTarget
    {
        GameView,
        SceneView,
    }
    
    public static class Utility
    {
        public static bool Initialized
        {
            get
            {
                return PathUtil.Initialized;
            }
        }

        /// <summary>
        /// Save EditorWindow view as png
        /// </summary>
        /// <param name="screenshotType"></param>
        /// <param name="savePath"></param>
        internal static void TakeScreenShot(ScreenshotTarget screenshotType, string savePath)
        {
            Texture2D screenshot = null;
            
            switch (screenshotType)
            {
                case ScreenshotTarget.GameView:
                    Camera gameViewCamera = Camera.main;

                    if (gameViewCamera != null)
                    {
                        screenshot = CameraViewToScreenshot(gameViewCamera);
                    }
                    break;
                
                case ScreenshotTarget.SceneView:
                    SceneView sv = SceneView.lastActiveSceneView;
                    if (sv != null)
                    {
                        screenshot = CameraViewToScreenshot(sv.camera);   
                    }
                    break;
            }

            if (!screenshot)
            {
                return;
            }

            string directory = Path.GetDirectoryName(savePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            
            byte[] ssBytes = screenshot.EncodeToPNG();
            File.WriteAllBytes(savePath, ssBytes);
            
            AssetDatabase.ImportAsset(savePath);
            
            // NPOT to None
            TextureImporter textureImporter = (TextureImporter)AssetImporter.GetAtPath(savePath);
            if (textureImporter != null)
            {
                textureImporter.npotScale = TextureImporterNPOTScale.None;
                textureImporter.mipmapEnabled = false;
                textureImporter.SaveAndReimport();
            }
            
            AssetDatabase.Refresh();
            
            // Destroy
            Object.DestroyImmediate(screenshot);
        }
        
        private static Texture2D CameraViewToScreenshot(Camera camera)
        {
            RenderTexture lastRt = camera.targetTexture;
            int w = Mathf.RoundToInt(camera.pixelWidth);
            int h = Mathf.RoundToInt(camera.pixelHeight);
            RenderTexture rt = new RenderTexture(w, h, 16);
            rt.Create();
            Texture2D screenshot = new Texture2D(w, h, TextureFormat.RGB24, false);

            camera.targetTexture = rt;
            camera.Render();

            RenderTexture.active = rt;
            screenshot.ReadPixels(new Rect(0, 0, w, h), 0, 0);
            screenshot.Apply();

            camera.targetTexture = lastRt;
            
            RenderTexture.active = null;
            Object.DestroyImmediate(rt);

            return screenshot;
        }
    }
}
