using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ResourceDisplay : MonoBehaviour
{
    public Resource resource;
    public TextMeshProUGUI resourceText;

    // Update is called once per frame
    void Update()
    {
        resourceText.text = "Resources: " + resource.quantity;
    }

    //void Update()
    //{
    //    // Directly access the public 'selected' list from the Selection script
    //    // List<ISelectable> selectedResources = Selection.Selector.selected;
    //    // Now you can use 'selectedResources' as needed
    //    if (Selection.Selector.selected.Count > 1)
    //    {
    //        int totalQuantity = 0;
    //        foreach (ISelectable selectable in Selection.Selector.selected)
    //        {
    //            if (selectable is Resource resource)
    //            {
    //                totalQuantity += resource.quantity;
    //            }
    //        }
    //        if (totalQuantity > 0) resourceText.text = "Resources: " + totalQuantity;
    //        else
    //        {
    //            resourceText.text = "Resources: " + resource.quantity;
    //        }
    //    }
    //}

    //public void UpdateResourceDisplay()
    //{
    //    int totalQuantity = 0;
    //    foreach (ISelectable selectable in Selection.Selector.selected)
    //    {
    //        if (selectable is Resource resource)
    //        {
    //            totalQuantity += resource.quantity;
    //        }
    //    }
    //    if (totalQuantity > 0) resourceText.text = "Resources: " + totalQuantity;
    //    else resourceText.text = "";
    //}
}
