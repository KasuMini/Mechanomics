using System.Collections.Generic;

public enum OutcomeDegree { Fail, Partial, Success }

public class EventOutcome
{
    public OutcomeDegree Degree;
    public int Successes;
    public int Required;
    public int CashDelta;
    public int QuotaDelta;
    public string ResultText;
    public List<int> DisabledMechIndices = new List<int>();

    public bool Success => Degree == OutcomeDegree.Success;
}
