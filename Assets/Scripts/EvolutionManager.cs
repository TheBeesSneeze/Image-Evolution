using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

public class EvolutionManager : Singleton<EvolutionManager>
{
    [Header("This might get deleted, but its important")]
    public Texture2D TextureToSimulate;

    public SettingsProfile settingsProfile;

    // Call this AFTER setting TextureToSimulate
    public UnityEvent OnRefreshImage = new UnityEvent();

    // calculation variables
    int bestCandidateIdx;
    bool started = false;

    private static Color32[] targetColors {
        get {
            if(CameraManager.Instance.targetColors != null)
                return CameraManager.Instance.targetColors; 
            else
                return null;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        settingsProfile = Instantiate(settingsProfile);
        TextureToSimulate = StaticUtilites.ResizeTexture(TextureToSimulate, CameraManager.Instance.resolution, CameraManager.Instance.GenerateMipMaps);

        InitialRefreshDelay();
    }

    async void InitialRefreshDelay()
    {
        await Task.Delay(2500);
        Debug.Log("its show time");
        OnRefreshImage.Invoke();
        started = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (!started)
            return;

    }

    public static Color GetRandomColorFromTargetTexture()
    {
        return targetColors[Random.Range(0, targetColors.Length)];
    }
    
    

}
