using Mechanomics;
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
        runState.AddMech(market.availableMechs[buttonID]);
        card.SetActive(false);
        
    }

}
