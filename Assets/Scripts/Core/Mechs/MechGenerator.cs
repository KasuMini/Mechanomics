using UnityEngine;

public class MechGenerator : MonoBehaviour
{
    
    void Start()
    {
        
    }

    // Generates new MechData for each mech that will be generated in the market with random ranges and names from a list
    public MechData GenerateNewData()
    {
        int agility = Random.Range(0, 10);
        int strength = Random.Range(0, 10);
        int reliability = Random.Range(0, 10);
        int systems = Random.Range(0, 10);
        int size = Random.Range(1, 4);
        MechData data = MechData.Create("mech", "ace", agility, strength, systems, reliability, size);
        return data;
    }
}
