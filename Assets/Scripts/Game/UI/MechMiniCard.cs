using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class MechMiniCard : MonoBehaviour
{
    public TextMeshProUGUI model;
    public TextMeshProUGUI stats;

    public MechData Mech { get; private set; }
    public event Action<MechMiniCard> Clicked;

    void Awake()
    {
        GetComponent<Button>().onClick.AddListener(() => Clicked?.Invoke(this));
    }

    public void Bind(MechData mech)
    {
        Mech = mech;
        model.text = mech.mechName;
        stats.text = $"AGI {mech.agilityStat}  STR {mech.strengthStat}  SYS {mech.systemsStat}  REL {mech.reliabilityStat}";
    }
}
