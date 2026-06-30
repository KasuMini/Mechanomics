using UnityEngine;
using UnityEngine.UI;

// A small countdown ring that floats over an active-event building at all times
// (independent of the popup). The scheduler clones one per live event, binds it
// to the building + clock, and destroys it on expiry.
public class EventMarker : MonoBehaviour
{
    public RectTransform rect;
    public RectTransform canvasRect;
    public Camera uiCamera;
    public Image ring;

    BuildingPrism building;
    CityClock clock;
    float spawnHour, expireHour;

    public void Bind(BuildingPrism b, CityClock c, float spawn, float expire)
    {
        building = b;
        clock = c;
        spawnHour = spawn;
        expireHour = expire;
        gameObject.SetActive(true);
    }

    void Update()
    {
        if (building == null) return;

        if (MapUi.WorldToCanvasLocal(uiCamera, canvasRect, building.CentroidWorld, out Vector2 local))
            rect.anchoredPosition = local;

        if (ring != null)
            ring.fillAmount = CityClock.Fraction(clock != null ? clock.Now : spawnHour, spawnHour, expireHour);
    }
}
