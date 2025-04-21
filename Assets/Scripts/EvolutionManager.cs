using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using Unity.Collections;


public class EvolutionManager : Singleton<EvolutionManager>
{
    [Header("This might get deleted, but its important")]
    public Texture2D TextureToSimulate;

    // Call this AFTER setting TextureToSimulate
    public UnityEvent OnRefreshImage = new UnityEvent();

    // calculation variables
    int bestCandidateIdx;
    bool started = false;

    private static NativeArray<Color32> targetColors =>
        CameraManager.Instance.targetColors;

    public static void BeginGeneration(Texture2D texture)
    {
        texture = StaticUtilites.ResizeTexture(texture, CameraManager.Instance.resolution, CameraManager.Instance.GenerateMipMaps);
        Instance.TextureToSimulate = texture;

        Instance.started = true;
        Instance.OnRefreshImage.Invoke();

        ShapeManager.Instance.StartMakingShapes();
    }

    public static Color GetRandomColorFromTargetTexture()
    {
        return targetColors[Random.Range(0, targetColors.Length)];
    }

}
