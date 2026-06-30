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

    // Mechs currently away on a dispatch - can't be re-selected until they return.
    readonly HashSet<MechData> busy = new HashSet<MechData>();
    public bool IsBusy(MechData mech) => mech != null && busy.Contains(mech);

    public event Action<int> CashChanged;
    public event Action<int> DayChanged;
    public event Action OwnedMechsChanged;
    public event Action BusyChanged;

    public void SetBusy(MechData mech, bool away)
    {
        if (mech == null) return;
        bool changed = away ? busy.Add(mech) : busy.Remove(mech);
        if (changed) BusyChanged?.Invoke();
    }

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

    // True if `mech` still fits on the bar (its span plus the rest stays within capacity).
    public bool CanAddMech(MechData mech) => inventory.CanAdd(mech);

    public bool TryAddMech(MechData mech)
    {
        if (!inventory.TryAdd(mech)) return false;
        OwnedMechsChanged?.Invoke();
        return true;
    }

    public bool RemoveMech(MechData mech)
    {
        if (!inventory.Remove(mech)) return false;
        OwnedMechsChanged?.Invoke();
        return true;
    }

    // The mech's centre in notch units within the centred, gapless row (-1 if absent).
    public float CenterOf(MechData mech) => inventory.CenterOf(mech);

    // Live drag-reorder: move the mech to the order slot nearest the cursor (`desiredCenter`,
    // notch units). No event - the bar's animator polls CenterOf each frame.
    public void ReorderMech(MechData mech, float desiredCenter) => inventory.Reorder(mech, desiredCenter);

    public void AdvanceDay()
    {
        Day++;
        DayChanged?.Invoke(Day);
    }
}
