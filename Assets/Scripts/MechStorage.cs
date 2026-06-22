using UnityEngine;

public class MechStorage : MonoBehaviour
{
    public MechMarket market;
    public MechData[] storedMechs;

    public int totalMechs;

    void Awake()
    {
        storedMechs = new MechData[totalMechs];
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
