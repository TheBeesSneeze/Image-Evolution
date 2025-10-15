using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

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
    //public Dictionary<Sprite, int> IconUseCounts;
    [Header("Debug")]
    [Foldout("Debug"), ShowIf(nameof(RecordIconUseFrequency))] public bool SaveToFileOnSessionEnd = true;
    [Foldout("Debug"), ShowIf(nameof(RecordIconUseFrequency))] public Dictionary<Sprite, int> IconUseCounts;
    [Foldout("Debug"), ShowIf(nameof(RecordIconUseFrequency))] [SerializeField] private TextAsset dataFile;
    [Tooltip("How many shapes until icon use stats are auto-printed")]
    [Foldout("Debug"), ShowIf(nameof(RecordIconUseFrequency))] public int debugLogFrequency=15;
    [Foldout("Debug")] public bool RecordIconUseFrequency = true;

    /// <summary>
    /// Loads IconUseCounts dictionary from json file
    /// </summary>
    [Button]
    public void LoadFromFile()
    {
        if (!RecordIconUseFrequency)
            return;

        if (dataFile == null)
        {
            Debug.LogWarning("No save file is present to load");
            IconUseCounts = new();
            return;
        }

        var stringUseCounts = JsonUtility.FromJson<SerializableDictionary<string, int>>(dataFile.text)
                                    .ToDictionary();
        foreach (Sprite sprite in shapeSprites)
        {
            if (!stringUseCounts.ContainsKey(sprite.name))
                stringUseCounts[sprite.name] = 0;
        }

        IconUseCounts = shapeSprites.Zip(stringUseCounts.Values, (key, value) => new { key, value })
                                    .ToDictionary(item => item.key, item => stringUseCounts[item.key.name]);

        Debug.Log("Successfully loaded sprite data!");
        PrintUsageStats();  
    }

    /// <summary>
    /// Saves IconUseCounts dictionary to json file
    /// </summary>
    [Button]
    public void SaveToFile()
    {
        if (!RecordIconUseFrequency)
            return;

        Dictionary<string, int> stringUseCounts = IconUseCounts.ToDictionary(item => item.Key.name, item => item.Value);

        Debug.Log(IconUseCounts.Count);
        Debug.Log(stringUseCounts.Count);

        string elemString = JsonUtility.ToJson(new SerializableDictionary<string,int>( stringUseCounts));
        string path = AssetDatabase.GetAssetPath(dataFile);

        //var textFile = File.CreateText(path);

        //textFile.WriteLine(elemString);

        Debug.Log($"Overwriting save file at {path}");
        Debug.Log(elemString);

        File.WriteAllText(path, elemString);
        AssetDatabase.Refresh();
    }

    //TODO: sort by sprite name btn

    //TODO: sort by sprite use counts btn

    [Button]
    public void ClearUsageStats()
    {
        if (!RecordIconUseFrequency)
            return;

        IconUseCounts = new();
        for (int i = 0; i < shapeSprites.Count; i++)
        {
            IconUseCounts.Add(shapeSprites[i], 0);
        }
        SaveToFile();
    }

    [Button]
    public void PrintUsageStats()
    {
        var sorted = IconUseCounts.AsEnumerable().OrderBy(i => i.Value);
        var best = sorted.LastOrDefault();
        var worst = sorted.FirstOrDefault();
        Debug.Log($"Best shape: {best.Key.name}: {best.Value} uses");

        var unused = IconUseCounts.Where(i => i.Value == 0);
        if (unused.Count() > 0)
        {
            var unusedList = String.Join("\n", unused.Select(i => i.Key.name));
            Debug.Log($"There are {unused.Count()} unused sprites:\n{unusedList}");
        }
        else
        {
            Debug.Log($"Worst shape: {worst.Key.name}: {worst.Value} uses");
        }
    }
    #endregion
}
