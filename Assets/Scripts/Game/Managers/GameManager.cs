using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; set; }

    [SerializeField] private RunConfig config;
    public StateManager stateManager;

    // The live run for the session (held in RunState.Active, surviving scene loads).
    public RunState runState => RunState.Active;

    void Awake()
    {
        // One persistent GameManager (it's DontDestroyOnLoad) - destroy scene duplicates.
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        // No active run yet -> start a new one from config; otherwise resume the run in progress.
        if (RunState.Active == null)
        {
            RunState.Active = new RunState(config);
            if (PremadeEvents.Instance != null) PremadeEvents.Instance.ResetPool();
        }
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}
