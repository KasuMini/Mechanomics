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

    [Header("Dispatch travel")]
    public CityGraphData graph;          // road network for routing
    public MechSpriteLibrary spriteLibrary;
    public Transform runnerParent;       // where mech runners spawn (a world-space transform)
    public float travelSpeed = 1.4f;     // world units / second along the road
    public float runnerLift = 0f;        // optional camera-ward nudge; 0 = full depth (buildings occlude)
    public float convoyStagger = 0.5f;   // seconds between each mech departing on a multi-mech dispatch

    [Header("Hover")]
    public bool enableHover = true;

    [Header("Tuning")]
    public int maxConcurrent = 4;
    public float lifespanHours = 2f;
    public float spawnIntervalHours = 0.6f;
    public bool avoidHQ = true;

    class Live { public EventData data; public BuildingPrism building; public float spawn, expire; public EventMarker marker; }

    readonly List<Live> live = new List<Live>();
    Queue<EventData> pending;
    List<BuildingPrism> freeBuildings;
    BuildingPrism hq;
    BuildingPrism hovered;
    float nextSpawn;

    void Start()
    {
        freeBuildings = new List<BuildingPrism>();
        BuildingPrism mostCentral = null;
        float bestCentre = float.MaxValue;
        foreach (var b in mapView != null ? mapView.GetComponentsInChildren<BuildingPrism>(true) : new BuildingPrism[0])
        {
            if (b.isHQ) hq = b;
            if (!(avoidHQ && b.isHQ)) freeBuildings.Add(b);
            float d = (b.CentroidUv - new Vector2(0.5f, 0.5f)).sqrMagnitude;
            if (d < bestCentre) { bestCentre = d; mostCentral = b; }

            // collider so clicks/hover raycast the real box, not the flat ground plane
            var mf = b.GetComponent<MeshFilter>();
            if (mf != null && mf.sharedMesh != null)
            {
                var mc = b.GetComponent<MeshCollider>();
                if (mc == null) mc = b.gameObject.AddComponent<MeshCollider>();
                mc.sharedMesh = mf.sharedMesh;
            }
        }
        if (hq == null) hq = mostCentral;   // fallback origin until a building is flagged HQ

        pending = new Queue<EventData>(dispatch != null ? (IEnumerable<EventData>)dispatch.TodaysEvents : new EventData[0]);
        nextSpawn = clock != null ? clock.startHour : 7f;
        if (clock != null) { clock.Ticked += OnTick; clock.DayEnded += OnDayEnded; }
    }

    void OnDestroy()
    {
        if (clock != null) { clock.Ticked -= OnTick; clock.DayEnded -= OnDayEnded; }
    }

    // The city day is over when the clock runs out: bank the day, then hand off to the
    // News recap, which rolls on to Preparation (or EndScreen once the run is finished).
    void OnDayEnded()
    {
        StateManager sm = StateManager.Instance;
        if (sm == null) return;
        RunState.Active?.AdvanceDay();
        bool runOver = RunState.Active != null && RunState.Active.IsRunOver;
        sm.currentState = runOver ? StateManager.GameplayState.EndState : StateManager.GameplayState.ShiftOver;
        sm.UpdateScene();   // City -> News -> Preparation / EndScreen
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
        if (mouse == null) return;
        Vector2 screenPos = mouse.position.ReadValue();
        bool overUi = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();

        if (enableHover) UpdateHover(overUi ? (Vector2?)null : screenPos);
        if (mouse.leftButton.wasPressedThisFrame && !overUi) TryClick(screenPos);
    }

    // The building whose 3D box the screen ray actually hits (mesh colliders), or null.
    // Raycasting the geometry - not the ground plane - so the hit matches what's drawn.
    BuildingPrism BuildingAt(Vector2 screenPos)
    {
        if (Camera.main == null) return null;
        return Physics.Raycast(MapUi.ScreenRay(Camera.main, screenPos), out RaycastHit hit, 100f)
            ? hit.collider.GetComponentInParent<BuildingPrism>()
            : null;
    }

    void UpdateHover(Vector2? screenPos)
    {
        BuildingPrism over = screenPos.HasValue ? BuildingAt(screenPos.Value) : null;
        if (over == hovered) return;
        hovered = over;
        // Push the hovered building's renderer to the screen-space outline feature.
        var outline = SelectionOutlineFeature.Active;
        if (outline != null)
            outline.SetRenderers(hovered != null
                ? new Renderer[] { hovered.Renderer }
                : System.Array.Empty<Renderer>());
    }

    void TryClick(Vector2 screenPos)
    {
        BuildingPrism hit = BuildingAt(screenPos);
        if (hit == null) return;
        Live found = live.Find(x => x.building == hit);
        if (popup != null && found != null) popup.Open(found.data, hit, clock, found.spawn, found.expire, OnDispatch);
    }

    // Dispatch: send each selected mech driving from HQ along the roads to the event
    // as a staggered convoy. The event resolves only when the LAST mech arrives; each
    // mech then drives home and is freed on its own return.
    void OnDispatch(EventData ev, BuildingPrism target)
    {
        var mechs = new List<MechData>();
        if (ownedList != null) foreach (MechData m in ownedList.Selected) mechs.Add(m);
        if (mechs.Count == 0) return;   // need at least one selected mech

        // claim the event: pull it from the live pool so it stops expiring / can't be re-clicked
        Live l = live.Find(x => x.data == ev);
        if (l == null) return;
        live.Remove(l);
        if (l.marker != null) Destroy(l.marker.gameObject);
        if (popup != null) popup.Close();

        // lock the mechs and clear the bar selection
        foreach (var m in mechs) RunState.Active?.SetBusy(m, true);
        if (ownedList != null) ownedList.ClearSelection();

        BuildingPrism origin = hq != null ? hq : target;
        Vector3[] outPath = BuildPath(origin, target);
        Vector3[] backPath = BuildPath(target, origin);
        Vector3 start = outPath.Length > 0 ? outPath[0] : target.CentroidWorld;

        int total = mechs.Count;
        int[] arrived = { 0 };   // boxed counter shared by the convoy closures
        for (int i = 0; i < total; i++)
        {
            MechData mech = mechs[i];
            MechRunner runner = SpawnRunner(mech, start);
            runner.Run(outPath, travelSpeed, i * convoyStagger, () =>
            {
                if (++arrived[0] == total)   // last mech on site -> resolve the event once
                {
                    if (dispatch != null) dispatch.Dispatch(ev, mechs);
                    target.ClearHighlight();
                    freeBuildings.Add(target);
                }
                runner.Run(backPath, travelSpeed, 0f, () =>
                {
                    RunState.Active?.SetBusy(mech, false);   // this mech is home - free it
                    if (runner != null) Destroy(runner.gameObject);
                });
            });
        }
    }

    // HQ/building -> nearest road node -> ... -> nearest road node -> target, in world space.
    Vector3[] BuildPath(BuildingPrism from, BuildingPrism to)
    {
        var pts = new List<Vector3> { Lift(from.CentroidWorld) };
        if (graph != null && mapView != null && graph.nodeUvs.Count > 0)
        {
            CityGraph g = graph.BuildGraph();
            List<int> nodePath = g.ShortestPath(graph.NearestNode(from.CentroidUv), graph.NearestNode(to.CentroidUv), out _);
            foreach (int n in nodePath) pts.Add(Lift(mapView.MapToWorld(graph.nodeUvs[n])));
        }
        pts.Add(Lift(to.CentroidWorld));
        return pts.ToArray();
    }

    MechRunner SpawnRunner(MechData mech, Vector3 at)
    {
        var go = new GameObject("MechRunner");
        if (runnerParent != null) go.transform.SetParent(runnerParent, false);
        go.transform.position = at;
        var runner = go.AddComponent<MechRunner>();
        runner.view = mapView;
        Sprite s = spriteLibrary != null ? spriteLibrary.Get(mech.size, mech.variant) : null;
        runner.SetSprite(s, mech.palette != null ? mech.palette.Material : null);
        return runner;
    }

    // Pull a map point toward the camera so the upright runner draws over the buildings.
    Vector3 Lift(Vector3 p) { p.z -= runnerLift; return p; }
}
