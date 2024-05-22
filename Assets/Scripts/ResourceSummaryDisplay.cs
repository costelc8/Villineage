
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ResourceSummaryDisplay : MonoBehaviour
{
    public TextMeshProUGUI resourceText;

    // Method to be called from the Selection script
    public void DisplaySelectedResources(List<ISelectable> selectedResources)
    {
        // Check if more than one resource is selected
        if (selectedResources.Count > 1)
        {
            int totalQuantity = 0;
            foreach (ISelectable selectable in selectedResources)
            {
                if (selectable is Resource resource)
                {
                    totalQuantity += resource.quantity;
                }
            }

            // Update the UI text to show the total quantity
            resourceText.text = "Selected Resources: " + totalQuantity;
            gameObject.SetActive(true); // Show the UI
        }
        else
        {
            gameObject.SetActive(false); // Hide the UI if less than 2 resources are selected
        }
    }
}



