using UnityEngine;
using TMPro;

public class JobDisplay : MonoBehaviour
{
    // Reference to the Villager script
    // public Villager villager;
    public VillagerJob job;
    // Reference to the TextMeshPro component
    public TextMeshProUGUI textMesh;

    // Start is called before the first frame update
    void Start()
    {
        // Check if the Villager script is assigned
        // if (villager == null)
        // {
        //     Debug.LogWarning("Villager reference is not assigned.");
        //     return;
        // }

        // Check if the TextMeshPro component is assigned
        if (textMesh == null)
        {
            Debug.LogWarning("TextMeshPro reference is not assigned.");
            return;
        }

        // Check the job of the villager and display it
        switch (job)
        {
            case VillagerJob.Nitwit:
                textMesh.text = "Job: Nitwit";
                break;
            case VillagerJob.Gatherer:
                textMesh.text = "Job: Gatherer";
                break;
            case VillagerJob.Lumberjack:
                textMesh.text = "Job: Lumberjack";
                break;
            case VillagerJob.Builder:
                textMesh.text = "Job: Builder";
                break;
            default:
                textMesh.text = "Job: Unknown";
                break;
        }
    }
}