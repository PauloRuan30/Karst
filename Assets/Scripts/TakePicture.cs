using System;
using System.IO;
using UnityEngine;



public class TakePicture : MonoBehaviour
{
    static byte[] lastPictureSaved;

    static int index = 0;

    public static bool SaveImages = false;

    public static void SavePicture(Camera cam, LineRenderer currentDrawing) {

        GameObject tip = GameObject.Find("tip");
        // Calculate bounds of the drawing
        Bounds bounds = new Bounds(currentDrawing.GetPosition(0), Vector3.zero);
        for (int i = 1; i < currentDrawing.positionCount; i++)
        {
            bounds.Encapsulate(currentDrawing.GetPosition(i));
        }

        float size = Mathf.Max(bounds.size.x, bounds.size.y) / 2f + 0.2f;
        cam.orthographicSize = size;
        cam.transform.position = tip.transform.position - tip.transform.forward*10; // Move camera back a bit
        //cam.transform.rotation = tip.transform.rotation;
        cam.transform.LookAt(bounds.center);


        RenderTexture currentRT = RenderTexture.active;
        RenderTexture.active = cam.targetTexture;

        cam.Render();

        Texture2D Image = new Texture2D(cam.targetTexture.width, cam.targetTexture.height);
        Image.ReadPixels(new Rect(0, 0, cam.targetTexture.width, cam.targetTexture.height), 0, 0);
        Image.Apply();
        RenderTexture.active = currentRT;

        byte[] bytes = Image.EncodeToJPG();
        lastPictureSaved = bytes;

        if (SaveImages)
        {
            // Save to file
            string currentTime = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string filePath = Path.Combine(Application.persistentDataPath, "drawing_" + currentTime + ".jpg");
            File.WriteAllBytes(filePath, bytes);
            Debug.Log("Image saved to: " + filePath);
        }

        Destroy(Image);
    }



    public static byte[] GetLastPictureAsData() {
        return lastPictureSaved;
    }
}
