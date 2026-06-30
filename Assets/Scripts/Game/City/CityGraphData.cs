using System.Collections.Generic;
using UnityEngine;

// Authored road network living on the CityMap root. Stores road nodes (in map UV)
// and undirected edges; builds a pure CityGraph for pathfinding and draws the
// network as scene gizmos (nodes, node-node edges, and each building's link to
// its nearest node). Edited with the CityGraphData scene tool.
[ExecuteAlways]
[RequireComponent(typeof(CityMapView))]
public class CityGraphData : MonoBehaviour
{
    public List<Vector2> nodeUvs = new List<Vector2>();       // road nodes in map UV
    public List<Vector2Int> edges = new List<Vector2Int>();   // node index pairs

    [Header("Gizmos")]
    public bool drawGizmos = true;
    public float nodeRadius = 0.04f;
    public Color nodeColor = new Color(1f, 0.85f, 0.2f);
    public Color edgeColor = new Color(1f, 0.7f, 0.1f, 0.95f);
    public Color linkColor = new Color(0.45f, 0.8f, 1f, 0.5f);

    CityMapView View => GetComponent<CityMapView>();

    public int NearestNode(Vector2 uv) => CityGraph.NearestIndex(nodeUvs, uv);

    // Pathfinding graph weighted in world space (so the tilt/scale affects costs).
    public CityGraph BuildGraph()
    {
        CityMapView view = View;
        var pts = new Vector2[nodeUvs.Count];
        for (int i = 0; i < nodeUvs.Count; i++)
        {
            Vector3 w = view != null ? view.MapToWorld(nodeUvs[i]) : (Vector3)nodeUvs[i];
            pts[i] = new Vector2(w.x, w.y);
        }
        return new CityGraph(pts, edges);
    }

    // Always-on road-network gizmo (nodes + edges only - cheap). The per-building
    // link visualisation is authoring feedback, drawn by CityGraphEditor when the
    // map is selected rather than every repaint here.
    void OnDrawGizmos()
    {
        CityMapView view = View;
        if (!drawGizmos || view == null) return;

        // Road edges.
        Gizmos.color = edgeColor;
        foreach (var e in edges)
        {
            if (!InRange(e.x) || !InRange(e.y)) continue;
            Gizmos.DrawLine(view.MapToWorld(nodeUvs[e.x]), view.MapToWorld(nodeUvs[e.y]));
        }

        // Road nodes.
        Gizmos.color = nodeColor;
        foreach (var uv in nodeUvs) Gizmos.DrawSphere(view.MapToWorld(uv), nodeRadius);
    }

    bool InRange(int i) => i >= 0 && i < nodeUvs.Count;
}
