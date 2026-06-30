using UnityEngine;
using UnityEngine.SceneManagement;

public class StateManager : MonoBehaviour
{

    public enum GameplayState
    {
        InDialogue,
        CityActive,
        ShiftOver,
        EndState,
    }

    public int currentScene;
    public GameplayState currentState;

    void Start()
    {
        UpdateScene();
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

    // Called at the end of a City day: advance the day, then go to Preparation (or EndScreen when the run is over).
    public void EndDay()
    {
        RunState rs = GameManager.Instance.runState;
        rs.AdvanceDay();
        SceneManager.LoadScene(rs.IsRunOver ? "EndScreen" : "Preparation");
    }
}
