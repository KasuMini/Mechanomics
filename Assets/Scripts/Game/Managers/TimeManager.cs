using UnityEngine;
using static StateManager;

public class TimeManager : MonoBehaviour
{
    public float timer;
    public int hour;
    public bool hasRun;

    void Start()
    {
        hasRun = false;
        hour = 7;
        timer = 60f;
    }

    void Update()
    {
        if (StateManager.Instance.currentState == GameplayState.CityActive)
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
        if (hour == 5 && !hasRun)
        {
            EndShift();
        }
    }

    public void EndShift()
    {
        hasRun = true;
        StateManager.Instance.currentState = GameplayState.ShiftOver;
        StateManager.Instance.UpdateSceneDelay();
    }
}
