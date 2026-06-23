using System;
using System.Collections.Generic;

// Base for every event type. Pick a concrete one via EventData's behaviour dropdown.
[Serializable]
public abstract class EventBehaviour
{
    public abstract EventOutcome Resolve(IReadOnlyList<IMechStats> mechs, System.Random rng);
    public abstract string Summary { get; }
}
