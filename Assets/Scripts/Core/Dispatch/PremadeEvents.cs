using System.Collections.Generic;
using UnityEngine;

// A pool of hand-authored events. Its GameManager is DontDestroyOnLoad, so this instance
// persists into City (where DispatchManager runs) - reach it via Instance rather than a
// scene reference. The pool depletes as events are drawn; reset on a new run.
public class PremadeEvents : MonoBehaviour
{
    public static PremadeEvents Instance { get; private set; }

    [SerializeField] private List<EventData> events = new List<EventData>();
    private List<EventData> pool;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    // Discard the working pool so the next draw reseeds a full set (call on a new run).
    public void ResetPool() => pool = null;

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
