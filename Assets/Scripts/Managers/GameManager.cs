using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public StateManager stateManager;
    public RunState runState;

    void Awake() => Instance = this;
}
