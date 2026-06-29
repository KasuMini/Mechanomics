using System.Collections.Generic;
using UnityEngine;

// A pool of hand-authored events. Draws (and removes) random events from a working copy
// that refills from the serialized source the first time it's used - so the pool is
// reinstated for each fresh instance (each new run / scene load).
public class PremadeEvents : MonoBehaviour
{
    [SerializeField] private List<EventData> events = new List<EventData>();

    List<EventData> pool;

    // Draw up to n distinct random events into dest, removing them from the pool.
    public void DrawInto(List<EventData> dest, int n)
    {
        if (pool == null) pool = new List<EventData>(events);
        for (int i = 0; i < n && pool.Count > 0; i++)
        {
            int idx = Random.Range(0, pool.Count);
            dest.Add(pool[idx]);
            pool.RemoveAt(idx);
        }
    }
}
