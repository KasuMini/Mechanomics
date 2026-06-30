using UnityEngine;

// Root of the city board. Tilts the whole map away from the fixed ortho camera
// so it reads as "looking down", and maps normalized map UV to world space.
// Buildings, road nodes and mechs all position through MapToWorld so a single
// tilt/size knob moves everything together.
[ExecuteAlways]
public class CityMapView : MonoBehaviour
{
    [Tooltip("Pitch away from camera, degrees. 0 = flat to camera, 90 = true top-down.")]
    [Range(0f, 85f)] public float tilt = 55f;

    [Tooltip("World width of the map at the base of the tilt.")]
    public float mapWidthUnits = 5.5f;

    [Tooltip("World height (depth) of the map before tilt. Set from the sprite aspect.")]
    public float mapHeightUnits = 5.5f;

    [Tooltip("The textured map quad to fit to the map size.")]
    public SpriteRenderer mapPlane;

    public Vector3 MapToLocal(Vector2 uv) => CityMapMath.UvToLocal(uv, mapWidthUnits, mapHeightUnits);
    public Vector3 MapToWorld(Vector2 uv) => transform.TransformPoint(MapToLocal(uv));

    // A world point (assumed on/near the map plane) -> map UV.
    public Vector2 WorldToUv(Vector3 world)
    {
        Vector3 local = transform.InverseTransformPoint(world);
        return CityMapMath.LocalToUv(local, mapWidthUnits, mapHeightUnits);
    }

    // World ray (e.g. from a mouse click) onto the tilted map plane -> map UV.
    // Used by the draw tool and runtime building selection.
    public bool RayToUv(Ray ray, out Vector2 uv)
    {
        uv = default;
        Plane plane = new Plane(transform.TransformDirection(Vector3.forward), transform.position);
        if (!plane.Raycast(ray, out float enter)) return false;
        uv = WorldToUv(ray.GetPoint(enter));
        return true;
    }

    void OnValidate() => Apply();
    void OnEnable() => Apply();

    // Push the tilt to the root rotation and scale the plane to the map size.
    public void Apply()
    {
        transform.localRotation = Quaternion.Euler(tilt, 0f, 0f);
        if (mapPlane != null && mapPlane.sprite != null)
        {
            Vector2 native = mapPlane.sprite.bounds.size;   // world units at scale 1
            if (native.x > 0f && native.y > 0f)
                mapPlane.transform.localScale = new Vector3(
                    mapWidthUnits / native.x, mapHeightUnits / native.y, 1f);
        }
    }
}
