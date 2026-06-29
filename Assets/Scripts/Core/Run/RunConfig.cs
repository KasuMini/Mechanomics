using System.Collections.Generic;
using UnityEngine;

// Authoring settings for a run (the game's knobs). A new RunState is built from one of these.
[CreateAssetMenu(fileName = "RunConfig", menuName = "Mechanomics/Run Config", order = 1)]
public class RunConfig : ScriptableObject
{
    public int startingCash = 10000;
    public int startingDay = 1;
    public int totalDays = 10;

    [Header("Debug")]
    [Tooltip("Mechs the run starts with (e.g. a test save of every model for palette iteration).")]
    public List<MechData> startingMechs = new List<MechData>();
}
