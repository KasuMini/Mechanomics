using UnityEngine;

public class CashManager : MonoBehaviour
{

    public int currentCash;

    void Start()
    {
        currentCash = 10000;
    }

    void Update()
    {
        
    }

    public void UpdateCash(int deltaCash)
    {
        currentCash += deltaCash;
    }

}
