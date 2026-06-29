using UnityEngine;

public class MarketButtons : MonoBehaviour
{
    public MechMarket market;
    public int buttonID;
    public RunState runState;
    public GameObject card;

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void Purchase()
    {
        if (runState.Cash >= market.availableMechs[buttonID].cost)
        {
            runState.TrySpend(market.availableMechs[buttonID].cost);
            runState.AddMech(market.availableMechs[buttonID]);
            card.SetActive(false);
        }
        
        
    }

}
