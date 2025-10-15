using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Debugger : MonoBehaviour
{
    [InfoBox("Press R to restart scene")]
    public RenderTexture differenceRenderTexture;
    public Material differenceShader;
    public RawImage differenceImage;

    // Start is called before the first frame update
    void Start()
    {
        ShapeManager.OnAnyShapeCreated.AddListener(UpdateDifferenceTexture);

        differenceRenderTexture = new RenderTexture(EvolutionManager.Instance.TextureToSimulate.width, EvolutionManager.Instance.TextureToSimulate.height, 0);
        differenceRenderTexture.Create();
        differenceImage.texture = differenceRenderTexture;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        { 
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    private void UpdateDifferenceTexture()
    {
        //differenceShader.SetTexture("_Current_State", CameraManager.Instance.renderTexture);
        Graphics.Blit(CameraManager.Instance.renderTexture, differenceRenderTexture, differenceShader);
    }
}
