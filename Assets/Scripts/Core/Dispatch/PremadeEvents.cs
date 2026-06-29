using System;
using System.Collections.Generic;
using UnityEngine;

public class PremadeEvents : MonoBehaviour
{
    [SerializeField]
    private List<EventData> events = new List<EventData>();
    public List<EventData> availableEvents;

    void Awake()
    {
        Debug.Log(events.Count);
        availableEvents = new List<EventData>(events);
        Debug.Log($"GameManager Awake: {availableEvents.Count}");
    }

    void Update()
    {
        
    }
}
