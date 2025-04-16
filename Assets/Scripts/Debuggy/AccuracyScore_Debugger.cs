using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using UnityEngine.UI;
using TMPro;

public class AccuracyScore_Debugger : MonoBehaviour
{
    [SerializeField] RenderTexture targetTexture;
    [SerializeField] RenderTexture compareTexture;

    [SerializeField] RawImage differenceImage;
    [SerializeField] RawImage outputImage;
    [SerializeField] TMP_Text scoreoutput_text;

    // Update is called once per frame
    void Update()
    {
        //if (Input.GetKeyDown(KeyCode.Space))
            CompareScore();
    }

    [Button]
    public void CompareScore()
    {
        Texture2D targetTextureTexture = StaticUtilites.TakeScreenshot(targetTexture);
        Texture2D compareTextureTexture = StaticUtilites.TakeScreenshot(compareTexture);
        var targetColors = targetTextureTexture.GetPixelData<Color>(1);
        var compareColors = compareTextureTexture.GetPixelData<Color>(1);

        // Difference
        Vector3 differenceSum = Vector3.zero;
        Color[] differenceColors = new Color[compareColors.Length];
        for(int i=0; i<targetColors.Length; i++)
        {
            Vector4 difference = StaticUtilites.VectorAbs((Vector4)targetColors[i] - (Vector4)compareColors[i]);
            differenceSum += (Vector3) difference;
            differenceColors[i] = (Color) difference;
            differenceColors[i].a = 1;
        }

        Texture2D differenceTexture = new Texture2D(targetTexture.width, targetTexture.height);
        differenceTexture.filterMode=FilterMode.Point;
        differenceTexture.anisoLevel = 0;
        differenceTexture.SetPixels(differenceColors);
        differenceTexture.Apply();

        differenceImage.texture = differenceTexture;

        differenceSum *= 256;
        Vector3Int differenceSumInt = StaticUtilites.Vector3ToInt(differenceSum);
        scoreoutput_text.text = (differenceSumInt.x + differenceSumInt.y + differenceSumInt.z).ToString(); 
    }
}
