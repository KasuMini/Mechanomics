using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Renders the owned mechs as a fixed 24-notch track and acts as a multi-select picker.
// The track is a pure reflection of RunState's inventory: card width = span * notch,
// card x = startNotch * notch. Cards drag-snap to free notches; positions live in the
// inventory, not here.
public class OwnedMechList : MonoBehaviour
{
    public MechMiniCard cardPrefab;
    public Transform content;          // existing wiring; its parent (Viewport) is the track
    public float cardInset = 6f;       // px shrink so neighbouring cards show a gap
    public Color notchColor = new Color(1f, 1f, 1f, 0.12f);

    public HashSet<MechData> Selected { get; } = new HashSet<MechData>();
    public event Action SelectionChanged;

    RunState runState;
    RectTransform track;
    readonly Dictionary<MechData, MechMiniCard> cardByMech = new Dictionary<MechData, MechMiniCard>();
    readonly List<RectTransform> notches = new List<RectTransform>();

    public float NotchWidth => track != null ? track.rect.width / MechData.TrackNotches : 0f;

    void Awake()
    {
        track = content != null ? content.parent as RectTransform : (RectTransform)transform;

        // The track is fixed-width and absolutely positioned — kill the old auto-layout/scroll.
        var scroll = GetComponent<ScrollRect>();
        if (scroll != null) scroll.enabled = false;
        DisableLayout(content);
        EnsureNotches();
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

    // Reconcile cards with the inventory: add new, remove gone, re-place everything.
    public void Sync()
    {
        if (track == null || cardPrefab == null || runState == null) return;

        var owned = new HashSet<MechData>(runState.OwnedMechs);

        // Remove cards whose mech left the inventory.
        var gone = new List<MechData>();
        foreach (var kv in cardByMech)
            if (!owned.Contains(kv.Key)) gone.Add(kv.Key);
        foreach (var mech in gone) RemoveCard(mech);

        // Add cards for newly owned mechs.
        foreach (var mech in owned)
            if (!cardByMech.ContainsKey(mech)) AddCard(mech);

        RelayoutAll();
        SelectionChanged?.Invoke();
    }

    void AddCard(MechData mech)
    {
        var card = Instantiate(cardPrefab, track);
        var rt = (RectTransform)card.transform;          // anchor/pivot are fixed for a card's life
        rt.anchorMin = rt.anchorMax = new Vector2(0f, 0.5f);
        rt.pivot = new Vector2(0f, 0.5f);
        card.AttachTrack(this);
        card.Bind(mech);
        card.Clicked += OnCardClicked;
        cardByMech[mech] = card;
    }

    void RemoveCard(MechData mech)
    {
        Selected.Remove(mech);
        if (cardByMech.TryGetValue(mech, out var card))
        {
            card.Clicked -= OnCardClicked;
            Destroy(card.gameObject);
        }
        cardByMech.Remove(mech);
    }

    void RelayoutAll()
    {
        LayoutNotches();
        foreach (var kv in cardByMech)
            Place(kv.Value, runState.StartOf(kv.Key), kv.Key.Span);
    }

    void Place(MechMiniCard card, int start, int span)
    {
        if (start < 0) return;
        var rt = (RectTransform)card.transform;
        rt.sizeDelta = new Vector2(span * NotchWidth - cardInset, track.rect.height - cardInset);
        rt.anchoredPosition = new Vector2(start * NotchWidth + cardInset * 0.5f, 0f);
    }

    // --- Drag API called by MechMiniCard ---

    // Local x measured from the track's LEFT edge (works regardless of track pivot).
    public float PointerToTrackX(Vector2 screenPos, Camera cam)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(track, screenPos, cam, out var lp);
        return lp.x + track.pivot.x * track.rect.width;
    }

    public void FollowDrag(MechMiniCard card, float leftX)
    {
        var rt = (RectTransform)card.transform;
        rt.anchoredPosition = new Vector2(leftX - card.DragGrabOffsetX, rt.anchoredPosition.y);
    }

    public bool TryDropCard(MechMiniCard card, float leftX)
    {
        MechData mech = card.Mech;
        int span = mech.Span;
        int cur = runState.StartOf(mech);
        float desiredLeft = leftX - card.DragGrabOffsetX;
        int target = Mathf.Clamp(Mathf.RoundToInt(desiredLeft / NotchWidth), 0, MechData.TrackNotches - span);

        bool moved = (target != cur) && runState.TryMoveMech(mech, target);
        // Whether moved (Sync re-places it) or reverted, snap to the authoritative position.
        Place(card, moved ? target : cur, span);
        return moved;
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

    // --- Notch dividers (visual guide) ---

    void EnsureNotches()
    {
        if (track == null || notches.Count > 0) return;
        for (int i = 0; i <= MechData.TrackNotches; i++)
        {
            var go = new GameObject("Notch" + i, typeof(RectTransform), typeof(Image));
            var rt = (RectTransform)go.transform;
            rt.SetParent(track, false);
            rt.anchorMin = rt.anchorMax = new Vector2(0f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            var img = go.GetComponent<Image>();
            img.color = notchColor;
            img.raycastTarget = false;
            notches.Add(rt);
        }
    }

    void LayoutNotches()
    {
        float nw = NotchWidth, h = track.rect.height;
        for (int i = 0; i < notches.Count; i++)
        {
            var rt = notches[i];
            rt.sizeDelta = new Vector2(2f, h);
            rt.anchoredPosition = new Vector2(i * nw, 0f);
        }
    }

    void OnRectTransformDimensionsChange()
    {
        if (track == null || runState == null) return;
        RelayoutAll();
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
