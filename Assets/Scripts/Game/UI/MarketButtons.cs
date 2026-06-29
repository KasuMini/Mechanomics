using UnityEngine;

public class MarketButtons : MonoBehaviour
{
    public MechMarket market;
    public int buttonID;
    public GameObject card;

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void Purchase()
    {
        var runState = GameManager.Instance != null ? GameManager.Instance.runState : null;
        MechData mech = market.availableMechs[buttonID];
        if (runState == null || mech == null || !runState.CanAddMech(mech)) return; // no room -> no spend
        if (!runState.TrySpend(mech.cost)) return;                                   // can't afford -> no add
        runState.TryAddMech(mech);
        card.SetActive(false);
    }

}
