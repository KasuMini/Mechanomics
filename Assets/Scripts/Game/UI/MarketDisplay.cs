using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MarketDisplay : MonoBehaviour
{
    public MechMarket mechMarket;
    public int cardID;
    public MechData mechData;
    public Image mechImage;
    public MechSpriteLibrary sprites;
    public TextMeshProUGUI model;
    public TextMeshProUGUI pilot;
    public TextMeshProUGUI stats;

    void Start()
    {
        mechData = mechMarket.availableMechs[cardID];
        if (model != null) model.text = mechData.mechName;
        if (pilot != null) pilot.text = mechData.pilotName;
        if (stats != null)
            stats.text = $"{MechMiniCard.ColoredStats(mechData)}\n${mechData.cost}";
        if (mechImage != null && sprites != null)
        {
            mechImage.sprite = sprites.Get(mechData.size, mechData.variant);
            mechImage.material = mechData.palette != null ? mechData.palette.Material : null;
            mechImage.preserveAspect = true;
        }
    }
}
