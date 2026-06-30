using System.Collections.Generic;
using UnityEngine;

// Pure road-network graph: nodes at 2D positions with undirected edges weighted
// by Euclidean distance. Dijkstra shortest path drives mech dispatch routing
// (travel time = path length / speed). Node counts are small, so an O(n^2)
// Dijkstra is plenty and keeps the code dependency-free and testable.
public class CityGraph
{
    readonly Vector2[] nodes;
    readonly List<int>[] adj;

    public int NodeCount => nodes.Length;
    public Vector2 Position(int i) => nodes[i];

    public CityGraph(Vector2[] nodePositions, IList<Vector2Int> edges)
    {
        nodes = nodePositions ?? new Vector2[0];
        adj = new List<int>[nodes.Length];
        for (int i = 0; i < nodes.Length; i++) adj[i] = new List<int>();
        if (edges != null)
            foreach (var e in edges)
                if (Valid(e.x) && Valid(e.y) && e.x != e.y)
                {
                    if (!adj[e.x].Contains(e.y)) adj[e.x].Add(e.y);
                    if (!adj[e.y].Contains(e.x)) adj[e.y].Add(e.x);
                }
    }

    bool Valid(int i) => i >= 0 && i < nodes.Length;

    // Index of the node closest to an arbitrary position (e.g. a building centroid).
    public int Nearest(Vector2 p) => NearestIndex(nodes, p);

    // Index of the point closest to p (shared by world-space and UV-space callers).
    public static int NearestIndex(IList<Vector2> points, Vector2 p)
    {
        int best = -1;
        float bestD = float.MaxValue;
        for (int i = 0; i < points.Count; i++)
        {
            float d = (points[i] - p).sqrMagnitude;
            if (d < bestD) { bestD = d; best = i; }
        }
        return best;
    }

    // Dijkstra between node indices. Returns the node-index path (inclusive of
    // from and to) and sets length; returns an empty list if unreachable.
    public List<int> ShortestPath(int from, int to, out float length)
    {
        length = 0f;
        var path = new List<int>();
        if (!Valid(from) || !Valid(to)) return path;
        if (from == to) { path.Add(from); return path; }

        int n = nodes.Length;
        var dist = new float[n];
        var prev = new int[n];
        var done = new bool[n];
        for (int i = 0; i < n; i++) { dist[i] = float.MaxValue; prev[i] = -1; }
        dist[from] = 0f;

        for (int it = 0; it < n; it++)
        {
            int u = -1;
            float best = float.MaxValue;
            for (int i = 0; i < n; i++) if (!done[i] && dist[i] < best) { best = dist[i]; u = i; }
            if (u == -1) break;
            done[u] = true;
            if (u == to) break;
            foreach (int v in adj[u])
            {
                float w = Vector2.Distance(nodes[u], nodes[v]);
                if (dist[u] + w < dist[v]) { dist[v] = dist[u] + w; prev[v] = u; }
            }
        }

        if (dist[to] == float.MaxValue) return path;   // unreachable
        length = dist[to];
        for (int at = to; at != -1; at = prev[at]) path.Add(at);
        path.Reverse();
        return path;
    }
}
