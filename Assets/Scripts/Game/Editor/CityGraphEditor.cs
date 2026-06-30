using UnityEditor;
using UnityEngine;

// Scene-view tool for authoring the road network on CityGraphData. Default mode
// lets you drag nodes on the map; "Add Nodes" drops nodes where you click;
// "Connect" toggles a road between two clicked nodes. The network itself is
// drawn by CityGraphData.OnDrawGizmos, so it stays visible without selection.
[CustomEditor(typeof(CityGraphData))]
public class CityGraphEditor : Editor
{
    enum Mode { Off, AddNode, Connect }

    const float PickPixels = 14f;

    Mode mode = Mode.Off;
    int connectFrom = -1;

    CityGraphData Data => (CityGraphData)target;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Graph Tool", EditorStyles.boldLabel);

        using (new EditorGUILayout.HorizontalScope())
        {
            ModeToggle(Mode.AddNode, "Add Nodes");
            ModeToggle(Mode.Connect, "Connect");
        }

        if (mode == Mode.AddNode)
            EditorGUILayout.HelpBox("Click the map to drop road nodes.", MessageType.Info);
        else if (mode == Mode.Connect)
            EditorGUILayout.HelpBox("Click a node, then another, to toggle a road between them.", MessageType.Info);
        else
            EditorGUILayout.HelpBox("Drag node handles to move them. Use the buttons to add or connect.", MessageType.None);

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Clear Edges")) { Undo.RecordObject(Data, "Clear Edges"); Data.edges.Clear(); EditorUtility.SetDirty(Data); }
            if (GUILayout.Button("Clear All")) { Undo.RecordObject(Data, "Clear Graph"); Data.nodeUvs.Clear(); Data.edges.Clear(); connectFrom = -1; EditorUtility.SetDirty(Data); }
        }
        EditorGUILayout.LabelField($"Nodes: {Data.nodeUvs.Count}    Edges: {Data.edges.Count}");
    }

    void ModeToggle(Mode m, string label)
    {
        bool on = mode == m;
        bool now = GUILayout.Toggle(on, label, "Button");
        if (now != on) { mode = now ? m : Mode.Off; connectFrom = -1; SceneView.RepaintAll(); }
    }

    void OnSceneGUI()
    {
        CityGraphData data = Data;
        CityMapView view = data.GetComponent<CityMapView>();
        if (view == null) return;
        Event e = Event.current;

        DrawBuildingLinks(view, data);   // authoring feedback, only while the map is selected

        if (mode == Mode.Off)
        {
            for (int i = 0; i < data.nodeUvs.Count; i++)
            {
                Vector3 w = view.MapToWorld(data.nodeUvs[i]);
                float size = HandleUtility.GetHandleSize(w) * 0.08f;
                EditorGUI.BeginChangeCheck();
                Vector3 moved = Handles.FreeMoveHandle(w, size, Vector3.zero, Handles.SphereHandleCap);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(data, "Move Node");
                    data.nodeUvs[i] = view.WorldToUv(moved);
                    EditorUtility.SetDirty(data);
                }
            }
            return;
        }

        int id = GUIUtility.GetControlID(FocusType.Passive);
        HandleUtility.AddDefaultControl(id);

        if (e.type == EventType.MouseDown && e.button == 0)
        {
            if (mode == Mode.AddNode)
            {
                Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
                if (view.RayToUv(ray, out Vector2 uv))
                {
                    Undo.RecordObject(data, "Add Node");
                    data.nodeUvs.Add(uv);
                    EditorUtility.SetDirty(data);
                    e.Use(); SceneView.RepaintAll();
                }
            }
            else if (mode == Mode.Connect)
            {
                int hit = PickNode(view, e.mousePosition);
                if (hit >= 0)
                {
                    if (connectFrom < 0) connectFrom = hit;
                    else if (hit != connectFrom)
                    {
                        Undo.RecordObject(data, "Toggle Edge");
                        ToggleEdge(data, connectFrom, hit);
                        connectFrom = -1;
                        EditorUtility.SetDirty(data);
                    }
                    e.Use(); SceneView.RepaintAll();
                }
            }
        }

        DrawOverlay(view, data);
    }

    int PickNode(CityMapView view, Vector2 gui)
    {
        int best = -1;
        float bestD = PickPixels;
        for (int i = 0; i < Data.nodeUvs.Count; i++)
        {
            Vector2 g = HandleUtility.WorldToGUIPoint(view.MapToWorld(Data.nodeUvs[i]));
            float d = Vector2.Distance(g, gui);
            if (d <= bestD) { bestD = d; best = i; }
        }
        return best;
    }

    static void ToggleEdge(CityGraphData data, int a, int b)
    {
        for (int i = 0; i < data.edges.Count; i++)
        {
            Vector2Int e = data.edges[i];
            if ((e.x == a && e.y == b) || (e.x == b && e.y == a)) { data.edges.RemoveAt(i); return; }
        }
        data.edges.Add(new Vector2Int(a, b));
    }

    // Faint line from each building to its nearest road node (authoring feedback).
    void DrawBuildingLinks(CityMapView view, CityGraphData data)
    {
        if (data.nodeUvs.Count == 0) return;
        Handles.color = data.linkColor;
        foreach (var b in data.GetComponentsInChildren<BuildingPrism>(true))
        {
            int near = data.NearestNode(b.CentroidUv);
            if (near >= 0) Handles.DrawLine(view.MapToWorld(b.CentroidUv), view.MapToWorld(data.nodeUvs[near]));
        }
    }

    void DrawOverlay(CityMapView view, CityGraphData data)
    {
        Handles.color = Color.yellow;
        for (int i = 0; i < data.nodeUvs.Count; i++)
        {
            Vector3 w = view.MapToWorld(data.nodeUvs[i]);
            float s = HandleUtility.GetHandleSize(w) * (i == connectFrom ? 0.1f : 0.05f);
            Handles.DrawWireDisc(w, view.transform.forward, s);
        }
    }
}
