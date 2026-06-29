using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [SerializeField] private RunConfig config;
    public StateManager stateManager;

    // The live run for the session (held in RunState.Active, surviving scene loads).
    public RunState runState => RunState.Active;

    void Awake()
    {
        Instance = this;
        // No active run yet -> start a new one from config; otherwise resume the run in progress.
        if (RunState.Active == null) RunState.Active = new RunState(config);
    }
}
