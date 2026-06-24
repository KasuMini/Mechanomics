using Unity.Android.Gradle.Manifest;
using UnityEngine;

public class EventGenerator : MonoBehaviour
{
    public LoadEventText eventText;
    public EventBehaviour behaviour;

    void Start()
    {
        
    }

    public EventData GenerateNewEvent()
    {
        int value1 = Random.Range(0, 9);
        int value2 = Random.Range(0, 7);
        string description = eventText.keyword1[value1] + " " + eventText.keyword2[value2];
        EventData data = EventData.Create("Help Requested", description, 1, behaviour);
        return data;

    }
}
