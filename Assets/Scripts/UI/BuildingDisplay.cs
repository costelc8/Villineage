using Mono.CecilX;
using System.Collections;
using System.Collections.Generic;
using System.Resources;
using TMPro;
using UnityEngine;

public class BuildingDisplay : MonoBehaviour
{
    public Building building;
    public TextMeshProUGUI woodText;
    public TextMeshProUGUI progressText;

    // Update is called once per frame
    void Update()
    {
        woodText.text = "Wood: " + building.currentWood + "/" + building.requiredWood;
        progressText.text = (100 * building.buildProgress / building.requiredWood) + "% Done";
    }
}
