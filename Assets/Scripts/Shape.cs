using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using NaughtyAttributes;
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public class Shape
{
    /*
     * BIG TODO: RANDOMIZE FLIPY AND FLIPX
     */ 

    [ReadOnly]
    public int score = -1;

    public bool settingsGenerated { get; private set; } // to be used by shapemanager
    public bool settingsApplied { get; private set; } // to be used by shapemanager

    [HideInInspector] public SpriteRenderer spriteRenderer;
    [HideInInspector] public Transform transform;
    public GameObject gameObject => transform.gameObject;

    [HideInInspector] public int variantLevel;
    public float a => spriteRenderer.color.a;

    //calculation variables
    static int colorModeIndex;
    static int possibleColorModesCount; /* TODO: move to shape manager */

    #region Attributes

    // percent 0 - 1
    Vector2 position;
    Vector2 scale;
    float rotation;
    Sprite sprite;
    Color color;
    bool flipX;
    public ShapeColorMode colorMode { get; private set; }
    int layer;

    #endregion

    Vector2 scaledPosition => StaticUtilities.DoubleLerp(-ShapeManager.scaledHalfSize, ShapeManager.scaledHalfSize, position);

    public Shape(Transform transform, ShapeColorMode shapeColorMode)
    {
        if (possibleColorModesCount <= 0)
            possibleColorModesCount = System.Enum.GetValues(typeof(ShapeColorMode)).Length;

        this.transform = transform;
        spriteRenderer = transform.GetComponent<SpriteRenderer>();

        Reset();
        //hasSetColor = false;
        //sprite.color = Color.white;
    }

    public void Reset()
    {
        settingsApplied = false;
        settingsGenerated = false;
        score = -1;
        variantLevel = 0;

        spriteRenderer.enabled = false;
    }

    public void InitializeRandomProperties(ShapeColorMode colorMode)
    {
        this.colorMode = colorMode; 
        score = -1;
        variantLevel = 0;

        RandomizePosition(1);
        RandomizeScale(1);
        RandomizeRotation(1);
        RandomizeOpacity(1);
        RandomizeColor(1);
        RandomizeSprite(1);
        if(ShapeManager.Instance.randomizeZOrder)
            RandomizeZOrder(1);
        RandomizeSpriteFlip(1);

        settingsGenerated = true;
        settingsApplied = false;
    }

    public void ApplyTransformations(bool showShape)
    {
        if (spriteRenderer == null) spriteRenderer = transform.GetComponent<SpriteRenderer>();

        transform.position = scaledPosition;
        transform.eulerAngles = new Vector3(0, 0, rotation);
        transform.localScale = scale;

        spriteRenderer.flipX = flipX;
        spriteRenderer.color = color;
        spriteRenderer.sortingOrder = layer;
        spriteRenderer.sprite = sprite;

        spriteRenderer.enabled = showShape;

        settingsApplied = true;
    }

    public void CopyShape(Shape other, bool copyLayer=false)
    {
        if(other.score < 0)
            CameraManager.Instance.CalculateScore(other);

        transform = GameObject.Instantiate(other.transform);
        spriteRenderer = transform.GetComponent<SpriteRenderer>();

        position = other.position;
        rotation = other.rotation;
        scale = other.scale;
        sprite = other.sprite;
        flipX = other.flipX;
        color =other.color;

        score = other.score;
        colorMode = other.colorMode;

        if (copyLayer)
            layer = other.layer;

        settingsApplied = false;
        settingsGenerated = true;
    }

    public void SetColorMode(ShapeColorMode colorMode)
    {
        this.colorMode = colorMode;
    }

    #region randomize properties

    public void RandomizeSprite(float intensityScalar=1)
    {
        if (intensityScalar >= Random.value) 
            sprite = ShapeManager.Instance.shapeSprites[Random.Range(0, ShapeManager.Instance.shapeSprites.Count)];
    }

    public void RandomizeSpriteFlip(float intensityScalar=1)
    {
        if (Random.value <= intensityScalar ) 
            flipX = !flipX;
    }

    public void RandomizeRotation(float intensityScalar = 1)
    {
        float z = rotation;
        float r = Random.value * 360f;
        float newz = Mathf.LerpAngle(z, r, intensityScalar);
        rotation = newz;
    }

    public void RandomizePosition(float intensityScalar = 1)
    {
        score = -1;

        //float x = Random.Range(-ShapeManager.scaledHalfSize.x, ShapeManager.scaledHalfSize.x);
        //float y = Random.Range(-ShapeManager.scaledHalfSize.y, ShapeManager.scaledHalfSize.y);

        Vector2 randomPos = new Vector2(Random.value, Random.value);

        Vector2 newPos = Vector3.Lerp (position, randomPos, intensityScalar);

        position = newPos;
    }

    public void RandomizeColor(float intensityScalar = 1, bool randomizeALittle = false)
    {
        if (colorMode == ShapeColorMode.AverageColorFromTexture)
            Debug.LogWarning("this is supposed to average");

        //score = -1;

        Color current = color;
        Color newColor;

        switch (colorMode)
        {
            case ShapeColorMode.AnyRandomColorFromImage:
                newColor = EvolutionManager.GetRandomColorFromTargetTexture();
                break;

            case ShapeColorMode.RandomColorNearPosition:
                newColor = EvolutionManager.GetRandomColorFromTargetTextureNearPoint(position);
                break;

            case ShapeColorMode.AverageColorFromTexture:
                float x_pct = Mathf.InverseLerp(-ShapeManager.halfsize.x, ShapeManager.halfsize.x, transform.position.x);
                float y_pct = Mathf.InverseLerp(-ShapeManager.halfsize.y, ShapeManager.halfsize.y, transform.position.y);

                if (randomizeALittle)
                {
                    x_pct = Mathf.Clamp01(x_pct + (0.1f * scale.x * (Random.value * 2 - 1)));
                    y_pct = Mathf.Clamp01(y_pct + (0.1f * scale.y * (Random.value * 2 - 1)));
                }

                newColor = EvolutionManager.Instance.TextureToSimulate.GetPixelBilinear(x_pct, y_pct);
                break;

            case ShapeColorMode.CompletelyRandom:
                newColor = Random.ColorHSV();
                break;

            default:
                Debug.Log($"Unrecognized color mode: {colorMode.ToString()}");
                newColor = Color.magenta;
                break;
        }

        newColor = Color.Lerp(current, newColor, intensityScalar);
        newColor.a = current.a;
        color = newColor;
    }

    public void RandomizeOpacity(float intensityScalar=1)
    {
        score = -1;

        float a = Random.Range(ShapeManager.Instance.minAlpha, ShapeManager.Instance.maxAlpha) ;
        //a = Mathf.Clamp(a,ShapeManager.Instance.minAlpha, 1);
        //a = Mathf.Clamp01 (a);
        float current = color.a;

        color.a = Mathf.Lerp(a, a, intensityScalar);
    }

    public void RandomizeScale(float intensityScalar = 1)
    {
        score = -1;

        float minsize = ShapeManager.Instance.MinShapeSize;
        float maxsize = ShapeManager.Instance.MaxShapeSize;

        if (ShapeManager.Instance.PreserveAspectRatio)
        {
            float oldScale = scale.x;
            float randomScale = Mathf.Lerp(minsize, maxsize, Mathf.Pow(Random.value, ShapeManager.Instance.SmallShapesSizePreference));
            float newScale = Mathf.Lerp(oldScale, randomScale, intensityScalar);
            scale = Vector3.one * newScale;
        }
        else
        {
            Vector2 oldScale = scale;
            float randomScalex = Mathf.Lerp(minsize, maxsize, Mathf.Pow(Random.value, ShapeManager.Instance.SmallShapesSizePreference));
            float randomScaley = Mathf.Lerp(minsize, maxsize, Mathf.Pow(Random.value, ShapeManager.Instance.SmallShapesSizePreference));

            float newx = Mathf.Lerp(oldScale.x, randomScalex, intensityScalar);
            float newy = Mathf.Lerp(oldScale.y, randomScaley, intensityScalar);

            Vector2 newScale = new Vector2(newx, newy);

            scale = newScale;
        }
    }

    public void RandomizeZOrder(float intensity=1)
    {
        score = -1;

        int random = Random.Range(0, ShapeManager.Instance.MaxZOrder);
        layer = (int)Mathf.Lerp(spriteRenderer.sortingOrder, random, intensity);
    }

    #endregion

    #region debug

    [Button]
    private void CalculateScore_DEBUG()
    {
        gameObject.layer = 6;
        score = -1;
        spriteRenderer.enabled = false;
        ShapeManager.OnShapeSelected.Invoke(); // to get current state
        spriteRenderer.enabled = true;
        CameraManager.Instance.CalculateScore(this);
        spriteRenderer.enabled = false;
        gameObject.layer = 7;
    }

    private void OnDrawGizmosSelected()
    {
        if(colorMode == ShapeColorMode.AverageColorFromTexture)
            CalculateScore_DEBUG();
    }

    #endregion

    #region obsolete
    [System.Obsolete]
    public void SetPosition(float x_pct, float y_pct)
    {
        score = -1;

        float x = Mathf.Lerp(-ShapeManager.scaledHalfSize.x, ShapeManager.scaledHalfSize.x, x_pct);
        float y = Mathf.Lerp(-ShapeManager.scaledHalfSize.y, ShapeManager.scaledHalfSize.y, y_pct);

        DrawVector2.Point(x, y, 0.5f);


        transform.position = new Vector3(x, y);
    }


    #endregion
}
