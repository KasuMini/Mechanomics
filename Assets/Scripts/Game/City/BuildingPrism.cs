using System.Collections.Generic;
using UnityEngine;

// A building defined by a hand-drawn footprint polygon (in map UV) extruded up a
// height. Rebuilds its mesh from the polygon through the parent CityMapView so it
// tracks the map size/tilt. Replaces the old axis-aligned box Building.
[ExecuteAlways]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class BuildingPrism : MonoBehaviour
{
    [Tooltip("Footprint polygon in normalized map UV (0..1), in order around the block.")]
    public List<Vector2> footprintUv = new List<Vector2>();

    [Tooltip("Extrusion height in world units (up the tilted board).")]
    public float height = 0.3f;

    public bool isHQ;

    Material baseMaterial;    // the original block material
    Material primary;         // current main material (base or active-event)
    Material outlineMat;      // hover-outline overlay, or null
    MeshRenderer rendererCache;

    CityMapView View => GetComponentInParent<CityMapView>();
    MeshRenderer Renderer => rendererCache != null ? rendererCache : (rendererCache = GetComponent<MeshRenderer>());

    void EnsurePrimary()
    {
        if (primary == null) { primary = Renderer.sharedMaterial; baseMaterial = primary; }
    }

    // Light the block up as an active event; ClearHighlight restores the base look.
    public void SetActiveHighlight(Material activeMat)
    {
        EnsurePrimary();
        primary = activeMat != null ? activeMat : baseMaterial;
        ApplyMaterials();
    }

    public void ClearHighlight()
    {
        EnsurePrimary();
        primary = baseMaterial;
        ApplyMaterials();
    }

    // Add a hover-outline overlay material (null to remove it). Coexists with highlight.
    public void SetOutlined(Material outline)
    {
        EnsurePrimary();
        outlineMat = outline;
        ApplyMaterials();
    }

    void ApplyMaterials()
    {
        Renderer.sharedMaterials = outlineMat != null
            ? new[] { primary, outlineMat }
            : new[] { primary };
    }

    public Vector2 CentroidUv
    {
        get
        {
            if (footprintUv.Count == 0) return new Vector2(0.5f, 0.5f);
            Vector2 c = Vector2.zero;
            foreach (var p in footprintUv) c += p;
            return c / footprintUv.Count;
        }
    }

    public Vector3 CentroidWorld
    {
        get { CityMapView v = View; return v != null ? v.MapToWorld(CentroidUv) : transform.position; }
    }

    void OnEnable() => Rebuild();
    void OnValidate() => Rebuild();

    public void Rebuild()
    {
        CityMapView view = View;
        if (view == null || footprintUv.Count < 3) return;

        var local = new List<Vector2>(footprintUv.Count);
        foreach (var uv in footprintUv)
        {
            Vector3 l = view.MapToLocal(uv);   // root-local plane point
            local.Add(new Vector2(l.x, l.y));
        }

        PrismMesh.Data d = PrismMesh.Build(local, Mathf.Max(0.001f, height));
        var mf = GetComponent<MeshFilter>();
        Mesh mesh = mf.sharedMesh;
        if (mesh == null || mesh.name != "PrismMesh")
        {
            mesh = new Mesh { name = "PrismMesh" };
            mf.sharedMesh = mesh;
        }
        mesh.Clear();
        mesh.vertices = d.vertices;
        mesh.normals = d.normals;
        mesh.triangles = d.triangles;
        if (d.smoothNormals != null) mesh.SetUVs(1, d.smoothNormals);   // UV1 = smooth normals for the outline hull
        mesh.RecalculateBounds();
        transform.localPosition = Vector3.zero;   // mesh is authored in root-local space

        // keep the raycast collider (added at runtime by EventScheduler) in sync with the mesh
        var mc = GetComponent<MeshCollider>();
        if (mc != null) mc.sharedMesh = mesh;
    }
}
