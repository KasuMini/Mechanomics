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

    // Ray from a full-screen pixel position, correct under a letterboxed camera.rect.
    // (Camera.ScreenPointToRay ignores the viewport offset, so go via the viewport.)
    public static Ray ScreenRay(Camera cam, Vector2 screenPos)
    {
        Rect vp = cam.pixelRect;
        Vector2 v = new Vector2((screenPos.x - vp.x) / Mathf.Max(1f, vp.width),
                                (screenPos.y - vp.y) / Mathf.Max(1f, vp.height));
        return cam.ViewportPointToRay(v);
    }
}
