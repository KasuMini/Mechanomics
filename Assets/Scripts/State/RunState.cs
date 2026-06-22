using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RunState", menuName = "Mechanomics/Run State", order = 1)]
public class RunState : ScriptableObject
{
    [SerializeField] private int startingCash = 10000;

    public int Cash { get; private set; }
    [field:SerializeField] public List<MechData> OwnedMechs { get; private set; } = new List<MechData>();

    public event Action<int> CashChanged;
    public event Action OwnedMechsChanged;

    // SO values survive Editor play sessions; call on a fresh run.
    public void ResetRun()
    {
        Cash = startingCash;
        OwnedMechs.Clear();
        CashChanged?.Invoke(Cash);
        OwnedMechsChanged?.Invoke();
    }
    
    private void OnEnable()
    {
        OwnedMechs ??= new List<MechData>();
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

    public void AddMech(MechData mech)
    {
        if (mech == null) return;
        OwnedMechs.Add(mech);
        OwnedMechsChanged?.Invoke();
    }
}
