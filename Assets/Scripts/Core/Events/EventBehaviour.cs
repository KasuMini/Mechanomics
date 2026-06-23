using System;

// Base for every event type. Pick a concrete one via EventData's behaviour dropdown.
[Serializable]
public abstract class EventBehaviour
{
    public abstract EventOutcome Resolve(IMechStats mech, System.Random rng);
    public abstract string Summary { get; }
}
