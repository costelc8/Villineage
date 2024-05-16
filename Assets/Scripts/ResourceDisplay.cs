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
}
