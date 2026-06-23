using TMPro;
using UnityEngine;

// Shared top-bar readout: current day (and cash). Reads the run from GameManager.Instance.
public class TopBarDisplay : MonoBehaviour
{
    public TextMeshProUGUI dayText;
    public TextMeshProUGUI cashText;

    RunState runState;

    void OnEnable()
    {
        runState = GameManager.Instance != null ? GameManager.Instance.runState : null;
        if (runState != null)
        {
            runState.DayChanged += OnDayChanged;
            runState.CashChanged += OnCashChanged;
        }
        RefreshDay();
        RefreshCash();
    }

    void OnDisable()
    {
        if (runState != null)
        {
            runState.DayChanged -= OnDayChanged;
            runState.CashChanged -= OnCashChanged;
        }
    }

    void OnDayChanged(int day) => RefreshDay();
    void OnCashChanged(int cash) => RefreshCash();

    void RefreshDay()
    {
        if (dayText != null && runState != null) dayText.text = $"Day {runState.Day}/{runState.TotalDays}";
    }

    void RefreshCash()
    {
        if (cashText != null && runState != null) cashText.text = $"${runState.Cash}";
    }
}
