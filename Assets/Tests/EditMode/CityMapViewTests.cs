using NUnit.Framework;
using UnityEngine;

public class CityMapMathTests
{
    [Test]
    public void Centre_MapsToOrigin()
    {
        Vector3 p = CityMapMath.UvToLocal(new Vector2(0.5f, 0.5f), 6f, 4f);
        Assert.AreEqual(0f, p.x, 1e-4f);
        Assert.AreEqual(0f, p.y, 1e-4f);
        Assert.AreEqual(0f, p.z, 1e-4f);
    }

    [Test]
    public void Corners_SpanFullSizeCentredOnOrigin()
    {
        Vector3 bl = CityMapMath.UvToLocal(new Vector2(0f, 0f), 6f, 4f);
        Vector3 tr = CityMapMath.UvToLocal(new Vector2(1f, 1f), 6f, 4f);
        Assert.AreEqual(-3f, bl.x, 1e-4f);
        Assert.AreEqual(-2f, bl.y, 1e-4f);
        Assert.AreEqual(3f, tr.x, 1e-4f);
        Assert.AreEqual(2f, tr.y, 1e-4f);
    }

    [Test]
    public void Uv_IsLinearInBothAxes()
    {
        Vector3 p = CityMapMath.UvToLocal(new Vector2(0.75f, 0.25f), 8f, 8f);
        Assert.AreEqual(2f, p.x, 1e-4f);   // (0.75-0.5)*8
        Assert.AreEqual(-2f, p.y, 1e-4f);  // (0.25-0.5)*8
    }

    [Test]
    public void PointInPolygon_InsideAndOutside()
    {
        var quad = new System.Collections.Generic.List<Vector2>
        {
            new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1)
        };
        Assert.IsTrue(CityMapMath.PointInPolygon(new Vector2(0.5f, 0.5f), quad));
        Assert.IsFalse(CityMapMath.PointInPolygon(new Vector2(1.5f, 0.5f), quad));
        Assert.IsFalse(CityMapMath.PointInPolygon(new Vector2(-0.1f, 0.5f), quad));
    }
}
