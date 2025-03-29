using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;
public class CameraManager : Singleton<CameraManager>
{
    [Header("Settings")]
    public int resolution = 96;
    [SerializeField] private int precision = 2;


    public static float CameraHeightWorldSpace => Instance._camera.orthographicSize * 2;
    public static float CameraWidthWorldSpace => CameraHeightWorldSpace * Instance._camera.aspect;

    [HideInInspector] public Camera _camera;
    [HideInInspector] public RenderTexture renderTexture => _camera.targetTexture;

    [SerializeField][ReadOnly] private Texture2D currentState;
    [SerializeField][ReadOnly] private Color[] currentStateColors;

    [Header("Material")]
    [SerializeField] Material differenceMaterial;

    [SerializeField]
    private RawImage debugImage;

    [SerializeField] [Layer] public int currentStateLayer;
    [SerializeField] [Layer] public int candidateLayer;

    [SerializeField]
    private LayerMask currentStateLayerMask, candidateLayerMask, everythingLayerMask;

    private Texture2D sc;
    //private Vector3 colorDifferenceSum;
    private int colorDifferenceSum;
    private Vector3 targetColor,currentColor,newColor;
    private Vector3 difference,newColorDifference;

    // calculation variables
    private Vector4[] targetColors;
    private Color[] screenshotolors;
    private int index_offset = 0;
    private Color bg_color;


    // Start is called before the first frame update
    void Start()
    {
        _camera = GetComponent<Camera>();

        currentState = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGBA32, false);

        if (!ShapeManager.Instance.AverageColorMask)
        { 
            _camera.cullingMask = everythingLayerMask;
            _camera.backgroundColor = bg_color;
        }

        EvolutionManager.Instance.OnRefreshImage.AddListener(UpdateBackgroundColors);
        EvolutionManager.Instance.OnRefreshImage.AddListener(UpdateSizeToMatchImage);
        EvolutionManager.Instance.OnRefreshImage.AddListener(GetTargetPixelColors);

        ShapeManager.OnShapeSelected.AddListener(OnShapeCreated);
    }

    void OnShapeCreated()
    {
        //sc = TakeScreenshot(sc);
        index_offset = (index_offset + 1) % precision;
        //differenceMaterial.SetTexture("_Current_State", currentState);

        _camera.backgroundColor = bg_color;
        _camera.cullingMask = currentStateLayerMask;
        currentState = TakeScreenshot(currentState);
        currentStateColors = currentState.GetPixels();
        //_camera.cullingMask = candidateLayerMask;
    }

    public int CalculateScore()
    {
        //double t = Time.realtimeSinceStartupAsDouble;

        sc = TakeScreenshot(sc);
        //colorDifferenceSum = Vector3.zero;
        colorDifferenceSum = 0;

        screenshotolors = sc.GetPixels();


        for (int i = index_offset; i < targetColors.Length; i += precision)
        {
            difference = StaticUtilites.VectorAbs(targetColors[i] - (Vector4)screenshotolors[i]);
            int worstColor = (int)(StaticUtilites.VectorMax(difference) * 256);
            worstColor *= worstColor; // square it >:)
            colorDifferenceSum += worstColor;
        }

        return colorDifferenceSum * precision;
        //colorDifferenceSum *= 256;
        //return (int)(colorDifferenceSum.x + colorDifferenceSum.y + colorDifferenceSum.z);
    }

    public int CalculateScore(Shape shape)
    {
        if(shape.score > -1)
            return shape.score;

        #region get screenshot colors
        shape.sprite.enabled = true;
        if ( shape.colorMode == ShapeColorMode.AverageColorFromTexture && 
             ShapeManager.Instance.AverageColorMask && 
            (ShapeManager.Instance.ApplyAverageToVariants || !shape.hasSetColor))
        {
            screenshotolors = GetShapeColorsAsAverageFromTarget(shape);
        }
        else
        {
            _camera.cullingMask = everythingLayerMask;
            _camera.backgroundColor = bg_color;
            
            sc = TakeScreenshot(sc);
            screenshotolors = sc.GetPixels();
        }
        #endregion

        colorDifferenceSum = 0;
        for (int i = index_offset; i < targetColors.Length; i += precision)
        {
            difference = StaticUtilites.VectorAbs(targetColors[i] - (Vector4)screenshotolors[i]);
            int worstColor = (int)(StaticUtilites.VectorMax(difference) * 256);
            worstColor *= worstColor; // square it >:)
            colorDifferenceSum += worstColor;
        }

        int score = colorDifferenceSum * precision;
        shape.score = score;
        shape.sprite.enabled = false;
        return score;
    }


    public Texture2D TakeScreenshot(Texture2D outputTexture)
    {
        if(outputTexture == null)
        {
            Debug.Log("initalizing screenshot texture");
            outputTexture = StaticUtilites.TakeScreenshot(renderTexture);
        }
            
        _camera.Render();
        outputTexture = StaticUtilites.TakeScreenshot(renderTexture, outputTexture);
        return outputTexture;
    }

    /// <summary>
    /// Find average color of area that shape encapsulates
    /// also returns screen pixels 
    /// </summary>
    /// <param name="currentShape"></param>
    /// <returns></returns>
    private Color[] GetShapeColorsAsAverageFromTarget(Shape currentShape)
    {
        /*
         * potential optimization, make the color arrays smaller to account for precision
         */

        float currentOpacity = currentShape.a;
        currentShape.sprite.color = Color.white;
        //Debug.Log(currentShape.sprite.color);
        _camera.backgroundColor = Color.clear;
        _camera.cullingMask = candidateLayerMask;

        sc = TakeScreenshot(sc);
        screenshotolors = sc.GetPixels();

        /*
        Texture2D debugoutput = new Texture2D(sc.width, sc.height, sc.format, false);
        debugoutput.SetPixels(screenshotolors);
        debugoutput.Apply();
        debugImage.texture = debugoutput; */

        Vector4 sum = Vector4.zero;
        float count = 0;
        Vector2Int topLeftIndex = new Vector2Int(sc.width, sc.height); /* new optimization: make this a function, do a big binary-esc search */
        Vector2Int bottomRightIndex = new Vector2Int(0,0);

        //find when screenshot is different from current
        for (int i = index_offset; i < screenshotolors.Length; i+=precision)
        {
            if (screenshotolors[i].r > 0)
            {
                int x = i % sc.width;
                int y = i / sc.height;
                topLeftIndex.x = Mathf.Min(topLeftIndex.x, x);
                topLeftIndex.y = Mathf.Min(topLeftIndex.y, y);
                bottomRightIndex.x = Mathf.Max(bottomRightIndex.x, x);
                bottomRightIndex.y = Mathf.Max(bottomRightIndex.y, y);

                sum += targetColors[i] * screenshotolors[i].a;
                count += screenshotolors[i].a;
            }
        }
        Color avg_color = (Color)(sum / count);
        avg_color.a = currentShape.sprite.color.a;

        // Apply color with avg 
        for (int i = index_offset; i < screenshotolors.Length; i += precision)
        {
            if (screenshotolors[i].r == 0)
                screenshotolors[i] = currentStateColors[i];
            else if (currentStateColors[i].r == 1)
                screenshotolors[i] = avg_color;
            else
                screenshotolors[i] = Color.Lerp(currentStateColors[i], avg_color, screenshotolors[i].r);
        }
        /*
        for(int x = topLeftIndex.x; x < bottomRightIndex.x; x++)
        {
            for(int y = topLeftIndex.y; y< bottomRightIndex.y; y++)
            {
                int i = y * sc.width + x;
                if (screenshotolors[i].r == 0)
                    screenshotolors[i] = currentStateColors[i];
                else if (currentStateColors[i].r == 1)
                    screenshotolors[i] = avg_color;
                else
                    screenshotolors[i] = Color.Lerp(currentStateColors[i], avg_color, screenshotolors[i].r);
            }
        } */


        avg_color.a = currentOpacity;   
        currentShape.SetColor(avg_color);
        return screenshotolors;
    }

    public void UpdateBackgroundColors()
    {
        bg_color = StaticUtilites.AverageTextureColor(EvolutionManager.Instance.TextureToSimulate);

        Camera[] cameras = FindObjectsOfType<Camera>();
        foreach(Camera cam in cameras) 
            if(cam != _camera)
                cam.backgroundColor = bg_color;

        OnShapeCreated();
    }

    private void UpdateSizeToMatchImage()
    {
        differenceMaterial.SetTexture("_Texture", EvolutionManager.Instance.TextureToSimulate);

        renderTexture.Release();
        renderTexture.width = EvolutionManager.Instance.TextureToSimulate.width;
        renderTexture.height = EvolutionManager.Instance.TextureToSimulate.height;
        renderTexture.Create();
        //renderTexture.format = (RenderTextureFormat)(System.Enum.Parse(typeof(RenderTextureFormat), EvolutionManager.Instance.TextureToSimulate.format.ToString()));
    }

    /// <summary>
    /// Sets targetColors[] with pixels from TextureToSimilate
    /// </summary>
    private void GetTargetPixelColors()
    {
        Color[] temp = EvolutionManager.Instance.TextureToSimulate.GetPixels();
        targetColors = new Vector4[temp.Length];

        for(int i=0; i<temp.Length; i++)
        {
            targetColors[i] = (Vector4)(temp[i]);
        }
    }

    #region obsolete

    [System.Obsolete]
    void SetRandomBackgroundColor()
    {
        _camera.backgroundColor = new Color(Random.value, Random.value, Random.value);
    }
    #endregion

}
