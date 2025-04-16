using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using UnityEngine.UI;
using TMPro;

public class AverageColorDebugger : MonoBehaviour
{
    [SerializeField] RenderTexture targetRenderTexture;
    [SerializeField] RenderTexture currentRenderTexture;

    [SerializeField] RawImage compareStateImage;
    [SerializeField] RawImage differenceImage;
    [SerializeField] RawImage averageImage;

    private Texture2D targetTexture;
    private Texture2D currentTexture;
    private Texture2D compareTexture;


    // Update is called once per frame
    void Update()
    {
        targetTexture = StaticUtilites.TakeScreenshot(targetRenderTexture, targetTexture);
        currentTexture = StaticUtilites.TakeScreenshot(currentRenderTexture, currentTexture);

        if (compareTexture == null && Time.time > 0.25f)
            RecordCurrentState();

        if (compareTexture == null)
            return;

        MaskPixelDifferences(targetTexture, compareTexture, currentTexture);
        averageImage.texture = AverageMaskedPixels(MaskPixelDifferences(targetTexture, compareTexture, currentTexture));
    }

    [Button]
    public void RecordCurrentState()
    {
        Debug.Log("recording current state");

        if (compareTexture == null)
            compareTexture = new Texture2D(targetTexture.width, targetTexture.height, targetTexture.format, true);

        Graphics.CopyTexture(targetTexture, compareTexture);
        compareTexture.Apply();
        compareStateImage.texture = compareTexture;

        return;
    }

    /// <summary>
    /// Returns a texture with filled pixels when a != b
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public Texture2D MaskPixelDifferences(Texture2D source, Texture2D a, Texture2D b)
    {
        // improve this by not calling .GetPixels unnecessarily many times
        var sourceColors = source.GetPixelData<Color>(1);
        var a_colors = a.GetPixelData<Color>(1);
        var b_colors = b.GetPixelData<Color>(1);

        Color[] output_colors = new Color[sourceColors.Length];

        bool hasDifferences = false;
        for (int i = 0; i < sourceColors.Length; i++)
        {
            if (a_colors[i] == b_colors[i])
                output_colors[i] = Color.clear;
            else
            {
                output_colors[i] = sourceColors[i];
                hasDifferences = true;

            }
        }
        Debug.Log(hasDifferences);

        Texture2D output = new Texture2D(source.width, source.height, source.format, false);
        output.SetPixels(output_colors);
        output.Apply();
        differenceImage.texture = output;
        return output;
    }

    public Texture2D AverageMaskedPixels(Texture2D maskedTexture)
    {
        // ignore clear pixels
        var colors = maskedTexture.GetPixelData<Color>(1);
        Vector4 sum = Vector4.zero;
        int count = 0;
        for (int i = 0; i < colors.Length; i++)
        {
            if (colors[i].a <= 0)
                continue;

            sum += (Vector4)colors[i];
            count++;
        }

        Color avg = sum / (float)count;
        avg.a = 1;

        for (int i = 0; i < colors.Length; i++)
        {
            if (colors[i].a <= 0)
                continue;

            colors[i] = avg;
        }

        Texture2D result = new Texture2D(maskedTexture.width, maskedTexture.height);
        result.SetPixelData<Color>(colors,1);
        //result.SetPixels(colors);
        result.Apply();
        return result;
    }
}
