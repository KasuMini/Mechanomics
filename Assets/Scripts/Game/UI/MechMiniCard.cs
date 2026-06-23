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

    Image bg;
    Color normalColor;

    void Awake()
    {
        bg = GetComponent<Image>();
        if (bg != null) normalColor = bg.color;
        GetComponent<Button>().onClick.AddListener(() => Clicked?.Invoke(this));
    }

    public void Bind(MechData mech)
    {
        Mech = mech;
        model.text = mech.mechName;
        stats.text = $"AGI {mech.agilityStat}  STR {mech.strengthStat}  SYS {mech.systemsStat}  REL {mech.reliabilityStat}";
    }

    public void SetSelected(bool on)
    {
        if (bg != null) bg.color = on ? new Color(0.18f, 0.42f, 0.30f, 1f) : normalColor;
    }
}
