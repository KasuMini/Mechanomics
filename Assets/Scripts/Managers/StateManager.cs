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

    void Start()
    {
        currentScene = SceneManager.GetActiveScene().buildIndex;
        if (currentScene == 0)
        {
            UpdateScene();
            // this is a fix to make sure currentScene is properly updated when moving to title
            currentScene++;
        }
    }

    void Update()
    {
        
    }
    
    public void UpdateScene()
    {
        switch (currentScene)
        {
            case 0:
                currentScene++;
                SceneManager.LoadScene(currentScene);
            break;

            case 1:
                currentScene++;
                SceneManager.LoadScene(currentScene);
            break;

            case 2:
                currentScene++;
                SceneManager.LoadScene(currentScene);
            break;

            case 3:
                currentScene++;
                SceneManager.LoadScene(currentScene);
            break;

            case 4:
                if (currentState != GameplayState.EndState)
                {
                    SceneManager.LoadScene(2);
                }
                else
                { SceneManager.LoadScene(5); 
                }
            break;
        }
    }
}
