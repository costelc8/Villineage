using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ResourceDisplay : MonoBehaviour
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
        // Directly access the public 'selected' list from the Selection script
        List<ISelectable> selectedResources = selectionScript.selected;
        // Now you can use 'selectedResources' as needed
        // if (selectedResources.Count > 1)
        // {
            int totalQuantity = 0;
            foreach (ISelectable selectable in selectedResources)
            {
                if (selectable is Resource resource)
                {
                    totalQuantity += resource.quantity;
                }
            }
            resourceText.text = "Resources: " + totalQuantity;
        //}
        // else
        // {
        //     resourceText.text = "Resources: " + resource.quantity;
        // }
    }
}
