using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Storage : Targetable, ISelectable
{
    public int[] resources;
    private BuildingGenerator buildingGenerator;

    public void Start()
    {
        Selection.Selector.AddSelectable(this);
        resources = new int[(int)ResourceType.MAX_VALUE];
        buildingGenerator = GetComponent<BuildingGenerator>();
    }

    public void OnSelect()
    {
        GameObject HUD = UnitHUD.HUD.AddUnitHUD(gameObject, UnitHUD.HUD.storageHUD, 1f);
        HUD.GetComponent<StorageDisplay>().storage = this;
    }

    public void OnDeselect()
    {
        UnitHUD.HUD.RemoveUnitHUD(gameObject);
    }

    //public override void OnStartClient()
    //{
    //    base.OnStartClient();
    //}

    public void Store(Villager villager, int[] deposit)
    {
        for (int i = 0; i < (int)ResourceType.MAX_VALUE; i++)
        {
            resources[i] += deposit[i];
        }
        if (resources[(int)ResourceType.Wood] >= SimVars.VARS.woodPerHouse)
        {
            buildingGenerator.PlaceBuilding(BuildingType.House);
            resources[(int)ResourceType.Wood] -= SimVars.VARS.woodPerHouse;
        }
        int neededFood = Mathf.Min((int)((villager.maxVitality - villager.vitality) / SimVars.VARS.hungerPerFood), resources[(int)ResourceType.Food]);
        if (resources[(int)ResourceType.Food] >= neededFood)
        {
            resources[(int)ResourceType.Food] -= neededFood;
            villager.vitality += neededFood * SimVars.VARS.hungerPerFood;
        }
    }
}
