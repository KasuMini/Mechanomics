using UnityEngine;

public class MechStorage : MonoBehaviour
{
    public MechMarket market;
    public MechData[] storedMechs;

    public int totalMechs;
    void Start()
    {
        storedMechs = new MechData[totalMechs];
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
