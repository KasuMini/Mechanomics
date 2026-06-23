using UnityEngine;
using UnityEngine.UI;

// Orchestrates a City day: show the event, let the player pick an owned mech, dispatch it, end the day.
// Kept thin so it can grow to multiple events / multiple mechs / travel time later.
public class DayController : MonoBehaviour
{
    public DispatchManager dispatch;
    public EventDisplay eventDisplay;
    public OwnedMechList ownedList;
    public Button dispatchButton;

    MechData selected;

    void Start()
    {
        if (eventDisplay != null) eventDisplay.Show(CurrentEvent());
        if (ownedList != null) ownedList.MechClicked += OnMechClicked;
        if (dispatchButton != null) dispatchButton.onClick.AddListener(Dispatch);
        SetButtonInteractable(false);
    }

    void OnDestroy()
    {
        if (ownedList != null) ownedList.MechClicked -= OnMechClicked;
    }

    void OnMechClicked(MechData mech)
    {
        selected = mech;
        SetButtonInteractable(true);
    }

    public void Dispatch()
    {
        EventData ev = CurrentEvent();
        if (ev == null || selected == null || dispatch == null) return;

        dispatch.Dispatch(ev, selected);
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
