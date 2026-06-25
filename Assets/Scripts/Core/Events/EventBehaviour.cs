using System;
using System.Collections.Generic;

// Base for every event type. Pick a concrete one via EventData's behaviour dropdown.
[Serializable]
public abstract class EventBehaviour
{
    public abstract EventOutcome Resolve(IReadOnlyList<IMechStats> mechs, System.Random rng);
    public abstract string Summary { get; }

    // Live, human-readable breakdown of how the selected mechs measure up (requirement rows + success %).
    public abstract string Preview(IReadOnlyList<IMechStats> selected);
}
