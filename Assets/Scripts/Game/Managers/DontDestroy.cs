using UnityEngine;

public class DontDestroy : MonoBehaviour
{
    void Awake()
    {
        DontDestroyOnLoad(this);
    }

    void Update()
    {
        
    }
}
