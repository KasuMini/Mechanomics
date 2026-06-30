using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class PrismMeshTests
{
    static List<Vector2> UnitSquare() => new List<Vector2>
    {
        new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1)
    };

    [Test]
    public void SignedArea_CcwSquare_IsPositiveAndHalfCorrect()
    {
        Assert.AreEqual(1f, PolygonTriangulator.SignedArea(UnitSquare()), 1e-4f);
    }

    [Test]
    public void Triangulate_Square_GivesTwoTriangles()
    {
        int[] tris = PolygonTriangulator.Triangulate(UnitSquare());
        Assert.AreEqual(6, tris.Length); // 2 triangles
    }

    [Test]
    public void Triangulate_Pentagon_GivesThreeTriangles()
    {
        var poly = new List<Vector2>
        {
            new Vector2(0, 0), new Vector2(2, 0), new Vector2(3, 2),
            new Vector2(1, 3), new Vector2(-1, 2)
        };
        int[] tris = PolygonTriangulator.Triangulate(poly);
        Assert.AreEqual((poly.Count - 2) * 3, tris.Length); // n-2 triangles
    }

    [Test]
    public void Triangulate_ConcaveLShape_CoversFullArea()
    {
        // L-shaped (concave) footprint, CCW.
        var poly = new List<Vector2>
        {
            new Vector2(0, 0), new Vector2(2, 0), new Vector2(2, 1),
            new Vector2(1, 1), new Vector2(1, 2), new Vector2(0, 2)
        };
        int[] tris = PolygonTriangulator.Triangulate(poly);
        Assert.AreEqual((poly.Count - 2) * 3, tris.Length); // 4 triangles, no failed snips

        // Triangle areas must sum to the polygon area (no gaps/overlap, correct winding).
        float triArea = 0f;
        for (int i = 0; i < tris.Length; i += 3)
        {
            Vector2 a = poly[tris[i]], b = poly[tris[i + 1]], c = poly[tris[i + 2]];
            triArea += Mathf.Abs((b.x - a.x) * (c.y - a.y) - (b.y - a.y) * (c.x - a.x)) * 0.5f;
        }
        Assert.AreEqual(Mathf.Abs(PolygonTriangulator.SignedArea(poly)), triArea, 1e-3f); // L area = 3
    }

    [Test]
    public void Build_Square_HasWallAndCapVertices()
    {
        PrismMesh.Data d = PrismMesh.Build(UnitSquare(), 0.5f);
        Assert.AreEqual(20, d.vertices.Length);          // 4 edges*4 + 4 cap
        Assert.AreEqual(30, d.triangles.Length);         // 8 wall tris + 2 cap tris
        Assert.AreEqual(d.vertices.Length, d.normals.Length);
    }

    [Test]
    public void Build_ExtrudesAlongNegativeZ()
    {
        PrismMesh.Data d = PrismMesh.Build(UnitSquare(), 0.5f);
        float minZ = 0f, maxZ = 0f;
        foreach (var v in d.vertices) { minZ = Mathf.Min(minZ, v.z); maxZ = Mathf.Max(maxZ, v.z); }
        Assert.AreEqual(-0.5f, minZ, 1e-4f);   // top cap sits at -height
        Assert.AreEqual(0f, maxZ, 1e-4f);      // base on the plane
    }

    [Test]
    public void Build_CapFacesViewer_RegardlessOfDrawWinding()
    {
        var ccw = UnitSquare();
        var cw = new List<Vector2>(ccw); cw.Reverse();
        foreach (var poly in new[] { ccw, cw })
        {
            PrismMesh.Data d = PrismMesh.Build(poly, 0.5f);
            int capStart = 4 * poly.Count;   // cap verts come after the wall verts
            int capTris = 0;
            for (int i = 0; i < d.triangles.Length; i += 3)
            {
                int t0 = d.triangles[i], t1 = d.triangles[i + 1], t2 = d.triangles[i + 2];
                if (t0 < capStart || t1 < capStart || t2 < capStart) continue; // wall tri
                capTris++;
                Vector3 a = d.vertices[t0], b = d.vertices[t1], c = d.vertices[t2];
                float gz = Vector3.Cross(b - a, c - a).z;
                Assert.Less(gz, 0f, "cap triangle must face -Z (the viewer)");
            }
            Assert.AreEqual(poly.Count - 2, capTris); // all cap triangles present
        }
    }

    [Test]
    public void Build_ConcaveLShape_WallsFaceOutward()
    {
        // L-shape (CCW). The concave edge (2,1)->(1,1) must have outward normal +Y
        // (into the notch), and every wall triangle must wind to face its normal.
        var poly = new List<Vector2>
        {
            new Vector2(0, 0), new Vector2(2, 0), new Vector2(2, 1),
            new Vector2(1, 1), new Vector2(1, 2), new Vector2(0, 2)
        };
        PrismMesh.Data d = PrismMesh.Build(poly, 0.5f);

        // edge index 2 is (2,1)->(1,1); its wall verts start at 2*4 = 8.
        Vector3 concaveNormal = d.normals[8];
        Assert.Less(Vector3.Distance(concaveNormal, new Vector3(0, 1, 0)), 1e-3f, "concave edge wall must face +Y (outward)");

        int wallVertEnd = 4 * poly.Count;
        for (int i = 0; i < d.triangles.Length; i += 3)
        {
            int t0 = d.triangles[i], t1 = d.triangles[i + 1], t2 = d.triangles[i + 2];
            if (t0 >= wallVertEnd || t1 >= wallVertEnd || t2 >= wallVertEnd) continue; // cap tri
            Vector3 a = d.vertices[t0], b = d.vertices[t1], c = d.vertices[t2];
            Vector3 geo = Vector3.Cross(b - a, c - a);
            Assert.Greater(Vector3.Dot(geo.normalized, d.normals[t0]), 0.5f, "wall must wind to face its outward normal");
        }
    }

    [Test]
    public void Build_AllTriangleIndicesInRange()
    {
        PrismMesh.Data d = PrismMesh.Build(UnitSquare(), 1f);
        foreach (int i in d.triangles) Assert.IsTrue(i >= 0 && i < d.vertices.Length);
    }
}
