using TMPro;
using UnityEngine;

public class MarketDisplay : MonoBehaviour
{
    #region Variables
    public MechMarket mechMarket;
    public int cardID;
    public MechData mechData;
    public string mechName;
    public string pilotName;
    public int mechAgi;
    public int mechStr;
    public int mechSys;
    public int mechRel;
    public int mechSize;

    public TextMeshProUGUI model;
    public TextMeshProUGUI pilot;
    public TextMeshProUGUI stats;

    #endregion
    void Start()
    {
        #region Setting variables to available mechs ID
        mechData = mechMarket.availableMechs[cardID];
        mechName = mechData.mechName;
        pilotName = mechData.pilotName;
        mechAgi = mechData.agilityStat;
        mechStr = mechData.strengthStat;
        mechSys = mechData.systemsStat;
        mechRel = mechData.reliabilityStat;
        mechSize = mechData.size;
        #endregion



    }

    void Update()
    {
        
    }
}
