using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

// Scene-view polygon draw tool for laying buildings onto the city map.
// Toggle a draw mode in the inspector, then click points on the tilted map to
// trace a block's footprint; Enter extrudes it (random height), Backspace undoes
// the last point, Esc cancels. Clicks within SnapPixels of an existing corner
// (or an in-progress point) snap to it, so neighbouring blocks share vertices.
[CustomEditor(typeof(CityMapAuthoring))]
public class CityMapAuthoringEditor : Editor
{
    enum Mode { Off, Building, HQ }

    const float SnapPixels = 12f;   // screen-space snap radius

    static readonly Color FaintCorner = new Color(1f, 1f, 1f, 0.35f);
    static readonly Color BuildingLine = new Color(0.5f, 0.9f, 1f);
    static readonly Color HqLine = new Color(1f, 0.4f, 0.4f);

    Mode mode = Mode.Off;
    readonly List<Vector2> points = new List<Vector2>();   // footprint in map UV

    CityMapAuthoring Auth => (CityMapAuthoring)target;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Draw Tool", EditorStyles.boldLabel);

        using (new EditorGUILayout.HorizontalScope())
        {
            DrawModeToggle(Mode.Building, "Draw Building");
            DrawModeToggle(Mode.HQ, "Draw HQ");
        }

        if (mode != Mode.Off)
        {
            EditorGUILayout.HelpBox(
                "Click the map to add footprint points (snaps to nearby corners).\n" +
                "Enter = extrude   Backspace = undo point   Esc = cancel\n" +
                $"Points: {points.Count}", MessageType.Info);
        }
    }

    void DrawModeToggle(Mode m, string label)
    {
        bool on = mode == m;
        bool now = GUILayout.Toggle(on, label, "Button");
        if (now != on)
        {
            mode = now ? m : Mode.Off;
            points.Clear();
            SceneView.RepaintAll();
        }
    }

    void OnSceneGUI()
    {
        if (mode == Mode.Off) return;
        CityMapView view = Auth.View;
        if (view == null) return;

        // Gather snap targets once per repaint: existing footprint corners (drawn
        // as faint dots) followed by the in-progress polygon points.
        var corners = new List<Vector2>();
        foreach (BuildingPrism bp in Auth.GetComponentsInChildren<BuildingPrism>(true))
            corners.AddRange(bp.footprintUv);
        int existingCount = corners.Count;
        corners.AddRange(points);

        Event e = Event.current;
        int id = GUIUtility.GetControlID(FocusType.Passive);
        HandleUtility.AddDefaultControl(id);   // swallow clicks so we don't select other objects

        if (e.type == EventType.MouseDown && e.button == 0)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            if (view.RayToUv(ray, out Vector2 uv))
            {
                if (TrySnap(e.mousePosition, view, corners, out Vector2 snapUv)) uv = snapUv;
                points.Add(uv);
                e.Use();
                SceneView.RepaintAll();
            }
        }
        else if (e.type == EventType.MouseMove)
        {
            SceneView.RepaintAll();   // live snap-hover feedback
        }
        else if (e.type == EventType.KeyDown)
        {
            if (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter) { Finalize(); e.Use(); }
            else if (e.keyCode == KeyCode.Backspace && points.Count > 0) { points.RemoveAt(points.Count - 1); e.Use(); SceneView.RepaintAll(); }
            else if (e.keyCode == KeyCode.Escape) { points.Clear(); e.Use(); SceneView.RepaintAll(); }
        }

        DrawPreview(view, e.mousePosition, corners, existingCount);
    }

    // Nearest snap corner to a screen point, if within SnapPixels.
    bool TrySnap(Vector2 guiPoint, CityMapView view, List<Vector2> corners, out Vector2 uv)
    {
        uv = default;
        float best = SnapPixels;
        bool found = false;
        foreach (Vector2 c in corners)
        {
            Vector2 g = HandleUtility.WorldToGUIPoint(view.MapToWorld(c));
            float d = Vector2.Distance(g, guiPoint);
            if (d <= best) { best = d; uv = c; found = true; }
        }
        return found;
    }

    void DrawPreview(CityMapView view, Vector2 mouseGui, List<Vector2> corners, int existingCount)
    {
        // Existing corners, faint, so blocks can be aligned to them.
        Handles.color = FaintCorner;
        for (int i = 0; i < existingCount; i++)
        {
            Vector3 w = view.MapToWorld(corners[i]);
            Handles.DrawSolidDisc(w, view.transform.forward, HandleUtility.GetHandleSize(w) * 0.025f);
        }

        // The polygon being drawn.
        Handles.color = mode == Mode.HQ ? HqLine : BuildingLine;
        var world = new Vector3[points.Count];
        for (int i = 0; i < points.Count; i++)
        {
            world[i] = view.MapToWorld(points[i]);
            Handles.SphereHandleCap(0, world[i], Quaternion.identity, HandleUtility.GetHandleSize(world[i]) * 0.06f, EventType.Repaint);
        }
        if (points.Count >= 2) Handles.DrawAAPolyLine(3f, world);
        if (points.Count >= 3)
            Handles.DrawAAPolyLine(3f, new Vector3[] { world[points.Count - 1], world[0] });

        // Highlight the corner the cursor would snap to.
        if (TrySnap(mouseGui, view, corners, out Vector2 snapUv))
        {
            Vector3 w = view.MapToWorld(snapUv);
            Handles.color = Color.yellow;
            Handles.DrawWireDisc(w, view.transform.forward, HandleUtility.GetHandleSize(w) * 0.09f);
        }
    }

    void Finalize()
    {
        if (points.Count < 3) return;
        BuildingPrism bp = Auth.CreateBuilding(points, mode == Mode.HQ);
        if (bp != null)
        {
            Undo.RegisterCreatedObjectUndo(bp.gameObject, "Draw Building");
            EditorUtility.SetDirty(Auth);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        }
        points.Clear();
        SceneView.RepaintAll();
    }
}
