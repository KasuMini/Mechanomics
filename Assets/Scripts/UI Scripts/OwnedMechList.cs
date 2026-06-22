using UnityEngine;

public class OwnedMechList : MonoBehaviour
{
    public MechMiniCard cardPrefab;
    public Transform content;

    RunState runState;

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

        if (runState == null || cardPrefab == null) return;

        foreach (var mech in runState.OwnedMechs)
            Instantiate(cardPrefab, content).Bind(mech);
    }
}
