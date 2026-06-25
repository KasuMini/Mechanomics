public enum OutcomeDegree { Fail, Partial, Success }

public class EventOutcome
{
    public OutcomeDegree Degree;
    public float Chance;
    public int CashDelta;
    public int QuotaDelta;
    public string ResultText;

    public bool Success => Degree == OutcomeDegree.Success;
}
