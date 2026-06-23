using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public StateManager stateManager;
    public RunState runState;

    void Awake() => Instance = this;

    // Fresh run each session (the RunState SO persists across scene loads and play sessions).
    void Start()
    {
        if (runState != null) runState.ResetRun();
    }
}
