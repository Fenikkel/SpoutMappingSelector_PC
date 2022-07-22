using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Runtime.InteropServices;
using System;

public static class Fenikkel
{

#if UNITY_ANDROID && !UNITY_EDITOR
        private static readonly AndroidJavaObject Vibrator =
    new AndroidJavaClass("com.unity3d.player.UnityPlayer")// Get the Unity Player.
    .GetStatic<AndroidJavaObject>("currentActivity")// Get the Current Activity from the Unity Player.
    .Call<AndroidJavaObject>("getSystemService", "vibrator");// Then get the Vibration Service from the Current Activity.
#endif



    public static float RemapValues(float value, float currentLow, float currentHigh, float newLow, float newHigh)
    {
        float newValue = newLow + (value - currentLow) * (newHigh - newLow) / (currentHigh - currentLow);

        return newValue;
    }

    #region Random in area

    public static Vector3 GetPointInsideBoxCollider(BoxCollider boxCollider)
    {
        Vector3 extents = boxCollider.size / 2f;
        Vector3 point = new Vector3(
            UnityEngine.Random.Range(-extents.x, extents.x),
            UnityEngine.Random.Range(-extents.y, extents.y),
            UnityEngine.Random.Range(-extents.z, extents.z)
        ) + boxCollider.center;

        return boxCollider.transform.TransformPoint(point);
    }

    public static Vector3 GetPointInsideBoxCollider2D(BoxCollider2D boxCollider2d)
    {
        Vector2 extents = boxCollider2d.size / 2f;
        Vector2 point = new Vector2(
            UnityEngine.Random.Range(-extents.x, extents.x),
            UnityEngine.Random.Range(-extents.y, extents.y)
        ) + boxCollider2d.offset;

        return boxCollider2d.transform.TransformPoint(point);
    }

    public static Vector3 GetRandomPointInsideSphere(GameObject sphere) //The sphere must have a sphere collider
    {
        SphereCollider sphereCollider = sphere.GetComponent<SphereCollider>();

        if (sphereCollider == null)
        {
            Debug.LogWarning("Sphere gameobject doesn't have an SphereCollider. Returning a Vector zero");
            return Vector3.zero;
        }

        float[] coordenades = new float[] { sphere.transform.localScale.x, sphere.transform.localScale.y, sphere.transform.localScale.z };
        float coorMax = Mathf.Max(coordenades);

        float sphereSize = sphereCollider.radius * coorMax;

        Vector3 spherePos = sphere.transform.position + sphereCollider.center;

        return (UnityEngine.Random.insideUnitSphere * sphereSize) + spherePos;
    }

    #endregion

    #region Cursor
    [Flags]
    public enum MouseEventFlags
    {
        LeftDown = 0x00000002,
        LeftUp = 0x00000004,
        MiddleDown = 0x00000020,
        MiddleUp = 0x00000040,
        Move = 0x00000001,
        Absolute = 0x00008000,
        RightDown = 0x00000008,
        RightUp = 0x00000010
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MousePoint
    {
        public int X;
        public int Y;

        public MousePoint(int x, int y)
        {
            X = x;
            Y = y;
        }
    }

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetCursorPos(out MousePoint lpMousePoint);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)] //?
    private static extern bool SetCursorPos(int X, int Y);

    [DllImport("user32.dll")]
    private static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);

    /*
    Unity screen coordinates: (0, 0) -> Bot_Left of the screen
    Windows screen coordinates: (0, 0) -> Top-Left of the screen
    So if we invert the "Y" (Screen.height - Y), we transform the point from Windows coordinates to Unity coordinates.
    */

    public static MousePoint GetCursorPosition()
    {
        MousePoint currentMousePoint;
        var gotPoint = GetCursorPos(out currentMousePoint);
        if (!gotPoint) { currentMousePoint = new MousePoint(0, 0); }
        currentMousePoint.Y = Screen.height - currentMousePoint.Y;
        return currentMousePoint;
    }

    public static void SetCursorPosition(int x, int y)
    {
        SetCursorPos(x, Screen.height - y);
    }

    public static void SetCursorPosition(Vector2 pos)
    {
        SetCursorPos((int)pos.x, Screen.height - (int)pos.y);
    }

    public static void SetCursorPosition(MousePoint point)
    {
        SetCursorPos(point.X, Screen.height - point.Y);
    }


    public static void MouseEvent(MouseEventFlags value)
    {
        MousePoint position = GetCursorPosition();

        mouse_event
            ((int)value,
             position.X,
             position.Y,
             0,
             0)
            ;
    }
    #endregion

    #region UI
    public static bool IsPointerOverUIObject() //Check if there is an UI element under the mouse
    {
        return UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
    }

    public static bool IsUIElementAtPoint(int u, int v) //Check if at that point of the canvas there is an UI Element
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(u, v);
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);

        if (0 < results.Count)
        {
            Debug.Log(results[0].gameObject.tag);
        }
        else
        {
            Debug.Log("Nothing");
        }


        return 0 < results.Count;

    }

    public static GameObject GetUIElementAtPoint(int u, int v) //Get the UI element at point
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(u, v);
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);

        if (0 < results.Count)
        {
            return results[0].gameObject; //The first element on the list seems it is the last element in the hirarchy, so if you have a button with a text, it will return the text
        }
        else
        {
            return null;
        }
    }

    public static GameObject[] GetAllUIElementAtPoint(int u, int v) //Get all UI gameobjects at canvas point
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(u, v);
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);

        GameObject[] retArray = new GameObject[results.Count];

        for (int i = 0; i < results.Count; i++)
        {
            retArray[i] = results[i].gameObject;
        }

        return retArray;
    }

    #endregion

    #region Triangles
    public static bool Is2dClockWise(Vector2 pointA, Vector2 pointB, Vector2 pointC) //Cheaper, but don't tanke into accound the camera point of view
    {
        float result = (pointB.x - pointA.x) * (pointC.y - pointA.y) - (pointC.x - pointA.x) * (pointB.y - pointA.y);
        //Debug.Log(result);

        if (0.0f < result) //Counterclockwise
        {
            return false;
        }
        else if (result < 0.0f) //Clockwise
        {
            return true;
        }
        else if (result == 0.0f) //Collinear (They are forming a line)
        {
            return true; // we dont care the result
        }
        return true;
    }

    public static bool Is3dClockWise(Vector3 pointA, Vector3 pointB, Vector3 pointC) //Take into accound the camera point of view                                                                              
    {
        //surfaceNormal = (pointB - pointA) x (pointC - pointA)
        Vector3 pointBA = pointB - pointA;
        Vector3 pointCA = pointC - pointA;
        Vector3 crossProduct = Vector3.Cross(pointBA, pointCA);
        Vector3 surfaceNormal = crossProduct;

        //w = sufraceNormal . (oneOfTheTriangleVertices - cameraPosition)
        Vector3 cameraVectorDirection = pointA - Camera.main.transform.position;
        float w = Vector3.Dot(surfaceNormal, cameraVectorDirection); //Dot product = scalar product;
                                                                     //Debug.Log(w);

        //If the coordenate system the z and y are swapped, the w values meaning may be inverted

        if (0.0f < w) //Counterclockwise
        {
            return false;
        }
        else if (w < 0.0f) //Clockwise
        {
            return true;
        }
        else if (w == 0.0f) //Collinear (They are forming a line)
        {
            return true;
        }
        return true;
    }
    #endregion

    #region Video
    public static void ClearOutRenderTexture(RenderTexture renderTexture)
    {
        RenderTexture rt = RenderTexture.active;
        RenderTexture.active = renderTexture;
        GL.Clear(true, true, Color.clear);
        RenderTexture.active = rt;
    }
    #endregion

    #region Images

    public static void ConvertToGrayscale(Texture2D image) // Reaad/Write of the image must be checked and also the "Texture type" in "Sprite(Texture2D/UI)" 
                                                           //MUST BE .PNG
    {
        Color32[] pixels = image.GetPixels32();

        Color32 currentPixel;
        int currentIndex = -1;
        int p = -1;
        int blue = -1;
        int green = -1;
        int red = -1;
        float luminance = -1.0f;
        Color c;

        for (int x = 0; x < image.width; x++)
        {
            for (int y = 0; y < image.height; y++)
            {
                currentIndex = x + y * image.width;

                currentPixel = pixels[currentIndex];
                p = ((256 * 256 + currentPixel.r) * 256 + currentPixel.b) * 256 + currentPixel.g;
                blue = p % 256;
                p = Mathf.FloorToInt(p / 256);
                green = p % 256;
                p = Mathf.FloorToInt(p / 256);
                red = p % 256;
                luminance = (0.2126f * red / 255f) + 0.7152f * (green / 255f) + 0.0722f * (blue / 255f);
                c = new Color(luminance, luminance, luminance, 1);
                image.SetPixel(x, y, c);
            }
        }

        image.Apply(false);

        //If we desire save the image
        /*
        var bytes = graph.EncodeToPNG();
        string savePath = Application.dataPath + "ImageSaveTest.png";
        Debug.Log("Saved to: " + savePath);
        System.IO.File.WriteAllBytes(savePath, bytes);
        */
    }

    #endregion

    #region Vibration
    static void KyVibrator()
    {
        // Trick Unity into giving the App vibration permission when it builds.
        // This check will always be false, but the compiler doesn't know that.
#if UNITY_ANDROID && !UNITY_EDITOR
        if (Application.isEditor) Handheld.Vibrate();
#endif
    }

    public static void Vibrate(long milliseconds)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        Vibrator.Call("vibrate", milliseconds);
#endif
    }

    public static void Vibrate(long[] pattern, int repeat)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        Vibrator.Call("vibrate", pattern, repeat);
#endif
    }
#endregion
}
