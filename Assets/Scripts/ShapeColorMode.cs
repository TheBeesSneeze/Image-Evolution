using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// TODO: Add biases
/// </summary>

[System.Flags]
public enum ShapeColorMode
{
    None = 0,
    AnyRandomColorFromImage = 1 << 0,
    RandomColorByPosition = 1 << 1,
    AverageColorFromTexture = 1 << 2,
    CompletelyRandom = 1 << 3,
}