using NUnit.Framework;
using UnityEngine;

public class MechMarket : MonoBehaviour
{
    // This script generates mechs for the preparation market using the MechGenerator GenerateNewData Function and populates the array.

    public MechGenerator generator;
    public MechData[] availableMechs;

    private void Awake()
    {
        availableMechs = new MechData[6];
        for (int i = 0; i < availableMechs.Length; i++)
        {
            availableMechs[i] = generator.GenerateNewData();
        }
    }
}
