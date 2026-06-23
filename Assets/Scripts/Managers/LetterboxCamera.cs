using UnityEngine;

// Keeps a fixed 16:9 frame by letterboxing the camera viewport; scales smoothly (non-integer).
[RequireComponent(typeof(Camera))]
public class LetterboxCamera : MonoBehaviour
{
    public float targetAspect = 16f / 9f;

    Camera cam;
    int lastW, lastH;

    void Awake()
    {
        cam = GetComponent<Camera>();
        CreateBars();
        Apply();
    }

    void Update()
    {
        if (Screen.width != lastW || Screen.height != lastH) Apply();
    }

    // A camera behind the main one that clears the whole screen black, so the bars are black.
    void CreateBars()
    {
        var go = new GameObject("LetterboxBars");
        go.transform.SetParent(transform, false);
        var bg = go.AddComponent<Camera>();
        bg.clearFlags = CameraClearFlags.SolidColor;
        bg.backgroundColor = Color.black;
        bg.cullingMask = 0;
        bg.depth = cam.depth - 1;
    }

    void Apply()
    {
        lastW = Screen.width;
        lastH = Screen.height;
        float window = (float)Screen.width / Mathf.Max(1, Screen.height);
        float s = window / targetAspect;
        cam.rect = s < 1f
            ? new Rect(0f, (1f - s) * 0.5f, 1f, s)
            : new Rect((1f - 1f / s) * 0.5f, 0f, 1f / s, 1f);
    }
}
