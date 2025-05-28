using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using TMPro;

public class StatsDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text SucceededShapesText;
    [SerializeField] private TMP_Text FailedShapesText;
    [SerializeField] private TMP_Text TimeDisplayText;

    private int succeededShapesCount, failedShapesCount;
    private float timeOfLastShape;
    private int lastScore;

    // Start is called before the first frame update
    void Start()
    {
        ShapeManager.OnShapeCreated += OnShapeSucceeded;
        ShapeManager.OnShapeFailed.AddListener(OnShapeFailed);

        timeOfLastShape = Time.realtimeSinceStartup;
    }

    public void SetScoreText()
    {
        //Debug.logw
    }

    void OnShapeSucceeded(Shape newShape)
    {
        succeededShapesCount++;
        SucceededShapesText.text = "Successful shapes: " + succeededShapesCount.ToString();
        UpdateTimeText();
    }

    void OnShapeFailed()
    {
        failedShapesCount++;
        FailedShapesText.text = "Failed shapes: " + failedShapesCount.ToString();
        UpdateTimeText();
    }

    void UpdateTimeText()
    {
        TimeDisplayText.text = StaticUtilites.RoundToNPlaces(Time.realtimeSinceStartup - timeOfLastShape, 2).ToString();
        timeOfLastShape = Time.realtimeSinceStartup;
    }
}
