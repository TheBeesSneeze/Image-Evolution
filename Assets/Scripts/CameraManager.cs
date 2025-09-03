using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;
public class CameraManager : Singleton<CameraManager>
{
    [Header("Settings")]
    public int resolution = 96;
    [SerializeField] private int precision = 2;
    [SerializeField] private bool useMaxColor = true;


    public static float CameraHeightWorldSpace => Instance._camera.orthographicSize * 2;
    public static float CameraWidthWorldSpace => CameraHeightWorldSpace * Instance._camera.aspect;

    [HideInInspector] public Camera _camera;
    [HideInInspector] public RenderTexture renderTexture => _camera.targetTexture;

    public bool GenerateMipMaps = false;
    [ShowIf("GenerateMipMaps")]
    public int MipMapLevel = 0;

    private Texture2D currentState;
    private NativeArray<Color32> currentStateColors;

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
    private Vector3 newColorDifference;

    // calculation variables
    [HideInInspector] public Color32[] targetColors; // move to evolution manager?
    private NativeArray<Color32> screenshotolors;
    private int index_offset = 0;
    private Color32 bg_color;

    [SerializeField] ComputeShader shader;
    int kernel;
    ComputeBuffer resultBuffer;
    uint[] resultArray = new uint[1];


    // Start is called before the first frame update
    void Start()
    {
        if (!GenerateMipMaps)
            MipMapLevel = 0;

        kernel = shader.FindKernel("CSMain");
        resultBuffer = new ComputeBuffer(1, sizeof(uint));

        _camera = GetComponent<Camera>();

        //currentState = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGBA32, GenerateMipMaps);
        currentState = new Texture2D(renderTexture.width, renderTexture.height, EvolutionManager.Instance.TextureToSimulate.format, GenerateMipMaps);

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
        //index_offset = (index_offset + 1) % precision;
        //differenceMaterial.SetTexture("_Current_State", currentState);

        _camera.backgroundColor = bg_color;
        _camera.cullingMask = currentStateLayerMask;
        //currentState = TakeScreenshot(currentState);
        //currentStateColors = currentState.GetPixelData<Color32>(MipMapLevel);
        //_camera.cullingMask = candidateLayerMask;
    }

    public int CalculateScore()
    {
        //double t = Time.realtimeSinceStartupAsDouble;

        sc = TakeScreenshot(sc);
        //colorDifferenceSum = Vector3.zero;
        colorDifferenceSum = 0;

        screenshotolors = sc.GetPixelData<Color32>(MipMapLevel);

        int difference = GetTextureDifference();

        return difference;
    }

    public int CalculateScore(Shape shape)
    {
        if (shape.score > -1)
            return shape.score;

        _camera.cullingMask = everythingLayerMask;
        _camera.backgroundColor = bg_color;

        shape.sprite.enabled = true;

        _camera.Render();

        resultBuffer.SetData(new uint[1]); // reset

        shader.SetTexture(kernel, "Target", EvolutionManager.Instance.TextureToSimulate);
        shader.SetTexture(kernel, "Current", renderTexture);
        shader.SetBuffer(kernel, "Result", resultBuffer);

        int threadGroupsX = EvolutionManager.Instance.TextureToSimulate.width / 8;
        int threadGroupsY = EvolutionManager.Instance.TextureToSimulate.height / 8;
        shader.Dispatch(kernel, threadGroupsX, threadGroupsY, 1);

        resultBuffer.GetData(resultArray); // just one int
        shape.score = (int)resultArray[0];
        shape.sprite.enabled = false;


        return shape.score;

        /*if (shape.score > -1)
            return shape.score;

        #region get screenshot colors
        shape.sprite.enabled = true;
        if (shape.colorMode == ShapeColorMode.AverageColorFromTexture &&
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
            screenshotolors = sc.GetPixelData<Color32>(MipMapLevel);
        }
        #endregion

        // Calculate color difference
        int difference = GetTextureDifference();

        shape.score = difference;
        shape.sprite.enabled = false;
        return difference;*/
    }

    private int GetTextureDifference()
    {
        int difference = 0;
        for (int i = index_offset; i < targetColors.Length; i += precision)
        {
            //difference = StaticUtilites.VectorAbs(targetColors[i] - screenshotolors[i]);
            //difference += StaticUtilites.ColorDifference(targetColors[i], screenshotolors[i]);
            int r = Mathf.Abs(targetColors[i].r - screenshotolors[i].r);
            int g = Mathf.Abs(targetColors[i].g - screenshotolors[i].g);
            int b = Mathf.Abs(targetColors[i].b - screenshotolors[i].b);
            if (useMaxColor)
                difference += Mathf.Max(Mathf.Max(r, g), b);
            else
                difference += r + g + b;
            //int worstColor = (int)(StaticUtilites.VectorMax(difference));
            //colorDifferenceSum += worstColor;
        }
        return difference;
    }


    public Texture2D TakeScreenshot(Texture2D outputTexture)
    {
        if(outputTexture == null)
        {
            Debug.Log("initalizing screenshot texture");
            outputTexture = StaticUtilites.TakeScreenshot(renderTexture, GenerateMipMaps);
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
    private NativeArray<Color32> GetShapeColorsAsAverageFromTarget(Shape currentShape)
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
        screenshotolors = sc.GetPixelData<Color32>(1);

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

                sum += (((Vector4)(Color)targetColors[i]) * 256) * (screenshotolors[i].a);
                count += screenshotolors[i].a;
            }
        }
        Color32 avg_color = (Color)(sum / count);
        avg_color.a = (byte)currentShape.sprite.color.a;

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

        avg_color.a = (byte)currentOpacity;   
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
        Debug.Log("getting target pixel colors");
        NativeArray<Color32> temp = EvolutionManager.Instance.TextureToSimulate.GetPixelData<Color32>(MipMapLevel);
        targetColors = new Color32[temp.Length];

        for(int i=0; i<temp.Length; i++)
        {
            targetColors[i] = temp[i];
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
