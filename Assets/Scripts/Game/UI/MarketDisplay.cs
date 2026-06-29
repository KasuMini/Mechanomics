using TMPro;
using UnityEngine;

public class MarketDisplay : MonoBehaviour
{
    #region Variables
    public MechMarket mechMarket;
    public int cardID;
    public MechData mechData;
    public TextMeshProUGUI model;
    public TextMeshProUGUI pilot;
    public TextMeshProUGUI stats;
 

    #endregion

    void Start()
    {
        mechData = mechMarket.availableMechs[cardID];
        model.text = mechData.mechName;
        pilot.text = "Pilot: " + mechData.pilotName;
        stats.text = "AGI: " + mechData.agilityStat + "<br>STR: " + mechData.strengthStat + "<br>SYS: " + mechData.systemsStat + "<br>Size: " + mechData.size + "<br>Cost: " + mechData.cost;

    }

    void Update()
    {

    }
}
