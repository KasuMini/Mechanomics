using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Orchestrates a City day: show the event, let the player pick a SET of owned mechs, dispatch them, end the day.
// Self-contained per event so the future scheduler (multiple events / travel time) can drive it unchanged.
public class DayController : MonoBehaviour
{
    public StateManager stateManager;
    public DispatchManager dispatch;
    public EventDisplay eventDisplay;
    public OwnedMechList ownedList;
    public Button dispatchButton;

    void Start()
    {
        if (ownedList != null) ownedList.SelectionChanged += OnSelectionChanged;
        if (dispatchButton != null) dispatchButton.onClick.AddListener(Dispatch);
        OnSelectionChanged();
    }

    void OnDestroy()
    {
        if (ownedList != null) ownedList.SelectionChanged -= OnSelectionChanged;
    }

    void OnSelectionChanged()
    {
        if (eventDisplay != null) eventDisplay.UpdatePreview(CurrentEvent(), SelectedStats());
        SetButtonInteractable(ownedList != null && ownedList.Selected.Count > 0);
    }

    List<IMechStats> SelectedStats()
    {
        var list = new List<IMechStats>();
        if (ownedList != null)
            foreach (MechData m in ownedList.Selected) list.Add(m);
        return list;
    }

    public void Dispatch()
    {
        EventData ev = CurrentEvent();
        if (ev == null || dispatch == null || ownedList == null || ownedList.Selected.Count == 0) return;

        dispatch.Dispatch(ev, new List<MechData>(ownedList.Selected));
        GameManager.Instance.stateManager.EndDay();
    }

    EventData CurrentEvent()
    {
        return dispatch != null && dispatch.TodaysEvents.Count > 0 ? dispatch.TodaysEvents[0] : null;
    }

    void SetButtonInteractable(bool on)
    {
        if (dispatchButton != null) dispatchButton.interactable = on;
    }
}
