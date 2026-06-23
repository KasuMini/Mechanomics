using System;
using System.Collections.Generic;
using UnityEngine;

public class DispatchManager : MonoBehaviour
{
    [SerializeField] private RunState runState;
    [SerializeField] private List<EventData> todaysEvents = new List<EventData>();

    public IReadOnlyList<EventData> TodaysEvents => todaysEvents;

    public event Action<EventData, IReadOnlyList<MechData>, EventOutcome> EventResolved;

    private readonly System.Random rng = new System.Random();

    public EventOutcome Dispatch(EventData job, IReadOnlyList<MechData> mechs)
    {
        if (job == null) throw new ArgumentNullException(nameof(job));
        if (mechs == null) throw new ArgumentNullException(nameof(mechs));

        EventOutcome outcome = job.Resolve(mechs, rng);

        if (runState != null)
        {
            runState.AddCash(outcome.CashDelta);
        }

        EventResolved?.Invoke(job, mechs, outcome);
        return outcome;
    }
}
