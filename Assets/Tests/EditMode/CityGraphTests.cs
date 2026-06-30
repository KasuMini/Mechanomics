using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class CityGraphTests
{
    static Vector2[] Square() => new[]
    {
        new Vector2(0, 0), new Vector2(2, 0), new Vector2(2, 2), new Vector2(0, 2)
    };

    static List<Vector2Int> Perimeter() => new List<Vector2Int>
    {
        new Vector2Int(0, 1), new Vector2Int(1, 2), new Vector2Int(2, 3), new Vector2Int(3, 0)
    };

    [Test]
    public void ShortestPath_AroundPerimeter_HasLengthFour()
    {
        var g = new CityGraph(Square(), Perimeter());
        List<int> path = g.ShortestPath(0, 2, out float len);
        Assert.AreEqual(4f, len, 1e-3f);          // two unit-2 edges
        Assert.AreEqual(3, path.Count);           // from, corner, to
        Assert.AreEqual(0, path[0]);
        Assert.AreEqual(2, path[2]);
    }

    [Test]
    public void ShortestPath_PrefersDiagonalShortcut()
    {
        var edges = Perimeter();
        edges.Add(new Vector2Int(0, 2));          // diagonal shortcut
        var g = new CityGraph(Square(), edges);
        List<int> path = g.ShortestPath(0, 2, out float len);
        Assert.AreEqual(Mathf.Sqrt(8f), len, 1e-3f);  // ~2.83 direct
        Assert.AreEqual(2, path.Count);               // [0, 2]
    }

    [Test]
    public void ShortestPath_Unreachable_ReturnsEmpty()
    {
        var nodes = new[] { new Vector2(0, 0), new Vector2(1, 0), new Vector2(5, 5) };
        var edges = new List<Vector2Int> { new Vector2Int(0, 1) }; // node 2 isolated
        var g = new CityGraph(nodes, edges);
        List<int> path = g.ShortestPath(0, 2, out float len);
        Assert.AreEqual(0, path.Count);
        Assert.AreEqual(0f, len);
    }

    [Test]
    public void Nearest_ReturnsClosestNode()
    {
        var g = new CityGraph(Square(), Perimeter());
        Assert.AreEqual(0, g.Nearest(new Vector2(0.1f, 0.1f)));
        Assert.AreEqual(2, g.Nearest(new Vector2(1.9f, 1.8f)));
    }

    [Test]
    public void SamePath_FromEqualsTo_SingleNode()
    {
        var g = new CityGraph(Square(), Perimeter());
        List<int> path = g.ShortestPath(1, 1, out float len);
        Assert.AreEqual(1, path.Count);
        Assert.AreEqual(0f, len);
    }
}
