using System.Collections.Generic;
using UnityEngine;

// Ear-clipping triangulator for simple (non-self-intersecting) polygons.
// Canonical FlipCode/Eberly implementation - robust for concave footprints
// (L-shapes etc). Output indices are into the original point list, wound CCW.
public static class PolygonTriangulator
{
    // Signed area; positive = counter-clockwise winding.
    public static float SignedArea(IList<Vector2> p)
    {
        float a = 0f;
        for (int i = 0; i < p.Count; i++)
        {
            Vector2 cur = p[i];
            Vector2 nxt = p[(i + 1) % p.Count];
            a += cur.x * nxt.y - nxt.x * cur.y;
        }
        return a * 0.5f;
    }

    // Returns triangle indices into the original point list (3 per triangle).
    public static int[] Triangulate(IList<Vector2> contour)
    {
        int n = contour != null ? contour.Count : 0;
        var result = new List<int>();
        if (n < 3) return result.ToArray();

        // Working index list, normalised to CCW.
        int[] V = new int[n];
        bool ccw = SignedArea(contour) > 0f;
        for (int v = 0; v < n; v++) V[v] = ccw ? v : (n - 1) - v;

        int nv = n;
        int errGuard = 2 * nv;   // bail-out for a non-simple polygon
        for (int v = nv - 1; nv > 2;)
        {
            if (errGuard-- <= 0) return result.ToArray();

            int u = v; if (nv <= u) u = 0;
            v = u + 1; if (nv <= v) v = 0;
            int w = v + 1; if (nv <= w) w = 0;

            if (Snip(contour, u, v, w, nv, V))
            {
                result.Add(V[u]); result.Add(V[v]); result.Add(V[w]);
                for (int s = v, t = v + 1; t < nv; s++, t++) V[s] = V[t];   // remove ear vertex
                nv--;
                errGuard = 2 * nv;
            }
        }
        return result.ToArray();
    }

    // True if (u,v,w) is a valid ear: convex corner with no other vertex inside.
    static bool Snip(IList<Vector2> c, int u, int v, int w, int nv, int[] V)
    {
        Vector2 a = c[V[u]];
        Vector2 b = c[V[v]];
        Vector2 d = c[V[w]];
        if (Mathf.Epsilon > ((b.x - a.x) * (d.y - a.y) - (b.y - a.y) * (d.x - a.x))) return false; // reflex/colinear
        for (int p = 0; p < nv; p++)
        {
            if (p == u || p == v || p == w) continue;
            if (InsideTriangle(a, b, d, c[V[p]])) return false;
        }
        return true;
    }

    static bool InsideTriangle(Vector2 a, Vector2 b, Vector2 c, Vector2 p)
    {
        float ax = c.x - b.x, ay = c.y - b.y;
        float bx = a.x - c.x, by = a.y - c.y;
        float cx = b.x - a.x, cy = b.y - a.y;
        float apx = p.x - a.x, apy = p.y - a.y;
        float bpx = p.x - b.x, bpy = p.y - b.y;
        float cpx = p.x - c.x, cpy = p.y - c.y;
        float aCrossBp = ax * bpy - ay * bpx;
        float cCrossAp = cx * apy - cy * apx;
        float bCrossCp = bx * cpy - by * cpx;
        return aCrossBp >= 0f && bCrossCp >= 0f && cCrossAp >= 0f;
    }
}
