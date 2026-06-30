using UnityEngine;

// Shared helper for placing a Screen-Space-Camera UI element over a world point
// (e.g. a building). Used by the event popup and the per-building countdown
// markers so the world->screen->canvas-local conversion lives in one place.
public static class MapUi
{
    public static bool WorldToCanvasLocal(Camera cam, RectTransform canvasRect, Vector3 world, out Vector2 local)
    {
        Vector2 screen = RectTransformUtility.WorldToScreenPoint(cam, world);
        return RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screen, cam, out local);
    }
}
