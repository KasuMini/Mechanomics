using System;
using System.Collections.Generic;

// The player's live state for the current run: cash, day, and the owned-mech inventory.
// Built from a RunConfig. The active run lives in the static Active for the session
// (survives scene loads, fresh each play) - see GameManager.
public class RunState
{
    // The current run, or null before one has started. GameManager owns its lifecycle.
    public static RunState Active { get; set; }

    readonly RunConfig config;

    public int Cash { get; private set; }
    public int Day { get; private set; }
    public int TotalDays => config != null ? config.totalDays : 0;
    public bool IsRunOver => config != null && Day > config.totalDays;
    public Queue<string> eventOutcomes = new Queue<string>();

    // The 20-notch bar IS the inventory: contents and positions live in one array.
    readonly MechInventory inventory = new MechInventory();
    public IEnumerable<MechData> OwnedMechs => inventory.Mechs;

    public event Action<int> CashChanged;
    public event Action<int> DayChanged;
    public event Action OwnedMechsChanged;

    public RunState(RunConfig config)
    {
        this.config = config;
        Cash = config != null ? config.startingCash : 0;
        Day = config != null ? config.startingDay : 1;
        if (config != null && config.startingMechs != null)
            foreach (var mech in config.startingMechs)
                if (mech != null) inventory.TryAdd(mech);
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

    // True if `mech` would fit somewhere on the bar (a contiguous free span exists).
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
