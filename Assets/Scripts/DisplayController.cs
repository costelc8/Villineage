using UnityEngine;
using TMPro;

public class DisplayController : MonoBehaviour
{
    // Reference to the Villager script
    public Villager villager; // This needs an object with the villager script attached to it
    // References to the TextMeshPro components
    public TextMeshProUGUI jobTextMesh;
    public TextMeshProUGUI inventoryTextMesh;
    public TextMeshProUGUI hungerTextMesh;

    // Start is called before the first frame update
    void Update()
    {
        // Check if the Villager script is assigned
        // if (villager == null)
        // {
        //     Debug.LogWarning("Villager reference is not assigned.");
        //     return;
        // }

        // Display job
        DisplayJob();

        // Display inventory
        DisplayInventory();

        // Display hunger
        DisplayHunger();
    }

    void DisplayJob()
    {
        // Check if the TextMeshPro component is assigned
        if (jobTextMesh == null)
        {
            Debug.LogWarning("Job TextMeshPro reference is not assigned.");
            return;
        }

        // Check the job of the villager and display it
        switch (villager.Job())
        {
            case VillagerJob.Nitwit:
                jobTextMesh.text = "Job: Nitwit";
                break;
            case VillagerJob.Gatherer:
                jobTextMesh.text = "Job: Gatherer";
                break;
            case VillagerJob.Lumberjack:
                jobTextMesh.text = "Job: Lumberjack";
                break;
            case VillagerJob.Builder:
                jobTextMesh.text = "Job: Builder";
                break;
            default:
                jobTextMesh.text = "Job: Unknown";
                break;
        }
    }

    void DisplayInventory()
    {
        // Check if the TextMeshPro component is assigned
        if (inventoryTextMesh == null)
        {
            Debug.LogWarning("Inventory TextMeshPro reference is not assigned.");
            return;
        }

        // Display the resources of the villager
        string resourcesText = "Resources: ";
        for (int i = 0; i < (int)ResourceType.MAX_VALUE; i++)
        {
            if (villager.inventory[i] > 0) resourcesText += (ResourceType)i + ": " + villager.inventory[i];
        }
        //foreach (var resource in villager.resources)
        //{
        //    resourcesText += resource.Key.ToString() + ": " + resource.Value + "\n";
        //}
        inventoryTextMesh.text = resourcesText;
    }

    void DisplayHunger()
    {
        // Check if the TextMeshPro component is assigned
        if (hungerTextMesh == null)
        {
            Debug.LogWarning("Hunger TextMeshPro reference is not assigned.");
            return;
        }

        // Display the hunger of the villager
        hungerTextMesh.text = "Hunger: " + ((int)villager.hunger).ToString();
    }
}
