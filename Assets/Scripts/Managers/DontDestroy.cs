using UnityEngine;

public class DontDestroy : MonoBehaviour
{
    public static GameObject Manager;

    void Start()
    {
        DontDestroyOnLoad(this);
    }


    void Update()
    {
        
    }
}
