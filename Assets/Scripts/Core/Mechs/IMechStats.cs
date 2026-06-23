// Abstraction events depend on instead of the concrete MechData, so the Core assembly stays self-contained and testable.
public interface IMechStats
{
    int GetStat(MechStat stat);
    int Reliability { get; }
}
