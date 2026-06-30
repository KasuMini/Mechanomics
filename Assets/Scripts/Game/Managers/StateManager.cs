using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StateManager : MonoBehaviour
{
    public static StateManager Instance { get; set; }
    public enum GameplayState
    {
        Tutorial,
        CityActive,
        ShiftOver,
        EndState,
    }

    public int currentScene;
    public GameplayState currentState;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }
    void Start()
    {
        UpdateScene();
    }

    void Update()
    {

    }
    
    public void UpdateScene()
    {
        int sceneIndex = SceneManager.GetActiveScene().buildIndex;
        if (sceneIndex <= 3)
        {
            SceneManager.LoadScene(sceneIndex + 1);
        }
        else if (sceneIndex == 4)
        {
            SceneManager.LoadScene(currentState != GameplayState.EndState ? 2 : 5);
        }
    }

    public void EndTutorial()
    {
        currentState = GameplayState.CityActive;
    }

    // Called at the end of a City day: advance the day, then go to Preparation (or EndScreen when the run is over).
    public void EndDay()
    {
        RunState rs = GameManager.Instance.runState;
        rs.AdvanceDay();
        SceneManager.LoadScene(rs.IsRunOver ? "EndScreen" : "Preparation");
    }

    public void UpdateSceneDelay()
    {
        Invoke(nameof(UpdateScene), 5f);
    }
}
