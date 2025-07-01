using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BeltainsTools
{
    [RequireComponent(typeof(Camera))]
    public class ScreenshotCapturer : MonoBehaviour
    {
        Camera m_Cam;


        public static void CaptureScreenshotWithCamera(Camera camera, System.Action<Sprite> onCapturedCallback)
            => GetOrAddCapturerForCamera(camera).CaptureScreenshot(onCapturedCallback);

        public static void CaptureScreenshotWithCamera(Camera camera, System.Action<Texture2D> onCapturedCallback)
            => GetOrAddCapturerForCamera(camera).CaptureScreenshot(onCapturedCallback);

        public static ScreenshotCapturer GetOrAddCapturerForCamera(Camera camera)
        {
            ScreenshotCapturer capturer = camera.GetComponent<ScreenshotCapturer>();
            if (capturer == null)
                capturer = camera.gameObject.AddComponent<ScreenshotCapturer>();
            return capturer;
        }


        public void CaptureScreenshot(System.Action<Sprite> onCapturedCallback)
            => StartCoroutine(CaptureScreenshotCo(onCapturedCallback));
        IEnumerator CaptureScreenshotCo(System.Action<Sprite> onCapturedCallback)
        {
            Texture2D captureResult = null;
            CaptureScreenshot((Texture2D result) => { captureResult = result; });
            while (captureResult == null)
                yield return null;

            Rect wholeScreen = new Rect(0, 0, Screen.width * m_Cam.rect.width, Screen.height * m_Cam.rect.height);
            Sprite captureSprite = Sprite.Create(captureResult, wholeScreen, new Vector2(0.5f, 0.5f), 100.0f);
            onCapturedCallback.Invoke(captureSprite);
        }

        public void CaptureScreenshot(System.Action<Texture2D> onCapturedCallback)
            => StartCoroutine(CaptureScreenshotCo(onCapturedCallback));
        IEnumerator CaptureScreenshotCo(System.Action<Texture2D> onCapturedCallback)
        {
            if(m_Cam == null)
                m_Cam = GetComponent<Camera>();

            yield return new WaitForEndOfFrame();

            Rect cameraScreenRect = new Rect(
                    Screen.width * m_Cam.rect.x,
                    Screen.height * m_Cam.rect.y,
                    Screen.width * m_Cam.rect.width,
                    Screen.height * m_Cam.rect.height
                );

            Vector2Int regionInPixels = new Vector2Int(Mathf.FloorToInt(cameraScreenRect.width), Mathf.FloorToInt(cameraScreenRect.height));

            RenderTexture previousCamTarget = m_Cam.targetTexture;
            m_Cam.targetTexture = new RenderTexture(Screen.width, Screen.height, 32);
            m_Cam.Render();
            RenderTexture previousActive = RenderTexture.active;
            RenderTexture.active = m_Cam.targetTexture;

            Texture2D capture = new Texture2D(regionInPixels.x, regionInPixels.y, TextureFormat.RGBA32, false);
            capture.ReadPixels(cameraScreenRect, 0, 0, false);
            capture.Apply();

            Destroy(m_Cam.targetTexture);
            RenderTexture.active = previousActive;
            m_Cam.targetTexture = previousCamTarget;

            onCapturedCallback.Invoke(capture);
        }
    }
}
