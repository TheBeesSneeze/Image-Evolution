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
        //settingsProfile = Instantiate(settingsProfile);
        TextureToSimulate = StaticUtilities.ResizeTexture(TextureToSimulate, CameraManager.Instance.resolution, CameraManager.Instance.GenerateMipMaps);

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

    public static Color GetRandomColorFromTargetTextureNearPoint(Vector2 percentagePoint)
    {
        // TODO: make this a setting
        float hardCodedDistance = 0.1f;
        percentagePoint += new Vector2(UnityEngine.Random.Range(-hardCodedDistance, hardCodedDistance), UnityEngine.Random.Range(-hardCodedDistance, hardCodedDistance));
        percentagePoint = percentagePoint.Clamp(Vector2.zero, Vector2.one);

        int x = (int)(Instance.TextureToSimulate.width * percentagePoint.x);
        int y = (int)(Instance.TextureToSimulate.height * percentagePoint.y);



        return targetColors[(y* Instance.TextureToSimulate.height) + x];
    }

}
