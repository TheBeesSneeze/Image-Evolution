using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
public class CameraManager : Singleton<CameraManager>
{

    public static float CameraHeightWorldSpace => Instance._camera.orthographicSize * 2;
    public static float CameraWidthWorldSpace => CameraHeightWorldSpace * Instance._camera.aspect;

    [HideInInspector] public Camera _camera;
    [HideInInspector] public RenderTexture renderTexture => _camera.targetTexture;

    [SerializeField][ReadOnly] private Texture2D currentState;
    [SerializeField][ReadOnly] private Color[] currentStateColors;

    [Header("Material")]
    [SerializeField] Material differenceMaterial;

    [Layer]
    private LayerMask layer;

    private Texture2D sc;
    private Vector3 colorDifferenceSum;
    private Vector3 targetColor,currentColor,newColor;
    private Vector3 difference,newColorDifference;

    // calculation variables
    private Vector4[] targetColors;
    private Color[] screenshotolors;
    private int index_offset = 0;
    private int precision = 4;
    private Color bg_color;

    // Start is called before the first frame update
    void Start()
    {
        _camera = GetComponent<Camera>();
        layer = gameObject.layer;

        currentState = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGBA32, false);

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

        currentState = TakeScreenshot(currentState);
        currentStateColors = currentState.GetPixels();

    }

    public int CalculateScore()
    {
        //double t = Time.realtimeSinceStartupAsDouble;

        sc = TakeScreenshot(sc);
        colorDifferenceSum = Vector3.zero;

        screenshotolors = sc.GetPixels();


        for (int i = index_offset; i < targetColors.Length; i += precision)
        {
            difference = StaticUtilites.VectorAbs(targetColors[i] - (Vector4)screenshotolors[i]);
            colorDifferenceSum += difference;
        }

        return (int)(StaticUtilites.VectorMax(colorDifferenceSum) * 256);
        //colorDifferenceSum *= 256;
        //return (int)(colorDifferenceSum.x + colorDifferenceSum.y + colorDifferenceSum.z);
    }

    public int CalculateScore(Shape shape)
    {
        /*
         * TODO:
         * change the culling layer in runtime
         * get colors for just the shape literally nothing else
         * change logic to check for if a != 0 => do averaging stuff
         * 
         */
        if(shape.score != -1)
            return shape.score;

        shape.gameObject.SetActive(true);
        sc = TakeScreenshot(sc);

        if (ShapeManager.Instance.AverageColorMask && (ShapeManager.Instance.ApplyAverageToVariants || !shape.hasSetColor))
        {
            float currentOpacity = shape.a;
            shape.sprite.color = StaticUtilites.InvertColor(bg_color); // bg color bc its probably the most different color we can get :p
            screenshotolors = GetShapeColorsAsAverageFromTarget(sc.GetPixels(), shape);
            shape.sprite.color = shape.sprite.color.WithAlpha(currentOpacity);
        }
        else
            screenshotolors = sc.GetPixels();

        colorDifferenceSum = Vector3.zero;
        for (int i = index_offset; i < targetColors.Length; i+= precision)
        {
            difference = StaticUtilites.VectorAbs(targetColors[i] - (Vector4)screenshotolors[i]);
            colorDifferenceSum += difference;
        }

        int score = (int)(StaticUtilites.VectorMax(colorDifferenceSum) * 256);
        shape.score = score;
        shape.gameObject.SetActive(false);

        return score;
        //colorDifferenceSum *= 256;
        //return (int)(colorDifferenceSum.x + colorDifferenceSum.y + colorDifferenceSum.z);
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

    private Color[] GetShapeColorsAsAverageFromTarget(Color[] colorsNoBorax, Shape currentShape)
    {

        Vector4 sum = Vector4.zero;
        int count = 0;
        //find when screenshot is different from current
        for (int i = index_offset; i < colorsNoBorax.Length; i+=precision)
        {
            if (colorsNoBorax[i] != currentStateColors[i])
            {
                sum += targetColors[i];
                count++;
            }
        }
        Color avg_color = (Color)(sum / (float)count);
        avg_color.a = currentShape.sprite.color.a;

        // Apply color with avg 
        for (int i = index_offset; i < colorsNoBorax.Length; i+= precision)
        {
            if (colorsNoBorax[i] != currentStateColors[i])
            {
                colorsNoBorax [i] = Color.Lerp(currentStateColors[i],avg_color,avg_color.a);
            }
        }

        currentShape.SetColor(avg_color);
        return colorsNoBorax;
    }

    public void UpdateBackgroundColors()
    {
        bg_color = StaticUtilites.AverageTextureColor(EvolutionManager.Instance.TextureToSimulate);

        Camera[] cameras = FindObjectsOfType<Camera>();
        foreach(Camera cam in cameras) 
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
    public int CalculateScore_OLD()
    {
        double t = Time.realtimeSinceStartupAsDouble;

        sc = TakeScreenshot(sc);
        colorDifferenceSum = Vector3.zero;

        Debug.Log("screenshot: " + (Time.realtimeSinceStartupAsDouble - t));
        t = Time.realtimeSinceStartupAsDouble;

        for (int x = 0; x < CandidateManager.x_slices; x++)
        {
            for (int y = 0; y < CandidateManager.y_slices; y++)
            {
                float x_pct = (((float)x + (float)t) / CandidateManager.x_slices) % 1;
                float y_pct = (((float)y + (float)t) / CandidateManager.y_slices) % 1;

                //DrawVector2.Point(x_pct * CameraManager.CameraWidthWorldSpace, y_pct * CameraManager.CameraHeightWorldSpace, 1);

                targetColor = StaticUtilites.GetPixelFromTexture(EvolutionManager.Instance.TextureToSimulate, x_pct, y_pct); // todo: cache pixels and create static util function so you dont need to getpixelfromtexture every time
                //currentColor = StaticUtilites.GetPixelFromTexture(currentState, x_pct, y_pct);
                newColor = StaticUtilites.GetPixelFromTexture(sc, x_pct, y_pct);

                //sum += Vector3.Distance(targetColor, newColor);

                Vector3 difference = targetColor - newColor;
                difference = StaticUtilites.VectorAbs(difference);

                colorDifferenceSum += difference;
            }
        }

        Debug.Log(Time.realtimeSinceStartupAsDouble - t);
        t = Time.realtimeSinceStartupAsDouble;

        Color[] colorstest = sc.GetPixels();
        Debug.Log(colorstest.Length);
        Debug.Log("getpixels" + (Time.realtimeSinceStartupAsDouble - t));



        colorDifferenceSum *= 256;

        return (int)(colorDifferenceSum.x + colorDifferenceSum.y + colorDifferenceSum.z);
    }

    [System.Obsolete]
    void SetRandomBackgroundColor()
    {
        _camera.backgroundColor = new Color(Random.value, Random.value, Random.value);
    }
    #endregion

}
