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
        float value = Random.Range(0.8f, 1.2f);
        MechData data = new MechData("mech", agility, strength, reliability, value);
        return data;
    }
}
