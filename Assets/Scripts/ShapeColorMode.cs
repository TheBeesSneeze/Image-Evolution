using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ShapeColorMode
{
    AnyRandomColorFromImage = 0,
    AverageColorFromTexture = 1 << 0,
    RandomColorByPosition = 1 << 1,
    CompletelyRandom = 1 << 2,
}