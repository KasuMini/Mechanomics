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
        MechData mech = market.availableMechs[buttonID];
        if (mech == null || !runState.CanAddMech(mech)) return;   // no room on the bar -> no spend
        if (!runState.TrySpend(mech.cost)) return;                // can't afford -> no add
        runState.TryAddMech(mech);
        card.SetActive(false);
    }

}
