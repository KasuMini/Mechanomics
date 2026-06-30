using UnityEngine;

// Pure map-space helpers shared by the City view, graph, and dispatch.
// Map coords are normalized UV (0..1, origin bottom-left); local coords are
// centred on the map root so the tilt/scale lives entirely on that transform.
public static class CityMapMath
{
    // Normalized map UV -> local position on the (untilted) map plane.
    public static Vector3 UvToLocal(Vector2 uv, float widthUnits, float heightUnits)
    {
        return new Vector3((uv.x - 0.5f) * widthUnits, (uv.y - 0.5f) * heightUnits, 0f);
    }

    // Inverse of UvToLocal - local plane position back to normalized UV.
    public static Vector2 LocalToUv(Vector3 local, float widthUnits, float heightUnits)
    {
        return new Vector2(local.x / widthUnits + 0.5f, local.y / heightUnits + 0.5f);
    }

    // Ray-cast point-in-polygon test (works in any 2D space, e.g. footprint UV).
    // Used to resolve a click on the map to the building whose footprint it hits.
    public static bool PointInPolygon(Vector2 p, System.Collections.Generic.IList<Vector2> poly)
    {
        bool inside = false;
        int n = poly.Count;
        for (int i = 0, j = n - 1; i < n; j = i++)
        {
            if (((poly[i].y > p.y) != (poly[j].y > p.y)) &&
                (p.x < (poly[j].x - poly[i].x) * (p.y - poly[i].y) / (poly[j].y - poly[i].y) + poly[i].x))
                inside = !inside;
        }
        return inside;
    }
}
