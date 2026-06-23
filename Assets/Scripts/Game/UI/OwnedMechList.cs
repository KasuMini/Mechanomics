using System;
using System.Collections.Generic;
using UnityEngine;

// Renders the owned mechs and acts as a multi-select picker (toggle on click).
public class OwnedMechList : MonoBehaviour
{
    public MechMiniCard cardPrefab;
    public Transform content;

    public HashSet<MechData> Selected { get; } = new HashSet<MechData>();
    public event Action SelectionChanged;

    RunState runState;
    readonly List<MechMiniCard> cards = new List<MechMiniCard>();

    void Start()
    {
        runState = GameManager.Instance != null ? GameManager.Instance.runState : null;
        if (runState != null) runState.OwnedMechsChanged += Refresh;
        Refresh();
    }

    void OnDestroy()
    {
        if (runState != null) runState.OwnedMechsChanged -= Refresh;
    }

    public void Refresh()
    {
        if (content == null) return;

        for (int i = content.childCount - 1; i >= 0; i--)
            Destroy(content.GetChild(i).gameObject);
        cards.Clear();
        Selected.Clear();

        if (runState == null || cardPrefab == null) return;

        foreach (var mech in runState.OwnedMechs)
        {
            var card = Instantiate(cardPrefab, content);
            card.Bind(mech);
            card.Clicked += OnCardClicked;
            cards.Add(card);
        }
        SelectionChanged?.Invoke();
    }

    public void ClearSelection()
    {
        foreach (var c in cards) c.SetSelected(false);
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
}
