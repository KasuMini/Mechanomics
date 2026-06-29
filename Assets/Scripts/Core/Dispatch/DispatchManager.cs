using System;
using System.Collections.Generic;
using UnityEngine;

public class DispatchManager : MonoBehaviour
{
    [SerializeField] private List<EventData> todaysEvents;
    public EventGenerator generator;

    public IReadOnlyList<EventData> TodaysEvents => todaysEvents;

    public event Action<EventData, IReadOnlyList<MechData>, EventOutcome> EventResolved;

    private readonly System.Random rng = new System.Random();

    void Awake()
    {
        todaysEvents = new List<EventData>();
        int difficulty = RunState.Active != null ? Mathf.Max(1, RunState.Active.Day) : 1;
        for (int i = 0; i < 9; i++)
        {
            todaysEvents.Add(generator.GenerateNewEvent(difficulty));
        }

        if (PremadeEvents.Instance != null) PremadeEvents.Instance.DrawInto(todaysEvents, 2);
    }

    public EventOutcome Dispatch(EventData job, IReadOnlyList<MechData> mechs)
    {
        if (job == null) throw new ArgumentNullException(nameof(job));
        if (mechs == null) throw new ArgumentNullException(nameof(mechs));

        EventOutcome outcome = job.Resolve(mechs, rng);

        RunState.Active?.AddCash(outcome.CashDelta);

        EventResolved?.Invoke(job, mechs, outcome);
        return outcome;
    }
}
