using UnityEngine;

// Eases a RectTransform's anchoredPosition toward Target with frame-rate-independent
// smoothing. Used independently by mech cards and platforms - it doesn't know why the
// target moved. Suspend it while an external driver (e.g. a drag) owns the position;
// SnapToTarget jumps instantly.
[RequireComponent(typeof(RectTransform))]
public class LerpToTarget : MonoBehaviour
{
    public Vector2 Target;
    public float speed = 18f;   // higher = snappier
    public bool suspended;      // external driver owns the position while true

    RectTransform rt;

    void Awake() => rt = (RectTransform)transform;

    void Update()
    {
        if (suspended) return;
        float k = 1f - Mathf.Exp(-speed * Time.deltaTime);
        rt.anchoredPosition = Vector2.Lerp(rt.anchoredPosition, Target, k);
    }

    public void SnapToTarget()
    {
        if (rt == null) rt = (RectTransform)transform;
        rt.anchoredPosition = Target;
    }
}
