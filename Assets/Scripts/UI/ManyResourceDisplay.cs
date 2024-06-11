using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ManyResourceDisplay : MonoBehaviour
{
    public TextMeshProUGUI resourceText;

    private void LateUpdate()
    {
        int totalQuantity = 0;
        int totalResources = 0;
        Vector3 position = Vector3.zero;
        foreach (ISelectable selectable in Selection.Selector.selected)
        {
            if (selectable is Resource resource)
            {
                totalQuantity += resource.quantity;
                totalResources++;
                position += resource.transform.position;
            }
        }
        if (totalResources > 1)
        {
            resourceText.text = "Total Resources: " + totalQuantity;
            position /= totalResources;
            GetComponent<RectTransform>().anchoredPosition = Selection.Selector.ScreenToCanvasSpace(Camera.main.WorldToScreenPoint(position));
        }
        else resourceText.text = "";
    }
}
