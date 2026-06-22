using TMPro;
using UnityEngine;

public class MechMiniCard : MonoBehaviour
{
    public TextMeshProUGUI model;
    public TextMeshProUGUI stats;

    public void Bind(MechData mech)
    {
        model.text = mech.mechName;
        stats.text = $"AGI {mech.agilityStat}  STR {mech.strengthStat}  SYS {mech.systemsStat}  REL {mech.reliabilityStat}";
    }
}
