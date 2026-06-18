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
        availableMechs = new MechData[9];
        availableMechs[0] = generator.GenerateNewData();
        availableMechs[1] = generator.GenerateNewData();
        availableMechs[2] = generator.GenerateNewData();
    }


    public void PurchaseMech(MechData purchasedMech)
    {
        totalMechs++;

    }
}
