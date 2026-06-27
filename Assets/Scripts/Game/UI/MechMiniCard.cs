using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MechMiniCard : MonoBehaviour,
    IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    public TextMeshProUGUI model;
    public TextMeshProUGUI stats;

    public MechData Mech { get; private set; }
    public event Action<MechMiniCard> Clicked;
    public float DragGrabOffsetX { get; private set; }

    OwnedMechList owner;
    Image bg;
    Color normalColor;
    bool didDrag;

    void Awake()
    {
        bg = GetComponent<Image>();
        if (bg != null) normalColor = bg.color;
        // Click/drag are handled here; the old Button would fight us for pointer events.
        var btn = GetComponent<Button>();
        if (btn != null) btn.enabled = false;
    }

    public void AttachTrack(OwnedMechList list) => owner = list;

    public void Bind(MechData mech)
    {
        Mech = mech;
        model.text = mech.mechName;
        stats.text = $"AGI {mech.agilityStat}  STR {mech.strengthStat}  " +
                     $"SYS {mech.systemsStat}  REL {mech.reliabilityStat}  Sz{mech.size}";
    }

    public void SetSelected(bool on)
    {
        if (bg != null) bg.color = on ? new Color(0.18f, 0.42f, 0.30f, 1f) : normalColor;
    }

    public void OnPointerDown(PointerEventData e) => didDrag = false;

    public void OnBeginDrag(PointerEventData e)
    {
        if (owner == null) return;
        didDrag = true;
        transform.SetAsLastSibling();
        float leftX = owner.PointerToTrackX(e.position, e.pressEventCamera);
        DragGrabOffsetX = leftX - ((RectTransform)transform).anchoredPosition.x;
    }

    public void OnDrag(PointerEventData e)
    {
        if (owner == null) return;
        owner.FollowDrag(this, owner.PointerToTrackX(e.position, e.pressEventCamera));
    }

    public void OnEndDrag(PointerEventData e)
    {
        if (owner == null) return;
        owner.TryDropCard(this, owner.PointerToTrackX(e.position, e.pressEventCamera));
    }

    public void OnPointerClick(PointerEventData e)
    {
        if (didDrag) { didDrag = false; return; }   // a drag ended here — not a real click
        Clicked?.Invoke(this);
    }
}
