using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

public class ShapeManager : Singleton<ShapeManager>
{
    private SettingsProfile settingsProfile => EvolutionManager.Instance.settingsProfile;

    #region Settings Profile Variables
    // these variables are better sorted in SettingsProfile.cs
    private bool UseRejectedShapes => settingsProfile.UseRejectedShapes;
    private bool AllRejectedShapes => settingsProfile.AllRejectedShapes;
    private bool IncreaseStandardsIfBadShapes => settingsProfile.IncreaseStandardsIfBadShapes;
    private int baseGenerations => settingsProfile.baseGenerations;
    private int generationsToForceStop => settingsProfile.generationsToForceStop;
    public int initalRandomShapes => settingsProfile.initalRandomShapes;
    public int randomShapesIfBadShapes => settingsProfile.randomShapesIfBadShapes;
    private int shapeVariants => settingsProfile.shapeVariants;
    private int shapeVariantsIfGoodShapes => settingsProfile.shapeVariantsIfGoodShapes;
    private int maxShapes=> settingsProfile.maxShapes;
    private bool ContinueIteratingIfShapeIsBest => settingsProfile.ContinueIteratingIfShapeIsBest;
    float ChanceToChangeSprite => settingsProfile.ChanceToChangeSprite;
    float ChanceToFlipSprite => settingsProfile.ChanceToFlipSprite;
    float rotationScalar => settingsProfile.rotationScalar;
    float positionScalar => settingsProfile.positionScalar;
    float colorScalar => settingsProfile.colorScalar;
    float opacityScalar => settingsProfile.opacityScalar;
    float sizeScalar => settingsProfile.sizeScalar;
    public float MaxShapeSize=> settingsProfile.MaxShapeSize;
    public float MinShapeSize => settingsProfile.MinShapeSize;
    public bool PreserveAspectRatio => settingsProfile.PreserveAspectRatio;
    public float SmallShapesSizePreference => settingsProfile.SmallShapesSizePreference;
    public float minAlpha => settingsProfile.minAlpha;
    public float maxAlpha => settingsProfile.maxAlpha;
    public bool AverageColorMask => settingsProfile.AverageColorMask;
    public bool ApplyAverageToVariants => settingsProfile.ApplyAverageToVariants;
    public bool FullyRandomColor => settingsProfile.FullyRandomColor;
    public bool AnyRandomColorFromImage=> settingsProfile.AnyRandomColorFromImage;
    private bool randomizeZOrder=> settingsProfile.randomizeZOrder;
    public int MaxZOrder => settingsProfile.MaxZOrder;
    public List<Sprite> shapeSprites=> settingsProfile.shapeSprites;

    #endregion

    public static UnityEvent OnShapeSelected = new UnityEvent();
    public static UnityEvent OnShapeFailed = new UnityEvent();

    [HideInInspector] public static Vector2 halfsize;
    [HideInInspector] public static Vector2 scaledHalfSize;

    [Header("Debug")]
    [ReadOnly]
    public List<Shape> shapes = new List<Shape>();
    public TMP_Text debug_text;

    private int bestScore = -1;
    private int currentScore = -1;
    private int shapesCreated = 0;

    #region obsolete
    [System.Obsolete]
    int shapeTweakIndex;
    #endregion

    private void Start()
    {
        EvolutionManager.Instance.OnRefreshImage.AddListener(SetHalfSize);

        DebugHSDFGds();
    }

    async void DebugHSDFGds()
    {
        await Task.Delay(3500);
        //NaturallySelectNewShape();
        OnShapeSelected.Invoke();

        bestScore = CameraManager.Instance.CalculateScore();
        currentScore = bestScore;
        debug_text.text = bestScore.ToString();

        //NaturallySelectNewShape();
        StartCoroutine(KeepMakingShapesForever());
    }

    #region natural selection
    private IEnumerator KeepMakingShapesForever()
    {
        while (true)
        {
            Destroy(NaturallySelectNewShape());
            //NaturallySelectNewShape();

            yield return null;
        }
    }    

    [Button]
    public Shape NaturallySelectNewShape()
    {
        if(shapes.IsUnityNull())
            shapes = new List<Shape>();

        shapes.Clear();
        PopulateRandomShapes();

        ScoreAllShapes();
        ShapeSort(0, shapes.Count - 1);
        EliminateAllButNShapes(maxShapes);

        bool foundShapeThatImprovesCurrentScore = false;

        int i=0;
        while (i<baseGenerations || !foundShapeThatImprovesCurrentScore || 
            (ContinueIteratingIfShapeIsBest && shapes[0].score < bestScore))
        {
            i++;
            // ok clearly what we have isnt working
            ConsiderAddingMoreShapes(i); // removes shapes, dont worry

            ScoreAllShapes();
            CreateShapeVariants();
            ScoreAllShapes(); // idk whats going on. they might be sorted already
            ShapeSort(0, shapes.Count - 1);

            if (i >= generationsToForceStop)
            {
                // Force stop after baseGenerations * 3
                if (shapes[0].score <= currentScore)
                    break;
                
                OnShapeFailed.Invoke();
                if (IncreaseStandardsIfBadShapes)
                    IncreaseStandards();

                if (!UseRejectedShapes || (!AllRejectedShapes && shapes[0].score-currentScore > 15000))
                {
                    Debug.LogError("shape with score " + shapes[0].score + " force rejected (current score: "+currentScore+")");
                    Debug.LogError(shapes[0].score - currentScore);
                    return null;
                }
                else
                {
                    //Debug.LogError("adding this shape makes the image worse");
                    break;
                }
            }

            EliminateAllButNShapes(maxShapes);

            foundShapeThatImprovesCurrentScore = (shapes[0].score < currentScore);
        }

        Shape winner = shapes[0];

        if(!winner.IsUnityNull())
            winner.sprite.enabled = true;
        winner.sprite.sortingOrder = shapesCreated + 1;

        ShapePoolManager.Instance.EjectShapeFromPool(winner);
        ShapePoolManager.Instance.RemoveAllShapes();

        #region set score text
        if (winner.score < bestScore)
            debug_text.color = Color.green;
        
        else if(winner.score < currentScore)
            debug_text.color = Color.yellow;
        
        else
            debug_text.color = Color.red;

        debug_text.text = winner.score.ToString();
        #endregion

        if(winner.score <= currentScore)
            OnShapeSelected.Invoke();

        shapesCreated++;

        if (winner.score < bestScore)
            bestScore = winner.score;
        currentScore = winner.score;


        winner.gameObject.name = winner.sprite.sprite.name;
        Debug.Log("Stopped after " + i + " generations...\n" +
                  "Shape variant level: " + winner.variantLevel + "\n" +
                  "Shape color type: " + winner.colorMode.ToString() + "\n" +
                  "Sprite: " + winner.sprite.sprite.name);

        return winner;
    }

    private void ConsiderAddingMoreShapes(int generation)
    {
        if (shapes[0].score >= currentScore /*&& generation < baseGenerations*/)
        {
            //ScoreAllShapes(); // sets the color of all shapes :/ (weird function, but NEEDED for optimization)
            PopulateNRandomShapes(randomShapesIfBadShapes);
            ScoreAllShapes();
            ShapeSort(0, shapes.Count - 1);
            EliminateAllButNShapes(maxShapes);
        }
    }

    private void IncreaseStandards()
    {
        // settingsProfile is a copy of the original, so modifying it is ok
        int statToIncrease = Random.Range(0, 6);

        switch(statToIncrease)
        {
            case 0:
                settingsProfile.baseGenerations++;
                break;
            case 1:
                settingsProfile.generationsToForceStop++;
                break;
            case 2:
                settingsProfile.initalRandomShapes++;
                break;
            case 3:
                settingsProfile.randomShapesIfBadShapes++;
                break;
            case 4:
                settingsProfile.shapeVariants++;
                break;
            case 5:
                settingsProfile.shapeVariantsIfGoodShapes++;
                break;
            case 6:
                settingsProfile.maxShapes++;
                break;
        };
        
    }

    [Button]
    private void PopulateRandomShapes()
    {
        while(shapes.Count< initalRandomShapes)
        {
            Shape newShape = CreateNewRandomShape();

            shapes.Add(newShape);
            newShape.sprite.enabled = false;
        }
    }

    private void PopulateNRandomShapes(int n)
    {
        for(int i=0; i<n; i++)
        {
            Shape newShape = CreateNewRandomShape();

            shapes.Add(newShape);
            newShape.sprite.enabled = false;
        }
    }

    [Button]
    private void CreateShapeVariants()
    {
        List<Shape> newShapes = new List<Shape>();


        for ( int i = 0; i < shapes.Count; i++ )
        {
            int variantsToCreate = shapes[i].score < currentScore ? shapeVariantsIfGoodShapes : shapeVariants;

            for (int j = 0; j< variantsToCreate; j++)
            {
                Shape variant = CreateNewShapeVariant(shapes[i]);
                newShapes.Add(variant);
                variant.sprite.enabled = false;
            }
        }

        for (int i = 0; i < newShapes.Count; i++)
        {
            shapes.Add(newShapes[i]);
        }
    }

    void ScoreAllShapes()
    {
        float t = Time.unscaledTime;
        for (int i = 0; i < shapes.Count; i++)
        {
            CameraManager.Instance.CalculateScore(shapes[i]);
            //EditorApplication.isPaused = true;

        }
        //Debug.Log((Time.unscaledTime - t) + " seconds to score " + shapes.Count + " shapes");
    }

    

    #region sort
    /// <summary>
    /// Mergesorts the shapes list by the scores
    /// </summary>
    void ShapeSort(int left, int right)
    {
        if (left >= right)
            return;

        int mid = left + (right - left) / 2;
        ShapeSort(left, mid);
        ShapeSort(mid + 1, right);
        mergeShapeSort(left, mid, right);
    }

    void mergeShapeSort(int left, int mid, int right)
    {
        int n1 = mid - left + 1;
        int n2 = right - mid;

        // Create temp vectors
        List<Shape> L = new List<Shape>();
        List<Shape> R = new List<Shape>();

        // Copy data to temp vectors L[] and R[]
        for (int _i = 0; _i < n1; _i++)
        {
            L.Add(shapes[left + _i]);
        }
        for (int _j = 0; _j < n2; _j++)
        {
            R.Add(shapes[mid + 1 + _j]);
        }

        int i = 0, j = 0;
        int k = left;

        // Merge the temp vectors back 
        // into arr[left..right]
        while (i < n1 && j < n2)
        {
            if (L[i].score <= R[j].score)
            {
                shapes[k] = L[i];
                i++;
            }
            else
            {
                shapes[k] = R[j];
                j++;
            }
            k++;
        }

        // Copy the remaining elements of L[], 
        // if there are any
        while (i < n1)
        {
            shapes[k] = L[i];
            i++;
            k++;
        }

        // Copy the remaining elements of R[], 
        // if there are any
        while (j < n2)
        {
            shapes[k] = R[j];
            j++;
            k++;
        }
    }

    #endregion
    /// <summary>
    /// returns winner
    /// </summary>
    private void EliminateAllButNShapes(int n)
    {
        if (shapes.Count <= 0)
        {
            Debug.LogError("dont get into this situation");
        }

        int index;
        while(shapes.Count > n)
        {
            index = shapes.Count-1;
            ShapePoolManager.Instance.RemoveShape(shapes[index]);
            shapes.RemoveAt(index);
        }
    }

    private void SetHalfSize()
    {
        halfsize = new Vector2(CameraManager.CameraWidthWorldSpace, CameraManager.CameraHeightWorldSpace);
        scaledHalfSize = halfsize / 2;
        scaledHalfSize *= 1.1f;
    }

    #endregion

    #region shape manipulation

    [Button]
    private Shape CreateNewRandomShape()
    {
        Shape shape = ShapePoolManager.Instance.CreateShape() ;

        shape.Initialize();
        shape.RandomizeSprite();
        shape.RandomizeSpriteFlip();
        shape.RandomizeRotation();
        shape.RandomizePosition();
        // color is set when score is set
        if (shape.colorMode == ShapeColorMode.RandomColorByPosition || !AverageColorMask)
        {
            if (FullyRandomColor)
                shape.RandomizeColorCompletely(1, false);
            else
                shape.RandomColorGenerationMethod();
        }
        shape.RandomizeOpacity();
        shape.RandomizeScale();
        if (randomizeZOrder)
            shape.RandomizeZOrder();
        else
            shape.sprite.sortingOrder = shapesCreated;
        CameraManager.Instance.CalculateScore(shape);
        return shape;
    }

    private void Random_TweakShape(Shape shape)
    {
        shape.RandomizeSprite(ChanceToChangeSprite);
        shape.RandomizeSpriteFlip(ChanceToFlipSprite);
        shape.RandomizeRotation(rotationScalar);
        shape.RandomizePosition(positionScalar);
        shape.RandomizeOpacity(opacityScalar);
        shape.RandomizeScale(sizeScalar);
        if(randomizeZOrder)
            shape.RandomizeZOrder(0.1f);
        if(shape.colorMode == ShapeColorMode.RandomColorByPosition || !ApplyAverageToVariants)
        {
            if (FullyRandomColor)
                shape.RandomizeColorCompletely(colorScalar, false);
            else
                shape.RandomColorGenerationMethod(colorScalar);
        }
    }

    /// <summary>
    /// Returns true if new shape is better
    /// </summary>
    public Shape CreateNewShapeVariant(Shape shape)
    {
        Shape newShape = ShapePoolManager.Instance.CreateShape(shape);

        newShape.Initialize();
        Random_TweakShape(newShape);
        newShape.variantLevel = shape.variantLevel + 1;

        shape.sprite.enabled = false;

        return newShape;
    }
    #endregion

    #region debug

    private void PrintAllShapeScores()
    {
        string output = "";
        for (int i = 0; i < shapes.Count; i++)
        {
            output += shapes[i].score.ToString() + " ";
        }
        Debug.Log(output);
    }

    private void PrintHighestVariant()
    {
        int highest = 0;
        for(int i=0; i< shapes.Count; i++)
        {
            if (shapes[i].variantLevel > highest)
                highest = shapes[i].variantLevel;
        }
        Debug.Log("highest variant level: " + highest);
    }


    #endregion

    #region obsolete

[System.Obsolete]
    public void RandomlyTweakNShapes(int n)
    {
        //float scalar = 1 - candidateController.Accuracy;
        float scalar = 0.1f;
        for (int i = 0; i < n; i++)
        {
            shapeTweakIndex = (shapeTweakIndex + 1) % shapes.Count;
            Random_TweakShape(shapes[shapeTweakIndex]);
        }
    }

    [System.Obsolete]
    public void Random_Partial_TweakShape(Shape shape, float scalar)
    {
        if (StaticUtilites.CoinFlip()) shape.RandomizeSprite(scalar);
        if (StaticUtilites.CoinFlip()) shape.RandomizeRotation(scalar);
        if (StaticUtilites.ChanceFraction(1, 1000)) shape.RandomizePosition(1);
        //if (StaticUtilites.CoinFlip()) shape.RandomizePosition(scalar);
        if (StaticUtilites.CoinFlip()) shape.SetColor(scalar);
        if (StaticUtilites.CoinFlip()) shape.RandomizeOpacity(scalar);
        if (StaticUtilites.CoinFlip()) shape.RandomizeScale(scalar);
        if (StaticUtilites.CoinFlip()) shape.RandomizeZOrder(scalar);
    }

    [System.Obsolete]
    public void CopyShapeController(ShapeManager other)
    {
        if (other == this)
        {
            Debug.LogWarning("bruh why");
            return;
        }

        for (int i = 0; i < other.shapes.Count; i++)
        {
            if (i >= shapes.Count)
            {
                Shape shape = ShapePoolManager.Instance.CreateShape();
                shape.CopyShape(other.shapes[i]);
                shapes.Add(shape);
                continue;
            }
            shapes[i].CopyShape(other.shapes[i]);
        }

        // if this shapes list is bigger, somehow
        for (int i = other.shapes.Count; i < shapes.Count; i++)
        {
            ShapePoolManager.Instance.RemoveShape(shapes[i]);
            shapes.RemoveAt(i);
        }
    }

    #region debug
    [Button]
    public void Force_RandomlyTweakAllShapes_DEBUG()
    {
        //float scalar = 1 - candidateController.Accuracy;
        float scalar = 0.1f;
        for (int i = 0; i < shapes.Count; i++)
        {

            Shape shape = shapes[i];
            Random_Partial_TweakShape(shape, scalar);
        }

    }

    [System.Obsolete]
    void RemoveScoresUnderAverage()
    {
        int sum = 0;
        int countbefore = shapes.Count;
        for (int i = 0; i < shapes.Count; i++)
        {
            if (shapes[i].score < 0)
                Debug.LogError("dont let this happen");

            sum += shapes[i].score;
        }
        int avg = (int)((float)sum / (float)shapes.Count);

        for (int i = 0; i < shapes.Count; i++)
        {
            while (i < shapes.Count && shapes[i].score < avg)
            {
                ShapePoolManager.Instance.RemoveShape(shapes[i]);
                shapes.RemoveAt(i);
            }
        }

        Debug.Log("removed " + (countbefore - shapes.Count) + "/" + countbefore);
    }


    #endregion

    #endregion
}
