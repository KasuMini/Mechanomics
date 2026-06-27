using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Event", menuName = "Mechanomics/Event", order = 0)]
public class EventData : ScriptableObject
{
    [Header("Identity")]
    public string title;
    [TextArea(2, 5)] public string description;

    [Header("Logistics")]
    [Range(1, 4)] public int suggestedSize = 1;

    [SerializeReference]
    public EventBehaviour behaviour;

    public string Summary => behaviour != null ? behaviour.Summary : "(no behaviour)";

    public EventOutcome Resolve(IReadOnlyList<IMechStats> mechs, System.Random rng) => behaviour.Resolve(mechs, rng);

    public string Preview(IReadOnlyList<IMechStats> selected) => behaviour != null ? behaviour.Preview(selected) : "(no behaviour)";

    public static EventData Create(string title, string description, int suggestedSize, EventBehaviour behaviour)
    {
        var data = CreateInstance<EventData>();
        data.name = title;              // Object name -> what inspector reference fields show
        data.title = title;
        data.description = description;
        data.suggestedSize = suggestedSize;
        data.behaviour = behaviour;
        return data;
    }
}
