using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

// Drives City events over the day clock: takes the day's generated events, spawns
// them onto buildings over time (lighting each block up), expires unclaimed ones,
// and opens the map popup when an active building is clicked. Dispatching the
// selected mechs resolves the event and frees the building.
public class EventScheduler : MonoBehaviour
{
    public CityClock clock;
    public DispatchManager dispatch;
    public CityMapView mapView;
    public EventPopup popup;
    public OwnedMechList ownedList;
    public Material activeBuildingMaterial;
    public EventMarker markerTemplate;   // cloned per active event for the always-on countdown ring

    [Header("Tuning")]
    public int maxConcurrent = 4;
    public float lifespanHours = 2f;
    public float spawnIntervalHours = 0.6f;
    public bool avoidHQ = true;

    class Live { public EventData data; public BuildingPrism building; public float spawn, expire; public EventMarker marker; }

    readonly List<Live> live = new List<Live>();
    Queue<EventData> pending;
    List<BuildingPrism> freeBuildings;
    float nextSpawn;

    void Start()
    {
        freeBuildings = new List<BuildingPrism>();
        if (mapView != null)
            foreach (var b in mapView.GetComponentsInChildren<BuildingPrism>(true))
                if (!(avoidHQ && b.isHQ)) freeBuildings.Add(b);

        pending = new Queue<EventData>(dispatch != null ? (IEnumerable<EventData>)dispatch.TodaysEvents : new EventData[0]);
        nextSpawn = clock != null ? clock.startHour : 7f;
        if (clock != null) clock.Ticked += OnTick;
    }

    void OnDestroy()
    {
        if (clock != null) clock.Ticked -= OnTick;
    }

    void OnTick(float now)
    {
        for (int i = live.Count - 1; i >= 0; i--)
            if (now >= live[i].expire) { Expire(live[i]); live.RemoveAt(i); }

        if (now >= nextSpawn && live.Count < maxConcurrent && pending.Count > 0 && freeBuildings.Count > 0)
        {
            Spawn(now);
            nextSpawn = now + spawnIntervalHours;
        }
    }

    void Spawn(float now)
    {
        EventData data = pending.Dequeue();
        int idx = Random.Range(0, freeBuildings.Count);
        BuildingPrism b = freeBuildings[idx];
        freeBuildings.RemoveAt(idx);

        var l = new Live { data = data, building = b, spawn = now, expire = now + lifespanHours };
        b.SetActiveHighlight(activeBuildingMaterial);
        if (markerTemplate != null)
        {
            l.marker = Instantiate(markerTemplate, markerTemplate.transform.parent);
            l.marker.Bind(b, clock, l.spawn, l.expire);
        }
        live.Add(l);
    }

    void Expire(Live l)
    {
        l.building.ClearHighlight();
        freeBuildings.Add(l.building);
        if (l.marker != null) Destroy(l.marker.gameObject);
        if (popup != null && popup.Current == l.data) popup.Close();
    }

    void Update()
    {
        Mouse mouse = Mouse.current;
        if (mouse == null || !mouse.leftButton.wasPressedThisFrame) return;
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return; // let UI handle its own clicks
        TryClick(mouse.position.ReadValue());
    }

    void TryClick(Vector2 screenPos)
    {
        if (mapView == null || Camera.main == null) return;
        if (!mapView.RayToUv(Camera.main.ScreenPointToRay(screenPos), out Vector2 uv)) return;
        foreach (Live l in live)
            if (CityMapMath.PointInPolygon(uv, l.building.footprintUv))
            {
                if (popup != null) popup.Open(l.data, l.building, clock, l.spawn, l.expire, OnDispatch);
                return;
            }
    }

    void OnDispatch(EventData ev, BuildingPrism b)
    {
        var mechs = new List<MechData>();
        if (ownedList != null) foreach (MechData m in ownedList.Selected) mechs.Add(m);
        if (mechs.Count == 0) return;   // need at least one selected mech

        dispatch.Dispatch(ev, mechs);
        for (int i = live.Count - 1; i >= 0; i--)
            if (live[i].data == ev) { Expire(live[i]); live.RemoveAt(i); break; }
        if (popup != null) popup.Close();
    }
}
