using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CandidateManager : Singleton<CandidateManager>
{
    [Header("Settings")]
    private float scoreDifferenceScalar = 1.5f;

    public static float ScoreDifferenceScalar;
    public static float x_gap;
    public static float y_gap;
    private float halfwidth,halfheight; 

    private CameraManager _cameraManager;

    private void Start()
    {
        _cameraManager = FindObjectOfType<CameraManager>();
        halfheight = _cameraManager.GetComponent<Camera>().orthographicSize;
        halfwidth = halfheight * _cameraManager.GetComponent<Camera>().aspect;

        ScoreDifferenceScalar = scoreDifferenceScalar;

        EvolutionManager.Instance.OnRefreshImage.AddListener(SetGaps);
    }

    void SetGaps()
    {
        halfheight = _cameraManager.GetComponent<Camera>().orthographicSize;
        halfwidth = halfheight * _cameraManager.GetComponent<Camera>().aspect;
    }
    

}
