using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using TMPro;
using UnityEditor;
using UnityEngine;
using System.Threading;

public class CandidateController : MonoBehaviour
{
    [SerializeField] private TMP_Text accuracy_text;

    private CameraController cameraController;
    private ShapeController shapeController;

    [System.Obsolete]
    [ReadOnly] public float Accuracy;

    // Start is called before the first frame update
    void Awake()
    {
        cameraController = GetComponent<CameraController>();
        shapeController = GetComponent<ShapeController>();
    }

    // EvolutionManager > CandidateManager
    public void Tick()
    {
        //shapeController.TryCreateNewShape();
        
    }

    #region obsolete

    
    #region Accuracy

    #endregion

    #endregion

}
