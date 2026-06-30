using System.Collections.Generic;
using UnityEngine;

// Builds an extruded-polygon prism from a footprint. Footprint points are on the
// map plane (z=0); the prism extrudes along -Z (which is "up" on the tilted board)
// to z = -height. Produces outward-facing walls + a top cap with flat per-face
// normals so the depth-shading prism shader reads each face distinctly.
public static class PrismMesh
{
    public struct Data
    {
        public Vector3[] vertices;
        public Vector3[] normals;
        public int[] triangles;
    }

    public static Data Build(IList<Vector2> footprint, float height)
    {
        int n = footprint != null ? footprint.Count : 0;
        var verts = new List<Vector3>();
        var norms = new List<Vector3>();
        var tris = new List<int>();
        if (n < 3) return new Data { vertices = verts.ToArray(), normals = norms.ToArray(), triangles = tris.ToArray() };

        // Normalise winding to CCW so each wall's outward normal is consistent
        // even for concave footprints. The centroid-based test fails on L-shapes
        // (centroid can sit so a concave edge faces inward -> culled wall).
        var fp = new List<Vector2>(footprint);
        if (PolygonTriangulator.SignedArea(fp) < 0f) fp.Reverse();

        float top = -height;

        // Walls: 4 verts per edge with a flat outward normal. For CCW winding the
        // outward (exterior) normal of edge a->b is (edge.y, -edge.x).
        for (int i = 0; i < n; i++)
        {
            Vector2 a = fp[i];
            Vector2 b = fp[(i + 1) % n];
            Vector2 edge = b - a;
            Vector3 outward = new Vector3(edge.y, -edge.x, 0f).normalized;

            int baseIdx = verts.Count;
            verts.Add(new Vector3(a.x, a.y, 0f));     // baseA
            verts.Add(new Vector3(b.x, b.y, 0f));     // baseB
            verts.Add(new Vector3(b.x, b.y, top));    // topB
            verts.Add(new Vector3(a.x, a.y, top));    // topA
            for (int k = 0; k < 4; k++) norms.Add(outward);

            // Two triangles fanned from baseA; swap the off-diagonal corners to
            // flip the winding when the quad would otherwise face inward.
            Vector3 geo = Vector3.Cross(verts[baseIdx + 1] - verts[baseIdx], verts[baseIdx + 2] - verts[baseIdx]);
            int c1 = baseIdx + 1, c3 = baseIdx + 3;
            if (Vector3.Dot(geo, outward) < 0f) { int t = c1; c1 = c3; c3 = t; }
            tris.Add(baseIdx); tris.Add(c1); tris.Add(baseIdx + 2);
            tris.Add(baseIdx); tris.Add(baseIdx + 2); tris.Add(c3);
        }

        // Top cap: flat normal toward the viewer (-Z).
        int capStart = verts.Count;
        Vector3 capN = new Vector3(0f, 0f, -1f);
        for (int i = 0; i < n; i++)
        {
            verts.Add(new Vector3(fp[i].x, fp[i].y, top));
            norms.Add(capN);
        }
        int[] capTris = PolygonTriangulator.Triangulate(fp);
        // Force every cap triangle to face the viewer (-Z) regardless of the
        // footprint's draw winding - otherwise CW-drawn blocks get a culled,
        // missing cap.
        for (int t = 0; t < capTris.Length; t += 3)
        {
            int a = capStart + capTris[t];
            int b = capStart + capTris[t + 1];
            int c = capStart + capTris[t + 2];
            float gz = Vector3.Cross(verts[b] - verts[a], verts[c] - verts[a]).z;
            if (gz < 0f) { tris.Add(a); tris.Add(b); tris.Add(c); }   // already faces -Z
            else { tris.Add(a); tris.Add(c); tris.Add(b); }            // flip to face -Z
        }

        return new Data { vertices = verts.ToArray(), normals = norms.ToArray(), triangles = tris.ToArray() };
    }
}
