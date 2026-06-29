using System;
using System.Collections.Generic;
using UnityEngine;

public class DispatchManager : MonoBehaviour
{
    [SerializeField] private RunState runState;
    [SerializeField] private List<EventData> todaysEvents;
    public EventGenerator generator;
    public PremadeEvents premades;


    public IReadOnlyList<EventData> TodaysEvents => todaysEvents;

    public event Action<EventData, IReadOnlyList<MechData>, EventOutcome> EventResolved;

    private readonly System.Random rng = new System.Random();

    void Awake()
    {
        Debug.Log($"Dispatch Awake: {premades.availableEvents.Count}");
        todaysEvents = new List<EventData>();
        int difficulty = runState != null ? Mathf.Max(1, runState.Day) : 1;
        for (int i = 0; i < 9; i++)
        {
            todaysEvents.Add(generator.GenerateNewEvent(difficulty));
        }

        for (int i = 0; i < 2; i++)
        {
            Debug.Log($"Count before selection: {premades.availableEvents.Count}");
            int rand = UnityEngine.Random.Range(0, premades.availableEvents.Count);
            todaysEvents.Add(premades.availableEvents[rand]);
            premades.availableEvents.RemoveAt(rand);
        }
    }

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
