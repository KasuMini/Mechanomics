using TMPro;
using UnityEngine;

// Pure view for an EventData (title, description, and the behaviour's summary).
public class EventDisplay : MonoBehaviour
{
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI summaryText;

    public void Show(EventData ev)
    {
        if (ev == null) return;
        if (titleText != null) titleText.text = ev.title;
        if (descriptionText != null) descriptionText.text = ev.description;
        if (summaryText != null) summaryText.text = ev.Summary;
    }
}
