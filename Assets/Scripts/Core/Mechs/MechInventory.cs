using System.Collections.Generic;

// The owned mechs are just an ordered list - no slot grid, no gaps. Each mech's on-screen
// position is derived by packing the list shoulder-to-shoulder and centring it in the bar
// (CenterOf); dragging is a plain reorder (Reorder). Widths are in notch units (MechData.Span);
// the bar is Capacity notches wide.
public class MechInventory
{
    public const int Capacity = 20;
    readonly List<MechData> order = new List<MechData>();

    public IReadOnlyList<MechData> Mechs => order;

    public bool Contains(MechData m) => order.Contains(m);

    public int UsedSpan
    {
        get
        {
            int n = 0;
            foreach (var m in order) n += m.Span;
            return n;
        }
    }

    public bool CanAdd(MechData m) => m != null && !Contains(m) && UsedSpan + m.Span <= Capacity;

    public bool TryAdd(MechData m)
    {
        if (!CanAdd(m)) return false;
        order.Add(m);
        return true;
    }

    public bool Remove(MechData m) => order.Remove(m);

    public void Clear() => order.Clear();

    // The mech's centre in notch units within the centred, gapless row, or -1 if absent.
    public float CenterOf(MechData m)
    {
        int idx = order.IndexOf(m);
        if (idx < 0) return -1f;
        float center = (Capacity - UsedSpan) * 0.5f;   // left pad that centres the whole row
        for (int i = 0; i < idx; i++) center += order[i].Span;
        return center + m.Span * 0.5f;
    }

    // Move `dragged` to the order position whose resulting centre sits nearest `desiredCenter`
    // (notch units). The rest repack and re-centre around it - no gaps, no chasing.
    public void Reorder(MechData dragged, float desiredCenter)
    {
        if (dragged == null || !order.Remove(dragged)) return;
        float left = (Capacity - (UsedSpan + dragged.Span)) * 0.5f;
        int best = 0;
        float bestDist = float.MaxValue;
        float run = 0f;
        for (int k = 0; k <= order.Count; k++)
        {
            float dist = System.Math.Abs(left + run + dragged.Span * 0.5f - desiredCenter);
            if (dist < bestDist) { bestDist = dist; best = k; }
            if (k < order.Count) run += order[k].Span;
        }
        order.Insert(best, dragged);
    }
}
