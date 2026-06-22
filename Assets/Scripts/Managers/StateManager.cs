using UnityEngine;
using UnityEngine.SceneManagement;

public class StateManager : MonoBehaviour
{

    public enum GameplayState
    {
        Preparation,
        EventStage,
        EndState,
    }

    public int currentScene;
    public GameplayState currentState;

    public float timer;
    public int hour;


    void Start()
    {
        UpdateScene();
    }

    void Update()
    {
        if (currentState == GameplayState.EventStage)
        {
            timer -= Time.deltaTime;
            if (timer <= 0)
            {
                timer = 60f;
                hour++;
                if (hour > 12)
                {
                    hour = 1;
                }
                
            }
        }
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
}
