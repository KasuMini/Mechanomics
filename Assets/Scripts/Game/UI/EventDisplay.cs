using System.Collections.Generic;
using TMPro;
using UnityEngine;

// Pure view for an EventData: title, description, and the live requirement/success preview for the picked mechs.
public class EventDisplay : MonoBehaviour
{
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI summaryText;

    public void UpdatePreview(EventData ev, IReadOnlyList<IMechStats> selected)
    {
        if (ev == null) return;
        if (titleText != null) titleText.text = ev.title;
        if (descriptionText != null) descriptionText.text = ev.description;
        if (summaryText != null) summaryText.text = ev.Preview(selected);
    }
}
