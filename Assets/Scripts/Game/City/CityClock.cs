using System;
using UnityEngine;

// The City day clock. Promotes the old cosmetic StateManager timer into the real
// master clock that drives event spawning, expiry, and (later) mech travel time.
// Runs from startHour to endHour at secondsPerHour real seconds per in-game hour.
public class CityClock : MonoBehaviour
{
    public float startHour = 7f;
    public float endHour = 12f;
    [Tooltip("Real seconds per in-game hour.")]
    public float secondsPerHour = 24f;
    public bool autoRun = true;

    public float Now { get; private set; }       // continuous hours, [startHour, endHour]
    public int Hour => Mathf.FloorToInt(Now);
    public bool DayOver { get; private set; }

    public event Action<float> Ticked;            // every frame while running
    public event Action DayEnded;                 // once, when Now reaches endHour

    // Remaining fraction (1 at spawn -> 0 at expire) for a timed thing on the clock.
    public static float Fraction(float now, float spawn, float expire)
        => Mathf.Clamp01((expire - now) / Mathf.Max(0.0001f, expire - spawn));

    void OnEnable()
    {
        Now = startHour;
        DayOver = false;
    }

    void Update()
    {
        if (!autoRun || DayOver) return;
        Now += Time.deltaTime / Mathf.Max(0.01f, secondsPerHour);
        if (Now >= endHour)
        {
            Now = endHour;
            DayOver = true;
            Ticked?.Invoke(Now);
            DayEnded?.Invoke();
            return;
        }
        Ticked?.Invoke(Now);
    }
}
