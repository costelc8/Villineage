using UnityEngine;
using TMPro;

public class VillagerDisplay : MonoBehaviour
{
    // Reference to the Villager script
    public Villager villager; // This needs an object with the villager script attached to it
    // References to the TextMeshPro components
    public TextMeshProUGUI jobTextMesh;
    public TextMeshProUGUI inventoryTextMesh;
    public TextMeshProUGUI hungerTextMesh;
    public TextMeshProUGUI causeOfDeathTextMesh;

    // Start is called before the first frame update
    void Update()
    {
        // Display job
        DisplayJob();

        // Display inventory
        DisplayInventory();

        // Display hunger
        DisplayHunger();

        // Display cause of death
        DisplayCauseOfDeath();
    }

    void DisplayJob()
    {
        // Display the job of the villager
        if (villager == null) jobTextMesh.text = "";
        else jobTextMesh.text = villager.villagerName + " the " + villager.job;
    }

    void DisplayInventory()
    {
        // Display the resources of the villager
        if (villager == null) inventoryTextMesh.text = "";
        else
        {
            string resourcesText = "Resources: ";
            for (int i = 0; i < (int)ResourceType.MAX_VALUE; i++)
            {
                if (villager.inventory[i] > 0) resourcesText += villager.inventory[i] + " " + (ResourceType)i;
            }
            inventoryTextMesh.text = resourcesText;
        }
    }

    void DisplayHunger()
    {
        // Display the hunger of the villager
        if (villager == null) hungerTextMesh.text = "";
        else hungerTextMesh.text = "Vitality: " + ((int)villager.vitality).ToString();
    }

    void DisplayCauseOfDeath()
    {
        // Display the villager's cause of death
        if (villager == null || villager.causeOfDeath == "") causeOfDeathTextMesh.text = "";
        else causeOfDeathTextMesh.text = "Died From " + villager.causeOfDeath;
    }
}
