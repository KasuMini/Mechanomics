using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MechMiniCard : MonoBehaviour,
    IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler,
    IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public TextMeshProUGUI model;
    public Image mechImage;

    // Stat colours: AGI / STR / SYS / REL.
    const string ColAgi = "#5AD1FF", ColStr = "#FF6B5A", ColSys = "#B58CFF", ColRel = "#FFD24A";

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
    }

    public void AttachTrack(OwnedMechList list) => owner = list;

    // Stats as coloured "agi/str/sys/rel" — shared by the card label and the hover tooltip.
    public static string ColoredStats(MechData m) =>
        $"<color={ColAgi}>{m.agilityStat}</color>/<color={ColStr}>{m.strengthStat}</color>/" +
        $"<color={ColSys}>{m.systemsStat}</color>/<color={ColRel}>{m.reliabilityStat}</color>";

    // Sprite size/placement live on the prefab's MechSprite RectTransform — here we just fill it.
    public void Bind(MechData mech, Sprite sprite)
    {
        Mech = mech;
        model.text = $"{mech.mechName} {ColoredStats(mech)}";
        mechImage.sprite = sprite;
        mechImage.enabled = sprite != null;
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
        owner.HideTooltip();
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

    public void OnPointerEnter(PointerEventData e)
    {
        if (owner != null) owner.ShowTooltip(Mech, (RectTransform)transform);
    }

    public void OnPointerExit(PointerEventData e)
    {
        if (owner != null) owner.HideTooltip();
    }
}
