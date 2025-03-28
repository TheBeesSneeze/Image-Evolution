using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CandidateManager : Singleton<CandidateManager>
{
    [Header("Settings")]
    [SerializeField] public float PercentOfShapesToTweak = 0.5f;
    [SerializeField] private int _x_slices = 10;
    [SerializeField] private int _y_slices = 10;

    public static float x_slices => (int)Instance._x_slices;
    public static float y_slices => (int)Instance._y_slices;

    public static float x_gap;
    public static float y_gap;
    private float halfwidth,halfheight; 

    private CameraManager _cameraManager;

    private void Start()
    {
        _cameraManager = FindObjectOfType<CameraManager>();
        halfheight = _cameraManager.GetComponent<Camera>().orthographicSize;
        halfwidth = halfheight * _cameraManager.GetComponent<Camera>().aspect;
        x_gap = halfwidth * 2 / ((float)_x_slices);
        y_gap = halfheight * 2 / (float)_y_slices;

        EvolutionManager.Instance.OnRefreshImage.AddListener(SetGaps);
    }

    void SetGaps()
    {
        halfheight = _cameraManager.GetComponent<Camera>().orthographicSize;
        halfwidth = halfheight * _cameraManager.GetComponent<Camera>().aspect;
        x_gap = halfwidth * 2 / ((float)_x_slices);
        y_gap = halfheight * 2 / (float)_y_slices;
    }

    private void OnDrawGizmosSelected()
    {
        
        float t = Time.time;


        for (int x = 0; x < _x_slices; x++)
        {
            float xpos = x_gap * (((float)x + t) % _x_slices);
            xpos = xpos - halfwidth;
            Debug.DrawLine(new Vector3(xpos, halfheight, 0), new Vector3(xpos, -halfheight, 0));
        }

        for (int y = 0; y < _y_slices; y++)
        {
            float ypos = y_gap * (((float)y + t) % _y_slices);
            ypos = ypos - halfheight;
            Gizmos.DrawLine(new Vector3(-halfwidth, ypos, 0), new Vector3(halfwidth, ypos, 0));
        }
    }

    

}
