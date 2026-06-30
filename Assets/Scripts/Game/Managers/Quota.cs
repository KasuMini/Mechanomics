using System.Reflection;
using UnityEngine;

public class Quota : MonoBehaviour
{
    public int todaysQuota;

    void Start()
    {
        CheckDay();
    }

    void Update()
    {
        
    }

    private void CheckDay()
    {
        switch (GameManager.Instance.runState.Day)
        {
            case 1:
                todaysQuota = 3;
            break;
            case 2:
                todaysQuota = 5;
            break;
            case 3:
                todaysQuota = 7;
            break;
            case 4:
                todaysQuota = 9;
            break;
            case 5:
                todaysQuota = 11;
            break;
                
        }
    }

}
