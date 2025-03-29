using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

public class Shape : MonoBehaviour
{
    [ReadOnly]
    public int score = -1;

    [ReadOnly]
    public bool inUse; // to be used by shapemanager

    public ShapeColorMode colorMode;

    [HideInInspector] public SpriteRenderer sprite;
    private bool hasSetPosition = false;
    [HideInInspector] public bool hasSetColor = false;
    [HideInInspector] public int variantLevel;
    public float a => sprite.color.a;

    //calculation variables
    static int colorModeIndex;
    static int possibleColorModesCount; /* TODO: move to shape manager */


    private void Start()
    {
        if(possibleColorModesCount <=0 )
            possibleColorModesCount = System.Enum.GetValues(typeof(ShapeColorMode)).Length;
    }

    public void Initialize()
    {
        sprite = GetComponent<SpriteRenderer>();
        score = -1;
        colorModeIndex = (colorModeIndex + 1) % /*possibleColorModesCount*/ 2;
        colorMode = (ShapeColorMode) (colorModeIndex);

        if(!ShapeManager.Instance.AverageColorMask)
            colorMode = ShapeColorMode.RandomColorByPosition;
        //hasSetColor = false;
        //sprite.color = Color.white;
    }

    /// <summary>
    /// when shape not in use anymore, NOT when shape is completely ejected from the pool.
    /// poor wording.
    /// </summary>
    public void OnRemoveFromPool()
    {
        variantLevel = 0;
        //sprite.color = Color.white;
        hasSetColor = false;
    }

    public void CopyShape(Shape other, bool copyLayer=false)
    {
        if(other.score < 0)
            CameraManager.Instance.CalculateScore(other);

        if(sprite == null)
            sprite = GetComponent<SpriteRenderer>();

        transform.position = other.transform.position;
        transform.rotation = other.transform.rotation;
        transform.localScale = other.transform.localScale;
        sprite.sprite = other.sprite.sprite;
        if(!ShapeManager.Instance.ApplyAverageToVariants)
            sprite.color =other.sprite.color;

        hasSetColor = other.hasSetColor;
        score = other.score;
        colorMode = other.colorMode;

        if (copyLayer)
            gameObject.layer = other.gameObject.layer;
    }

    public void SetColor(Color color)
    {
        if (colorMode != ShapeColorMode.AverageColorFromTexture)
            Debug.LogWarning("warning");

        color.a = sprite.color.a;
        sprite.color = color;
        hasSetColor = true;
    }

    #region randomize properties

    public void RandomizeSprite(float intensityScalar=1)
    {
        score = -1;

        if (sprite == null)
            sprite = GetComponent<SpriteRenderer>();

        if (intensityScalar >= Random.value) 
            sprite.sprite = ShapeManager.Instance.shapeSprites[Random.Range(0, ShapeManager.Instance.shapeSprites.Count)];
    }
    public void RandomizeRotation(float intensityScalar = 1)
    {
        score = -1;

        float z = transform.eulerAngles.z;
        float r = Random.value * 360;
        float newz = Mathf.LerpAngle(z, r, intensityScalar);
        transform.eulerAngles = new Vector3(0, 0, newz);
    }

    public void RandomizePosition(float intensityScalar = 1)
    {
        score = -1;

        float x = Random.Range(-ShapeManager.scaledHalfSize.x, ShapeManager.scaledHalfSize.x);
        float y = Random.Range(-ShapeManager.scaledHalfSize.y, ShapeManager.scaledHalfSize.y);

        Vector2 randomPos = new Vector3(x, y) ;

        Vector2 newPos = Vector3.Lerp (transform.position, randomPos, intensityScalar);

        if (false && hasSetPosition)
        {
            Debug.DrawLine(transform.position, randomPos, Color.black, 1);
            Debug.DrawLine(transform.position, newPos, Color.green, 1);
        }

        transform.position = newPos;

        hasSetPosition = true;
    }

    public void RandomColorGenerationMethod(float intensityScalar = 1)
    {
        if (StaticUtilites.ChanceFraction(1,3))
        {
            RandomizeColorCompletely(intensityScalar, true);
        }
        else
        {
            SetColor(intensityScalar, StaticUtilites.CoinFlip());
        }

    }

    //[System.Obsolete]
    public void RandomizeColorCompletely(float intensityScalar = 1, bool useColorFromTexture = true)
    {
        if (colorMode == ShapeColorMode.AverageColorFromTexture)
            Debug.LogWarning("this is supposed to average");

        if(intensityScalar >= 1)
            hasSetColor = true;

        if (!hasSetColor && ShapeManager.Instance.AverageColorMask)
            Debug.LogWarning("color not initially set! oh no");

        score = -1;

        if (intensityScalar >= 1)
            hasSetColor = true;

        Color current = sprite.color;
        Color random;

        if (useColorFromTexture)
        {
            random = StaticUtilites.GetRandomColorFromTexture(EvolutionManager.Instance.TextureToSimulate);
        }
        else
            random = new Color(Random.value, Random.value, Random.value);

        random.a = current.a;
        sprite.color = Color.Lerp(current, random, intensityScalar);
    }

    public void SetColor(float intensityScalar = 1, bool randomizeALittle = false)
    {
        if (colorMode == ShapeColorMode.AverageColorFromTexture)
            Debug.LogWarning("this is supposed to average");

        if (intensityScalar >= 1)
            hasSetColor = true;

        if (!hasSetColor && ShapeManager.Instance.AverageColorMask)
            Debug.LogWarning("color not initially set! oh no");

        //score = -1;

        Color current = sprite.color;
        Color newColor;

        if (!hasSetPosition && ShapeManager.Instance.AnyRandomColorFromImage)
            Debug.LogError("Color set before position");

        if (hasSetPosition && ShapeManager.Instance.AnyRandomColorFromImage)
        {
            newColor = StaticUtilites.GetRandomColorFromTexture(EvolutionManager.Instance.TextureToSimulate);
        }
        else
        {
            float x_pct = Mathf.InverseLerp(-ShapeManager.halfsize.x, ShapeManager.halfsize.x, transform.position.x);
            float y_pct = Mathf.InverseLerp(-ShapeManager.halfsize.y, ShapeManager.halfsize.y, transform.position.y);

            if (randomizeALittle)
            {
                float scale = transform.localScale.x;
                x_pct = Mathf.Clamp01(x_pct + (0.1f * scale * (Random.value * 2 - 1))); 
                y_pct = Mathf.Clamp01(y_pct + (0.1f * scale * (Random.value * 2 - 1)));
            }

            newColor = EvolutionManager.Instance.TextureToSimulate.GetPixelBilinear(x_pct, y_pct);
        }

        newColor.a = current.a;
        sprite.color = newColor;
    }

    public void RandomizeOpacity(float intensityScalar=1)
    {
        score = -1;

        float a = Random.Range(ShapeManager.Instance.minAlpha, ShapeManager.Instance.maxAlpha) ;
        //a = Mathf.Clamp(a,ShapeManager.Instance.minAlpha, 1);
        //a = Mathf.Clamp01 (a);
        Color current = sprite.color;

        current.a = Mathf.Lerp(current.a, a, intensityScalar);

        sprite.color = current;
    }

    public void RandomizeScale(float intensityScalar = 1)
    {
        score = -1;

        float minsize = ShapeManager.Instance.MinShapeSize;
        float maxsize = ShapeManager.Instance.MaxShapeSize;

        if (ShapeManager.Instance.PreserveAspectRatio)
        {
            float oldScale = transform.localScale.x;
            float randomScale = Mathf.Lerp(minsize, maxsize, Mathf.Pow(Random.value, ShapeManager.Instance.SmallShapesSizePreference));
            float newScale = Mathf.Lerp(oldScale, randomScale, intensityScalar);
            transform.localScale = Vector3.one * newScale;
        }
        else
        {
            Vector2 oldScale = transform.localScale;
            float randomScalex = Mathf.Lerp(minsize, maxsize, Mathf.Pow(Random.value, ShapeManager.Instance.SmallShapesSizePreference));
            float randomScaley = Mathf.Lerp(minsize, maxsize, Mathf.Pow(Random.value, ShapeManager.Instance.SmallShapesSizePreference));

            float newx = Mathf.Lerp(oldScale.x, randomScalex, intensityScalar);
            float newy = Mathf.Lerp(oldScale.y, randomScaley, intensityScalar);

            Vector2 newScale = new Vector2(newx, newy);

            transform.localScale = newScale;
        }
    }

    public void RandomizeZOrder(float intensity=1)
    {
        score = -1;

        int random = Random.Range(0, ShapeManager.Instance.MaxZOrder);
        sprite.sortingOrder = (int)Mathf.Lerp(sprite.sortingOrder, random, intensity);
    }

    #endregion

    #region debug

    [Button]
    private void CalculateScore_DEBUG()
    {
        gameObject.layer = 6;
        hasSetColor = false;
        score = -1;
        sprite.enabled = false;
        ShapeManager.OnShapeSelected.Invoke(); // to get current state
        sprite.enabled = true;
        CameraManager.Instance.CalculateScore(this);
        sprite.enabled = true;
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
        hasSetPosition = true;
    }


    #endregion
}
