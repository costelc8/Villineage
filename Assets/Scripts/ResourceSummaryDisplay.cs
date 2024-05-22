
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ResourceSummaryDisplay : MonoBehaviour
{
    public Resource resource;
    public TextMeshProUGUI resourceText;
    private Selection selectionScript;

    void Start()
    {
        // Find the Selection script on the appropriate GameObject
        selectionScript = FindObjectOfType<Selection>();
    }

    void Update()
    {
        Debug.Log("MyMethod is called!");
        // Directly access the public 'selected' list from the Selection script
        List<ISelectable> selectedResources = selectionScript.selected;
        Debug.Log("Number of selected resources: " + selectedResources.Count);
        // Now you can use 'selectedResources' as needed
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
            resourceText.text = "Selected Resources: " + totalQuantity;
            gameObject.SetActive(true); // Show the UI
        }
        else
        {
            gameObject.SetActive(false); // Hide the UI if less than 2 resources are selected
        }
    }
}



