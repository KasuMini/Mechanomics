using System;
using System.Collections.Generic;
using UnityEngine;

public class DispatchManager : MonoBehaviour
{
    [SerializeField] private RunState runState;
    [SerializeField] private List<EventData> todaysEvents = new List<EventData>();

    public IReadOnlyList<EventData> TodaysEvents => todaysEvents;

    public event Action<EventData, MechData, EventOutcome> EventResolved;

    private readonly System.Random rng = new System.Random();

    public EventOutcome Dispatch(EventData job, MechData mech)
    {
        if (job == null) throw new ArgumentNullException(nameof(job));
        if (mech == null) throw new ArgumentNullException(nameof(mech));

        EventOutcome outcome = job.Resolve(mech, rng);

        if (runState != null)
        {
            runState.AddCash(outcome.CashDelta);
        }

        EventResolved?.Invoke(job, mech, outcome);
        return outcome;
    }
}
