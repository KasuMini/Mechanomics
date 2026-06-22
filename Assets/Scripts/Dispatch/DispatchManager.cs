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

        int statValue = GetStat(mech, job.testedStat);
        EventOutcome outcome = EventResolver.Resolve(job, statValue, mech.reliabilityStat, rng);

        if (runState != null)
        {
            runState.AddCash(outcome.CashDelta);
        }

        EventResolved?.Invoke(job, mech, outcome);
        return outcome;
    }

    private static int GetStat(MechData mech, MechStat stat)
    {
        switch (stat)
        {
            case MechStat.Agility: return mech.agilityStat;
            case MechStat.Strength: return mech.strengthStat;
            case MechStat.Systems: return mech.systemsStat;
            default: return 0;
        }
    }
}
