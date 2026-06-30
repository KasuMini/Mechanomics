using System.Collections.Generic;
using UnityEngine;

// Edit-time settings + factory for hand-drawing buildings onto the map. The
// scene-view draw tool (CityMapAuthoringEditor) calls CreateBuilding with a
// footprint polygon; height is randomised within heightRange so the skyline
// varies. Lives on the CityMap root next to CityMapView.
[RequireComponent(typeof(CityMapView))]
public class CityMapAuthoring : MonoBehaviour
{
    [Tooltip("Container the drawn buildings are parented under.")]
    public Transform buildingsParent;

    [Tooltip("Random extrusion height range for a new building.")]
    public Vector2 heightRange = new Vector2(0.22f, 0.45f);

    public Material buildingMaterial;
    public Material hqMaterial;

    public CityMapView View => GetComponent<CityMapView>();

    // Build a prism GameObject from a drawn UV polygon. Returns it so the editor
    // can register Undo / select it. Height is random unless overridden (>0).
    public BuildingPrism CreateBuilding(IList<Vector2> footprintUv, bool hq, float heightOverride = -1f)
    {
        if (footprintUv == null || footprintUv.Count < 3) return null;

        Transform parent = buildingsParent != null ? buildingsParent : transform;
        var go = new GameObject(hq ? "HQ" : "Building");
        go.transform.SetParent(parent, false);

        var bp = go.AddComponent<BuildingPrism>();
        bp.footprintUv = new List<Vector2>(footprintUv);
        bp.isHQ = hq;
        bp.height = heightOverride > 0f ? heightOverride : Random.Range(heightRange.x, heightRange.y);
        go.GetComponent<MeshRenderer>().sharedMaterial = hq ? hqMaterial : buildingMaterial;
        bp.Rebuild();
        return bp;
    }
}
