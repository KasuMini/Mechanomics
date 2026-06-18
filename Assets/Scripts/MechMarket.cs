using NUnit.Framework;
using UnityEngine;

public class MechMarket : MonoBehaviour
{
    public CashManager cashManager;
    public MechGenerator generator;
    public MechData[] availableMechs;

    public int totalMechs;

    void Start()
    {
        totalMechs = 0;
        availableMechs = new MechData[6];
        availableMechs[0] = generator.GenerateNewData();
        availableMechs[1] = generator.GenerateNewData();
        availableMechs[2] = generator.GenerateNewData();
        availableMechs[3] = generator.GenerateNewData();
        availableMechs[4] = generator.GenerateNewData();
        availableMechs[5] = generator.GenerateNewData();
    }


    public void PurchaseMech(MechData purchasedMech)
    {
        totalMechs++;

    }
}
