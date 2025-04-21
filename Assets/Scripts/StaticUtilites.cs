using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class StaticUtilites 
{
    #region Math

    /// <summary>
    /// RoundToNPlaces(1.2345, 2) => 1.23
    /// </summary>
    public static float RoundToNPlaces(float val, int n)
    {
        // r was the first letter that came to mind
        float r = Mathf.Pow(10, n); 
        return Mathf.Round(val * r) / r;
    }

    #endregion

    #region Boolean

    public static bool CoinFlip()
    {
        return Random.value > 0.5f;
    }

    /// <summary>
    /// There is a a/b chance of returning true
    /// </summary>
    public static bool ChanceFraction(float a, float b)
    {
        return Random.value < a / b;
    }

    #endregion

    #region Vector

    public static float ManhattanDistance(Vector2 a, Vector2 b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    public static float ManhattanDistance(Vector3 a, Vector3 b)
    {
        return Mathf.Abs(a.x-b.x) + Mathf.Abs(a.y-b.y) + Mathf.Abs(a.z-b.z);
    }

    /// <summary>
    /// Returns highest value present in a vector
    /// </summary>
    public static float VectorMax(Vector3 v)
    {
        return Mathf.Max(Mathf.Max(v.x, v.y), v.z);
    }

    public static int ColorMax(Color32 color)
    {
        return Mathf.Max(Mathf.Max(color.r, color.g), color.b);
    }

    public static Vector3Int Vector3ToInt(Vector3 vector)
    {
        return new Vector3Int((int)vector.x, (int)vector.y, (int)vector.z);
    }

    /// <summary>
    /// Returns a vector with all positive components
    /// </summary>
    public static Vector3 VectorAbs(Vector3 vector)
    {
        return new Vector3(Mathf.Abs(vector.x), Mathf.Abs(vector.y), Mathf.Abs(vector.z));
    }

    /// <summary>
    /// Returns a vector with all positive components
    /// </summary>
    public static Vector4 VectorAbs(Vector4 vector)
    {
        return new Vector4(Mathf.Abs(vector.x), Mathf.Abs(vector.y), Mathf.Abs(vector.z), Mathf.Abs(vector.w));
    }

    /// <summary>
    /// Returns a vector with all positive components
    /// </summary>
    public static Vector4 VectorAbs(Color32 color)
    {
        return new Vector4(Mathf.Abs(color.r), Mathf.Abs(color.g), Mathf.Abs(color.b), Mathf.Abs(color.a));
    }

    #endregion

    #region Textures

    public static Texture2D TakeScreenshot(RenderTexture inputRenderTexture, bool mipmaps=false)
    {
        //currentRenderTexture = RenderTexture.active;

        //if(RenderTexture.active != inputRenderTexture)
        //    RenderTexture.active = inputRenderTexture;

        Texture2D texture2D = new Texture2D(inputRenderTexture.width, inputRenderTexture.height, TextureFormat.RGB24, mipmaps);

        return TakeScreenshot(inputRenderTexture, texture2D);

        return texture2D;
    }

    public static Texture2D TakeScreenshot(RenderTexture inputRenderTexture, Texture2D outputTexture, bool mipmaps=false)
    {
        if (outputTexture == null)
        {
            Debug.LogWarning("outputTexture not properly set");
            return TakeScreenshot(inputRenderTexture, mipmaps);
        }

        RenderTexture currentRenderTexture = RenderTexture.active;
        //if (RenderTexture.active != inputRenderTexture)
            RenderTexture.active = inputRenderTexture;

        outputTexture.ReadPixels(new Rect(0, 0, inputRenderTexture.width, inputRenderTexture.height), 0, 0);
        outputTexture.Apply();

        RenderTexture.active = currentRenderTexture;

        return outputTexture;
    }

    public static Texture2D ResizeTexture(Texture2D source, int maxWidth = 128, bool mipmaps=false)
    {
        if (source == null || maxWidth <= 0)
            return null;

        int width = Mathf.Min(maxWidth, source.width);
        float aspectRatio = (float)source.height / source.width;
        int height = Mathf.RoundToInt(width * aspectRatio);

        RenderTexture rt = new RenderTexture(width, height, 24);
        RenderTexture.active = rt;
        Graphics.Blit(source, rt);

        Texture2D result = new Texture2D(width, height, source.format, mipmaps);
        result.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        result.Apply();

        RenderTexture.active = null;
        rt.Release();

        result = ResizeTextureHeight(result, maxWidth);

        return result;
    }

    public static Texture2D ResizeTextureHeight(Texture2D source, int maxHeight = 128, bool mipmaps = false)
    {
        if (source == null || maxHeight <= 0)
            return null;

        int height = Mathf.Min(maxHeight, source.height);
        float aspectRatio = (float)source.width / source.height;
        int width = Mathf.RoundToInt(height * aspectRatio);

        RenderTexture rt = new RenderTexture(width, height, 24);
        RenderTexture.active = rt;
        Graphics.Blit(source, rt);

        Texture2D result = new Texture2D(width, height, source.format, mipmaps);
        result.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        result.Apply();

        RenderTexture.active = null;
        rt.Release();

        return result;
    }

    /// <summary>
    /// Returns pixel on texture by percent coordinates.
    /// </summary>
    /// <param name="tex"></param>
    /// <param name="x_percent">Position between 0 and 1</param>
    /// <param name="y_percent">Position between 0 and 1</param>
    /// <returns>Vector4 (color)</returns>
    public static Vector4 GetPixelFromTexture(Texture2D tex, float x_percent, float y_percent)
    {
        return tex.GetPixel((int)(tex.width * x_percent), (int)(tex.height * y_percent));
    }

    /// <summary>
    /// Returns the average color from a texture
    /// </summary>
    /// <param name="tex">Texture to average</param>
    /// <param name="x_slices">how many vertical slices the sampling will take</param>
    /// <param name="y_slices">how many horizontal slices the sampling will take</param>
    /// <returns></returns>
    public static Color AverageTextureColor(Texture2D tex)
    {
        Color[] colors = tex.GetPixels();

        Vector4 sum = Vector4.zero;

        foreach (Color c in colors)
        {
            sum += (Vector4)c;
        }

        return sum / colors.Length;

    }

    /// <summary>
    /// Returns the average color from a texture
    /// </summary>
    /// <param name="tex">Texture to average</param>
    /// <param name="x_slices">how many vertical slices the sampling will take</param>
    /// <param name="y_slices">how many horizontal slices the sampling will take</param>
    /// <returns></returns>
    public static Color AverageTextureColor_BySlices(Texture2D tex, int x_slices=100, int y_slices=100)
    {
        Vector4 sum=Vector4.zero;
        float total = x_slices * y_slices;

        for (int x = 0; x < x_slices; x++)
        {
            for (int y = 0; y < y_slices; y++)
            {
                float x_pct = (float)x / (float)x_slices;
                float y_pct = (float)y / (float)y_slices;

                sum += StaticUtilites.GetPixelFromTexture(tex, x_pct, y_pct);
                
            }
        }
        return sum/total;

    }

    public static Color GetRandomColorFromTexture(Texture2D tex)
    {
        return tex.GetPixelBilinear(Random.value, Random.value);
    }



    #endregion

    #region Color

    public static Color InvertColor(Color color, bool invertAlpha=false)
    {
        float a = color.a;
        Color output = Color.white - color;
        if(!invertAlpha)
            output.a = a;
        return output;
    }

    public static int ColorDifference(Color32 color1, Color32 color2)
    {
        int sum = Mathf.Abs(color1.r - color2.r)
            + Mathf.Abs(color1.g - color2.g)
            + Mathf.Abs(color1.b - color2.b)
            + Mathf.Abs(color1.a - color2.a);
        return sum;
    }

    #endregion
}
