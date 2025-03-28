using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

public class EvolutionManager : Singleton<EvolutionManager>
{
    [Header("This might get deleted, but its important")]
    public Texture2D TextureToSimulate;

    // Call this AFTER setting TextureToSimulate
    public UnityEvent OnRefreshImage = new UnityEvent();

    // calculation variables
    int bestCandidateIdx;
    bool started = false;

    // Start is called before the first frame update
    void Start()
    {
        TextureToSimulate = StaticUtilites.ResizeTexture(TextureToSimulate, 64);

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

    [Obsolete]
    private int GetBestCandidateIndex()
    {
        /*
        int bestCandidate = 0;
        for (int i = 1; i < CandidateManager.Candidates.Count; i++)
        {
            CandidateController candidate = CandidateManager.Candidates[i];
            if (candidate.Accuracy > CandidateManager.Candidates[bestCandidate].Accuracy)
            {
                bestCandidate = i;
            }
        }
        return bestCandidate;
        */
        return -1;
    }

    
    

}
