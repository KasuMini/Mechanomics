using System.Collections.Generic;

// The bottom bar's 24 notches ARE the inventory: a mech occupying [start, start+span)
// is stored as its own reference repeated across those cells. Contents, position, and
// capacity are all read off this one array — there is no separate position map.
public class MechInventory
{
    public const int Capacity = 24;
    readonly MechData[] slots = new MechData[Capacity];

    // First cell holding the mech, or -1 if absent.
    public int StartOf(MechData m)
    {
        if (m == null) return -1;
        for (int i = 0; i < Capacity; i++)
            if (slots[i] == m) return i;
        return -1;
    }

    public bool Contains(MechData m) => StartOf(m) >= 0;

    public int UsedSpan
    {
        get
        {
            int n = 0;
            for (int i = 0; i < Capacity; i++)
                if (slots[i] != null) n++;
            return n;
        }
    }

    // Distinct mechs, left to right.
    public IEnumerable<MechData> Mechs
    {
        get
        {
            for (int i = 0; i < Capacity; i++)
                if (slots[i] != null && (i == 0 || slots[i - 1] != slots[i]))
                    yield return slots[i];
        }
    }

    // Lowest start with `span` free cells, or -1.
    public int FirstFit(int span)
    {
        for (int s = 0; s + span <= Capacity; s++)
            if (IsFree(s, span)) return s;
        return -1;
    }

    // Are cells [start, start+span) free, treating `ignore`'s own cells as free?
    public bool IsFree(int start, int span, MechData ignore = null)
    {
        if (start < 0 || span <= 0 || start + span > Capacity) return false;
        for (int i = start; i < start + span; i++)
            if (slots[i] != null && slots[i] != ignore) return false;
        return true;
    }

    public bool TryAdd(MechData m)
    {
        if (m == null || Contains(m)) return false;
        int start = FirstFit(m.Span);
        if (start < 0) return false;
        Fill(start, m.Span, m);
        return true;
    }

    public bool TryMove(MechData m, int newStart)
    {
        if (m == null) return false;
        int cur = StartOf(m);
        if (cur < 0) return false;
        if (!IsFree(newStart, m.Span, m)) return false;
        Fill(cur, m.Span, null);
        Fill(newStart, m.Span, m);
        return true;
    }

    public bool Remove(MechData m)
    {
        int start = StartOf(m);
        if (start < 0) return false;
        Fill(start, m.Span, null);
        return true;
    }

    public void Clear()
    {
        for (int i = 0; i < Capacity; i++) slots[i] = null;
    }

    void Fill(int start, int span, MechData value)
    {
        for (int i = start; i < start + span; i++) slots[i] = value;
    }
}
