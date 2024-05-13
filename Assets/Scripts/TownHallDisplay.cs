using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TownHallDisplay : MonoBehaviour
{
    public TownCenter townCenter;
    public TextMeshProUGUI resourceText;

    void Update()
    {
        // Ensure a reference to the townCenter is set
        if (townCenter == null)
        {
            Debug.LogError("TownCenter reference not set in ResourceDisplay script!");
            return;
        }

        // Ensure a reference to the Text component is set
        if (resourceText == null)
        {
            Debug.LogError("Text component reference not set in ResourceDisplay script!");
            return;
        }

        // Update the resource display
        UpdateResourceDisplay();
    }

    void UpdateResourceDisplay()
    {
        // Get the resources from the TownCenter
        int wood = townCenter.resources[(int)ResourceType.Wood];
        int food = townCenter.resources[(int)ResourceType.Food];

        // Update the Text component with the resources
        resourceText.text = "Wood: " + wood + "\n" + "Food: " + food;
    }
}
