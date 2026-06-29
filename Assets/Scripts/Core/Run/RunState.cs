using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RunState", menuName = "Mechanomics/Run State", order = 1)]
public class RunState : ScriptableObject
{
    [SerializeField] private int startingCash = 10000;
    [SerializeField] private int startingDay = 1;
    [SerializeField] private int totalDays = 10;

    public int Cash { get; private set; }
    public int Day { get; private set; }
    public int TotalDays => totalDays;
    public bool IsRunOver => Day > totalDays;

    // The 24-notch bar IS the inventory: contents and positions live in one array.
    readonly MechInventory inventory = new MechInventory();
    public IEnumerable<MechData> OwnedMechs => inventory.Mechs;

    public event Action<int> CashChanged;
    public event Action<int> DayChanged;
    public event Action OwnedMechsChanged;

    // SO values survive Editor play sessions; call on a fresh run.
    public void ResetRun()
    {
        Cash = startingCash;
        Day = startingDay;
        inventory.Clear();
        CashChanged?.Invoke(Cash);
        OwnedMechsChanged?.Invoke();
    }

    public void AddCash(int delta)
    {
        if (delta == 0) return;
        Cash += delta;
        CashChanged?.Invoke(Cash);
    }

    public bool TrySpend(int amount)
    {
        if (amount < 0 || Cash < amount) return false;
        Cash -= amount;
        CashChanged?.Invoke(Cash);
        return true;
    }

    // True if `mech` would fit somewhere on the 24-notch bar (a contiguous free span exists).
    public bool CanAddMech(MechData mech) => mech != null && inventory.FirstFit(mech.Span) >= 0;

    public bool TryAddMech(MechData mech)
    {
        if (!inventory.TryAdd(mech)) return false;
        OwnedMechsChanged?.Invoke();
        return true;
    }

    // Drag-relocate: validated against the inventory (ignoring the mech's own cells).
    public bool TryMoveMech(MechData mech, int newStart)
    {
        if (!inventory.TryMove(mech, newStart)) return false;
        OwnedMechsChanged?.Invoke();
        return true;
    }

    public bool RemoveMech(MechData mech)
    {
        if (!inventory.Remove(mech)) return false;
        OwnedMechsChanged?.Invoke();
        return true;
    }

    public int StartOf(MechData mech) => inventory.StartOf(mech);

    public void AdvanceDay()
    {
        Day++;
        DayChanged?.Invoke(Day);
    }
}
