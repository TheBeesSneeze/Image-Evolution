using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RenderTextureArea : MonoBehaviour
{
    // like a dog
    private AspectRatioFitter arf;
    private RawImage rawImage;

    private void Awake()
    {
        arf = GetComponent<AspectRatioFitter>();    
        rawImage = GetComponent<RawImage>();

    }

    // Start is called before the first frame update
    void Start()
    {
        EvolutionManager.Instance.OnRefreshImage.AddListener(Rescale);
    }

    void Rescale()
    {
        rawImage.texture = CameraManager.Instance.renderTexture;

        arf.aspectRatio = (float)CameraManager.Instance.renderTexture.width / (float)CameraManager.Instance.renderTexture.height;
    }
}
