using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEditor.SceneManagement;
using UnityEngine;

public class CartDisplay : MonoBehaviour
{
    public Cart cart;
    public TextMeshProUGUI cartText;

    void Update()
    {
        // Ensure a reference to the cart is set
        if (cart == null)
        {
            Debug.LogError("TownCenter reference not set in ResourceDisplay script!");
            return;
        }

        // Ensure a reference to the Text component is set
        if (cartText == null)
        {
            Debug.LogError("Text component reference not set in ResourceDisplay script!");
            return;
        }

        // Update the resource display
        UpdateResourceDisplay();
    }

    void UpdateResourceDisplay()
    {
        // Get the resources from the storage
        StringBuilder sb = new StringBuilder("Inventory:\n");
        for (int i = 0; i < (int)ResourceType.MAX_VALUE; i++)
        {
            if (cart.inventory[i] > 0)
            {
                sb.Append((ResourceType)i);
                sb.Append(": ");
                sb.Append(cart.inventory[i]);
                sb.Append('\n');
            }
        }

        // Update the Text component with the resources
        cartText.text = sb.ToString();
    }
}
