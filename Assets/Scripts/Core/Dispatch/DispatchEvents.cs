using System;

public static class DispatchEvents
{
    public static event Action<EventData, EventOutcome> Succeeded;
    public static event Action<EventData, EventOutcome> Failed;

    public static void Raise(EventData job, EventOutcome outcome){
        if (outcome.Success)    Succeeded?.Invoke(job, outcome);
        else                    Failed?.Invoke(job, outcome);
    }    
}
