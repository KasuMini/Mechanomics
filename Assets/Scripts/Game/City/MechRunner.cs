using System;
using UnityEngine;

// A mech sprite that drives along a world-point path (the road route) at a fixed
// speed, then fires a callback. The sprite stands perpendicular to the tilted city
// plane (its up follows the plane's extrude direction, like the buildings), and is
// stretched vertically by 1/sin(tilt) to undo the foreshortening so it reads at its
// full native (1x) pixel height. Rendered at 1x, not the 2x UI scale.
[RequireComponent(typeof(SpriteRenderer))]
public class MechRunner : MonoBehaviour
{
    public SpriteRenderer sr;
    public CityMapView view;   // supplies the map tilt for the orientation/stretch

    Vector3[] path;
    int seg;
    float segT;
    float speed;
    float startDelay;
    Action onDone;

    void Reset() => sr = GetComponent<SpriteRenderer>();

    public void SetSprite(Sprite sprite, Material paletteMat)
    {
        if (sr == null) sr = GetComponent<SpriteRenderer>();
        sr.sprite = sprite;
        if (paletteMat != null) sr.sharedMaterial = paletteMat;
        sr.sortingOrder = 50;   // orders vs the map sprite / other runners (depth vs buildings is ZTest)
        ApplyGroundOrientation();
    }

    // Stand the sprite perpendicular to the tilted plane and pre-stretch its height.
    public void ApplyGroundOrientation()
    {
        float tilt = view != null ? view.tilt : 0f;
        transform.localRotation = Quaternion.Euler(tilt - 90f, 0f, 0f);
        float s = Mathf.Sin(tilt * Mathf.Deg2Rad);
        float vy = s > 0.01f ? 1f / s : 1f;
        transform.localScale = new Vector3(1f, vy, 1f);
    }

    // Drive along worldPath at unitsPerSecond after waiting delay seconds, then call done.
    public void Run(Vector3[] worldPath, float unitsPerSecond, float delay, Action done)
    {
        path = worldPath;
        speed = Mathf.Max(0.01f, unitsPerSecond);
        startDelay = Mathf.Max(0f, delay);
        onDone = done;
        seg = 0;
        segT = 0f;
        if (path != null && path.Length > 0) transform.position = path[0];
    }

    void Update()
    {
        if (path == null || seg >= path.Length - 1) return;
        if (startDelay > 0f) { startDelay -= Time.deltaTime; return; }   // wait at the start (convoy stagger)

        Vector3 a = path[seg], b = path[seg + 1];
        float len = Vector3.Distance(a, b);
        if (len < 1e-4f) { seg++; return; }

        segT += speed * Time.deltaTime / len;
        while (segT >= 1f && seg < path.Length - 1)
        {
            segT -= 1f;
            seg++;
            a = path[Mathf.Min(seg, path.Length - 1)];
            b = path[Mathf.Min(seg + 1, path.Length - 1)];
        }

        if (seg >= path.Length - 1)
        {
            transform.position = path[path.Length - 1];
            path = null;
            Action d = onDone; onDone = null;
            d?.Invoke();
            return;
        }

        transform.position = Vector3.Lerp(path[seg], path[seg + 1], segT);
        if (sr != null) sr.flipX = (path[seg + 1].x - path[seg].x) < 0f;   // face travel direction
    }
}
