using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Settings Profile", menuName = "ScriptableObjects/Settings Profile")]
public class SettingsProfile : ScriptableObject
{
    [Header("Properties")]
    [SerializeField] public bool UseRejectedShapes = true;
    [ShowIf("UseRejectedShapes")]
    [SerializeField] public bool AllRejectedShapes = true;
    [SerializeField] public bool IncreaseStandardsIfBadShapes = false;

    [Header("Iterations")]
    [SerializeField] public int baseGenerations = 10;
    [ShowIf(nameof(UseRejectedShapes))]
    [SerializeField] public int generationsToForceStop = 12;
    [SerializeField] public int initalRandomShapes = 90;
    [SerializeField] public int randomShapesIfBadShapes = 10;
    [SerializeField] public int shapeVariants = 5;
    [SerializeField] public int shapeVariantsIfGoodShapes = 7;
    [SerializeField] public int maxShapes = 7; // 4
    [SerializeField] public bool ContinueIteratingIfShapeIsBest = false;

    public float ChanceToChangeSprite = 0.5f;
    public float ChanceToFlipSprite = 0.1f;
    public float rotationScalar = 0.05f;
    public float positionScalar = 0.02f;
    public float colorScalar = 0.04f;
    public float opacityScalar = 0.04f;
    public float sizeScalar = 0.04f;

    [Header("Size")]
    public float MaxShapeSize = 5;
    public float MinShapeSize = 0.2f;
    public bool PreserveAspectRatio = true;
    [Tooltip("Increase this variable to make shapes generally smaller. Scale is normalized and set to the power if this variable")]
    public float SmallShapesSizePreference = 2.75f; 

    [Header("Alpha")]
    public float minAlpha = 0.25f;
    public float maxAlpha = 1.25f;

    [Header("Colors")]
    public bool AverageColorMask = false;
    [ShowIf("AverageColorMask")]
    public bool ApplyAverageToVariants = false;
    //[HideIf("AverageColorMask")]
    [Tooltip("Literally any color")]
    public bool FullyRandomColor = false;
    //[HideIf(EConditionOperator.Or, "AverageColorMask", "FullyRandomColor")]
    public bool AnyRandomColorFromImage = true;
    // TODO: sample from image position?
    // TODO: make this an enum bro,,, and an array for different shape types

    [Header("Other")]
    public bool randomizeZOrder = false;
    [ShowIf("randomizeZOrder")]
    public int MaxZOrder = 100;

    [Header("Sprites")]
    [SerializeField] public List<Sprite> shapeSprites;

    #region Debug Stats
    public Dictionary<Sprite, int> IconUseCounts;

    //TODO: sort by sprite name btn

    //TODO: sort by sprite use counts btn

    [Button]
    public void UpdateIconUsageStats()
    {
        // add and remove guys
    }

    [Button]
    public void ResetIconUsageStats()
    {
        IconUseCounts = new();
        for (int i = 0; i < shapeSprites.Count; i++)
        {
            IconUseCounts.Add(shapeSprites[i], 0);
        }
    }
    #endregion
}
