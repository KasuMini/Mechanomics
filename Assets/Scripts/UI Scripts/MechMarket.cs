using NUnit.Framework;
using UnityEngine;

public class MechMarket : MonoBehaviour
{
    public MechGenerator generator;
    public MechData[] availableMechs;

    private void Awake()
    {
        availableMechs = new MechData[6];
        availableMechs[0] = generator.GenerateNewData();
        availableMechs[1] = generator.GenerateNewData();
        availableMechs[2] = generator.GenerateNewData();
        availableMechs[3] = generator.GenerateNewData();
        availableMechs[4] = generator.GenerateNewData();
        availableMechs[5] = generator.GenerateNewData();
    }

    void Start()
    {

    }

}
