using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Small event popup anchored over a building on the map. One auto-sizing card: a
// combined text block (title + description + live requirement rows) plus a radial
// countdown ring and a Dispatch button. The card resizes to its content via a
// VerticalLayoutGroup + ContentSizeFitter, so nothing here hard-codes sizes.
public class EventPopup : MonoBehaviour
{
    public RectTransform rect;
    public RectTransform canvasRect;
    public Camera uiCamera;             // camera the Screen-Space-Camera canvas renders through
    public Image countdownRing;         // Image type=Filled, Radial360
    public TextMeshProUGUI bodyText;    // title + description + requirement rows, one block
    public Button dispatchButton;
    public OwnedMechList ownedList;     // for the live requirement coverage
    public Vector2 screenOffset = new Vector2(0f, 44f);

    public EventData Current { get; private set; }

    BuildingPrism building;
    CityClock clock;
    float spawnHour, expireHour;
    Action<EventData, BuildingPrism> onDispatch;
    readonly List<IMechStats> selectedBuffer = new List<IMechStats>();
    readonly StringBuilder sb = new StringBuilder();
    bool bodyDirty;   // rebuild the text block only on open or selection change, not every frame

    void Awake()
    {
        if (dispatchButton != null) dispatchButton.onClick.AddListener(OnDispatchClicked);
        gameObject.SetActive(false);
    }

    public void Open(EventData ev, BuildingPrism b, CityClock c, float spawn, float expire,
                     Action<EventData, BuildingPrism> dispatchCb)
    {
        Current = ev;
        building = b;
        clock = c;
        spawnHour = spawn;
        expireHour = expire;
        onDispatch = dispatchCb;
        bodyDirty = true;
        if (ownedList != null) { ownedList.SelectionChanged -= MarkBodyDirty; ownedList.SelectionChanged += MarkBodyDirty; }
        gameObject.SetActive(true);
        UpdateView();
    }

    public void Close()
    {
        if (ownedList != null) ownedList.SelectionChanged -= MarkBodyDirty;
        Current = null;
        building = null;
        gameObject.SetActive(false);
    }

    void Update()
    {
        if (Current != null) UpdateView();
    }

    void MarkBodyDirty() => bodyDirty = true;

    void UpdateView()
    {
        if (bodyDirty && bodyText != null && Current != null)
        {
            selectedBuffer.Clear();
            if (ownedList != null) foreach (MechData m in ownedList.Selected) selectedBuffer.Add(m);

            sb.Clear();
            if (!string.IsNullOrEmpty(Current.title)) sb.Append("<b>").Append(Current.title).Append("</b>\n");
            if (!string.IsNullOrEmpty(Current.description)) sb.Append(Current.description).Append("\n\n");
            sb.Append(Current.Preview(selectedBuffer));   // "Strength 4/7\n...\nSuccess: 60%"
            bodyText.text = sb.ToString();
            bodyDirty = false;
        }

        if (countdownRing != null)
            countdownRing.fillAmount = CityClock.Fraction(clock != null ? clock.Now : spawnHour, spawnHour, expireHour);

        if (building != null) PositionAt(building);
    }

    void PositionAt(BuildingPrism b)
    {
        if (rect == null || canvasRect == null) return;
        if (!MapUi.WorldToCanvasLocal(uiCamera, canvasRect, b.CentroidWorld, out Vector2 local)) return;

        Vector2 pos = local + screenOffset;
        Vector2 half = canvasRect.rect.size * 0.5f;
        Vector2 ext = rect.rect.size * 0.5f;   // current fitted size
        pos.x = Mathf.Clamp(pos.x, -half.x + ext.x, half.x - ext.x);
        pos.y = Mathf.Clamp(pos.y, -half.y + ext.y, half.y - ext.y - 16f);
        rect.anchoredPosition = pos;
    }

    void OnDispatchClicked()
    {
        if (Current != null) onDispatch?.Invoke(Current, building);
    }
}
