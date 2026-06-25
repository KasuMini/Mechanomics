using UnityEngine;

[CreateAssetMenu(fileName = "Event", menuName = "Mechanomics/Event", order = 0)]
public class EventData : ScriptableObject
{
    [Header("Identity")]
    public string title;
    [TextArea(2, 5)] public string description;

    [Header("Logistics")]
    [Range(1, 4)] public int suggestedSize = 1;

    [SerializeReference, SubclassSelector]
    public EventBehaviour behaviour;

    public string Summary => behaviour != null ? behaviour.Summary : "(no behaviour)";

    public EventOutcome Resolve(IMechStats mech, System.Random rng) => behaviour.Resolve(mech, rng);

    public static EventData Create(string title, string description, int suggestedSize, EventBehaviour behaviour)
    {
        var data = CreateInstance<EventData>();
        data.title = title;
        data.description = description;
        data.suggestedSize = suggestedSize;
        data.behaviour = behaviour;
        return data;
    }
}
