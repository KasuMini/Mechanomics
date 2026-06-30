using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// A mech in the bar: just the standing sprite (+ optional label) and a LerpToTarget that eases
// it toward its slot. Dragging is forwarded to the OwnedMechList, which owns the reflow.
[RequireComponent(typeof(LerpToTarget))]
public class MechMiniCard : MonoBehaviour,
    IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler,
    IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public TextMeshProUGUI model;
    public Image mechImage;
    public GameObject selBack;    // white rect, slightly taller, behind the sprite
    public GameObject selFront;   // white rect, slightly shorter, in front - together they read as a cuboid

    // Stat colours: AGI / STR / SYS / REL.
    const string ColAgi = "#5AD1FF", ColStr = "#FF6B5A", ColSys = "#B58CFF", ColRel = "#FFD24A";

    const float PixelScale = 2f;    // art px -> canvas units (640 ref / 320 design)
    const float SelPad = 2f;        // selection box side margin (canvas px)
    const float SelBoxHeight = 32f * PixelScale; // fixed box height (32 art px), consistent across mechs

    public MechData Mech { get; private set; }
    public LerpToTarget Lerp { get; private set; }
    public event Action<MechMiniCard> Clicked;

    OwnedMechList owner;
    bool didDrag;

    void Awake() => Lerp = GetComponent<LerpToTarget>();

    public void AttachTrack(OwnedMechList list) => owner = list;

    // Stats as coloured "agi/str/sys/rel" - shared by the card label and the hover tooltip.
    public static string ColoredStats(MechData m) =>
        $"<color={ColAgi}>{m.agilityStat}</color>/<color={ColStr}>{m.strengthStat}</color>/" +
        $"<color={ColSys}>{m.systemsStat}</color>/<color={ColRel}>{m.reliabilityStat}</color>";

    public void Bind(MechData mech, Sprite sprite)
    {
        Mech = mech;
        if (model != null) model.text = $"{mech.mechName} {ColoredStats(mech)}";
        mechImage.sprite = sprite;
        mechImage.enabled = sprite != null;
        mechImage.material = mech.palette != null ? mech.palette.Material : null;

        // Render at the sprite's true size; hug its width, fixed height for a consistent box.
        Vector2 size = sprite != null ? sprite.rect.size * PixelScale : new Vector2(64f, 64f);
        mechImage.rectTransform.sizeDelta = size;
        Vector2 box = new Vector2(size.x + SelPad * 2f, SelBoxHeight);
        if (selBack != null)  ((RectTransform)selBack.transform).sizeDelta = box;
        if (selFront != null) ((RectTransform)selFront.transform).sizeDelta = box;
    }

    public void SetSelected(bool on)
    {
        if (selBack != null) selBack.SetActive(on);
        if (selFront != null) selFront.SetActive(on);
    }

    // Dim the card while the mech is away on a dispatch (can't be picked).
    public void SetAvailable(bool available)
    {
        if (mechImage != null)
            mechImage.color = available ? Color.white : new Color(0.42f, 0.44f, 0.5f, 0.55f);
    }

    public void OnPointerDown(PointerEventData e) => didDrag = false;

    public void OnBeginDrag(PointerEventData e)
    {
        if (owner == null) return;
        didDrag = true;
        owner.BeginDrag(this, owner.PointerToTrackLocal(e.position, e.pressEventCamera));
    }

    public void OnDrag(PointerEventData e)
    {
        if (owner == null) return;
        owner.Drag(this, owner.PointerToTrackLocal(e.position, e.pressEventCamera));
    }

    public void OnEndDrag(PointerEventData e)
    {
        if (owner != null) owner.EndDrag(this);
    }

    public void OnPointerClick(PointerEventData e)
    {
        if (didDrag) { didDrag = false; return; }   // a drag ended here - not a real click
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
