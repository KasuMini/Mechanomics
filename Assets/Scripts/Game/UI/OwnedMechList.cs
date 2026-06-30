using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// The owned-mech bar. Each mech is a card (front layer) standing on a platform (back layer).
// Positions live in RunState's inventory (discrete slots); cards/platforms just lerp toward
// the screen position of their slot - they don't know why a slot changed. Dragging follows
// the cursor and live-reflows the slots (push neighbours aside); the dragged card's lerp is
// suspended until release, then it eases home onto its platform.
public class OwnedMechList : MonoBehaviour
{
    public MechMiniCard cardPrefab;
    public MechSpriteLibrary sprites;     // (size, variant) -> mech sprite
    public Sprite[] platformSprites;      // index size-1: small / medium / large
    public Transform content;             // existing wiring; its parent (Viewport) is the track
    public float mechBaseY = 16f;         // how high the mech stands above the bar bottom
    public float lerpSpeed = 18f;         // snappy

    public HashSet<MechData> Selected { get; } = new HashSet<MechData>();
    public event Action SelectionChanged;

    RunState runState;
    RectTransform track, platformsLayer, mechsLayer;
    readonly Dictionary<MechData, MechMiniCard> cardByMech = new Dictionary<MechData, MechMiniCard>();
    readonly Dictionary<MechData, LerpToTarget> platformByMech = new Dictionary<MechData, LerpToTarget>();

    Vector2 dragGrab;

    public float NotchWidth => track != null ? track.rect.width / MechData.TrackNotches : 0f;

    void Awake()
    {
        track = content != null ? content.parent as RectTransform : (RectTransform)transform;
        var scroll = GetComponent<ScrollRect>(); if (scroll != null) scroll.enabled = false;
        var mask = track != null ? track.GetComponent<RectMask2D>() : null; if (mask != null) mask.enabled = false;
        DisableLayout(content);
        platformsLayer = MakeLayer("Platforms");   // first sibling -> behind
        mechsLayer = MakeLayer("Mechs");           // second sibling -> in front
    }

    RectTransform MakeLayer(string layerName)
    {
        var go = new GameObject(layerName, typeof(RectTransform));
        var rt = (RectTransform)go.transform;
        rt.SetParent(track, false);
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        rt.pivot = Vector2.zero;
        return rt;
    }

    void Start()
    {
        runState = GameManager.Instance != null ? GameManager.Instance.runState : null;
        if (runState != null) runState.OwnedMechsChanged += Sync;
        Sync();
    }

    void OnDestroy()
    {
        if (runState != null) runState.OwnedMechsChanged -= Sync;
    }

    // Reconcile cards/platforms with the inventory contents (add/remove); positions are the
    // animator's job each frame.
    public void Sync()
    {
        if (track == null || cardPrefab == null || runState == null) return;
        var owned = new HashSet<MechData>(runState.OwnedMechs);

        var gone = new List<MechData>();
        foreach (var kv in cardByMech) if (!owned.Contains(kv.Key)) gone.Add(kv.Key);
        foreach (var mech in gone) RemoveMech(mech);

        foreach (var mech in owned)
        {
            if (cardByMech.TryGetValue(mech, out var card)) card.Bind(mech, SpriteFor(mech));
            else AddMech(mech);
        }
        SelectionChanged?.Invoke();
    }

    Sprite SpriteFor(MechData mech) => sprites != null ? sprites.Get(mech.size, mech.variant) : null;

    Sprite PlatformFor(MechData mech)
    {
        if (platformSprites == null || platformSprites.Length == 0) return null;
        return platformSprites[Mathf.Clamp(mech.size - 1, 0, platformSprites.Length - 1)];
    }

    void AddMech(MechData mech)
    {
        // Platform (back layer).
        var pgo = new GameObject("Platform", typeof(RectTransform), typeof(Image));
        var prt = (RectTransform)pgo.transform;
        prt.SetParent(platformsLayer, false);
        prt.anchorMin = prt.anchorMax = Vector2.zero;
        prt.pivot = new Vector2(0.5f, 0f);
        prt.sizeDelta = new Vector2(128f, 128f);
        var pimg = pgo.GetComponent<Image>();
        pimg.sprite = PlatformFor(mech);
        pimg.raycastTarget = false;
        pimg.preserveAspect = true;
        var plerp = pgo.AddComponent<LerpToTarget>();
        plerp.speed = lerpSpeed;
        platformByMech[mech] = plerp;

        // Card (front layer).
        var card = Instantiate(cardPrefab, mechsLayer);
        var rt = (RectTransform)card.transform;
        rt.anchorMin = rt.anchorMax = Vector2.zero;
        rt.pivot = new Vector2(0.5f, 0f);
        if (card.Lerp != null) card.Lerp.speed = lerpSpeed;
        card.AttachTrack(this);
        card.Bind(mech, SpriteFor(mech));
        card.Clicked += OnCardClicked;
        cardByMech[mech] = card;

        ApplyTargets(mech, snap: true);   // snap into place so it doesn't fly in from the origin
    }

    void RemoveMech(MechData mech)
    {
        Selected.Remove(mech);
        if (cardByMech.TryGetValue(mech, out var card))
        {
            card.Clicked -= OnCardClicked;
            Destroy(card.gameObject);
        }
        cardByMech.Remove(mech);
        if (platformByMech.TryGetValue(mech, out var plat) && plat != null) Destroy(plat.gameObject);
        platformByMech.Remove(mech);
    }

    // Animator: every frame, point each card/platform at the screen position of its slot.
    void Update()
    {
        if (runState == null || track == null) return;
        foreach (var kv in cardByMech) ApplyTargets(kv.Key, snap: false);
    }

    void ApplyTargets(MechData mech, bool snap)
    {
        float center = runState.CenterOf(mech);
        if (center < 0f) return;
        float centerX = center * NotchWidth;

        if (platformByMech.TryGetValue(mech, out var plat) && plat != null)
        {
            plat.Target = new Vector2(centerX, 0f);          // flush with the bar bottom
            if (snap) plat.SnapToTarget();
        }
        if (cardByMech.TryGetValue(mech, out var card) && card.Lerp != null)
        {
            card.Lerp.Target = new Vector2(centerX, mechBaseY);
            if (snap) card.Lerp.SnapToTarget();              // suspended (dragged) cards ignore Target anyway
        }
    }

    // --- Drag: cursor-driven, live reorder ---

    // Pointer in track-local coords with a bottom-left origin (matches card/platform anchoredPosition).
    public Vector2 PointerToTrackLocal(Vector2 screenPos, Camera cam)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(track, screenPos, cam, out var lp);
        return new Vector2(lp.x + track.pivot.x * track.rect.width,
                           lp.y + track.pivot.y * track.rect.height);
    }

    public void BeginDrag(MechMiniCard card, Vector2 pointerLocal)
    {
        HideTooltip();
        var rt = (RectTransform)card.transform;
        dragGrab = pointerLocal - rt.anchoredPosition;
        if (card.Lerp != null) card.Lerp.suspended = true;   // the cursor owns the card now
        rt.SetAsLastSibling();                                // draw over its neighbours
    }

    public void Drag(MechMiniCard card, Vector2 pointerLocal)
    {
        var rt = (RectTransform)card.transform;
        rt.anchoredPosition = pointerLocal - dragGrab;        // free x + y

        float nw = NotchWidth;
        if (nw <= 0f) return;
        float centerNotches = rt.anchoredPosition.x / nw;        // dragged centre, in notch units
        runState.ReorderMech(card.Mech, centerNotches);          // others repack and re-centre around it
    }

    public void EndDrag(MechMiniCard card)
    {
        // Future: if released over an event / other drop target, hand off here instead.
        if (card.Lerp != null) card.Lerp.suspended = false;   // ease home onto its platform
    }

    public void ClearSelection()
    {
        foreach (var c in cardByMech.Values) c.SetSelected(false);
        Selected.Clear();
        SelectionChanged?.Invoke();
    }

    void OnCardClicked(MechMiniCard card)
    {
        MechData mech = card.Mech;
        if (Selected.Contains(mech)) { Selected.Remove(mech); card.SetSelected(false); }
        else { Selected.Add(mech); card.SetSelected(true); }
        SelectionChanged?.Invoke();
    }

    // --- Hover tooltip (built at runtime on the root canvas) ---

    GameObject tooltip;
    RectTransform tooltipRT;
    TextMeshProUGUI tooltipText;
    Canvas tooltipCanvas;
    RectTransform canvasRT;

    public void ShowTooltip(MechData mech, RectTransform cardRT)
    {
        if (mech == null || cardRT == null) return;
        EnsureTooltip();
        if (tooltip == null) return;
        tooltipText.text = $"<b>{mech.mechName}</b>\n{MechMiniCard.ColoredStats(mech)}";
        tooltip.SetActive(true);
        tooltip.transform.SetAsLastSibling();

        Camera cam = tooltipCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : tooltipCanvas.worldCamera;
        var corners = new Vector3[4];
        cardRT.GetWorldCorners(corners);                       // 1=top-left, 2=top-right
        Vector2 screen = RectTransformUtility.WorldToScreenPoint(cam, (corners[1] + corners[2]) * 0.5f);
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRT, screen, cam, out var local))
            tooltipRT.anchoredPosition = local + new Vector2(0f, 8f);
    }

    public void HideTooltip()
    {
        if (tooltip != null) tooltip.SetActive(false);
    }

    void EnsureTooltip()
    {
        if (tooltip != null) return;
        var canvas = GetComponentInParent<Canvas>();
        if (canvas == null) return;
        tooltipCanvas = canvas.rootCanvas != null ? canvas.rootCanvas : canvas;
        canvasRT = (RectTransform)tooltipCanvas.transform;

        tooltip = new GameObject("MechTooltip", typeof(RectTransform), typeof(Image));
        tooltipRT = (RectTransform)tooltip.transform;
        tooltipRT.SetParent(canvasRT, false);
        tooltipRT.anchorMin = tooltipRT.anchorMax = new Vector2(0.5f, 0.5f);
        tooltipRT.pivot = new Vector2(0.5f, 0f);

        var bg = tooltip.GetComponent<Image>();
        bg.color = new Color(0.05f, 0.06f, 0.08f, 0.92f);
        bg.raycastTarget = false;

        var hlg = tooltip.AddComponent<HorizontalLayoutGroup>();
        hlg.padding = new RectOffset(8, 8, 6, 6);
        hlg.childControlWidth = hlg.childControlHeight = true;
        hlg.childForceExpandWidth = hlg.childForceExpandHeight = false;
        var fitter = tooltip.AddComponent<ContentSizeFitter>();
        fitter.horizontalFit = fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        var textGO = new GameObject("Text", typeof(RectTransform));
        textGO.transform.SetParent(tooltipRT, false);
        tooltipText = textGO.AddComponent<TextMeshProUGUI>();
        tooltipText.raycastTarget = false;
        tooltipText.textWrappingMode = TextWrappingModes.NoWrap;
        tooltipText.alignment = TextAlignmentOptions.Center;
        if (cardPrefab != null && cardPrefab.model != null) tooltipText.font = cardPrefab.model.font;
        tooltipText.fontSize = 16f;

        tooltip.SetActive(false);
    }

    static void DisableLayout(Transform t)
    {
        if (t == null) return;
        var lg = t.GetComponent<HorizontalLayoutGroup>();
        if (lg != null) lg.enabled = false;
        var csf = t.GetComponent<ContentSizeFitter>();
        if (csf != null) csf.enabled = false;
    }
}
