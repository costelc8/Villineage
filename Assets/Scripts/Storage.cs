using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Storage : Targetable, ISelectable
{
    public readonly SyncList<int> resources = new SyncList<int>();

    public void Start()
    {
        Selection.Selector.AddSelectable(this);
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        resources.Clear();
        for (int i = 0; i < (int)ResourceType.MAX_VALUE; i++) resources.Add(0);
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

    public void Store(Villager villager, int[] deposit)
    {
        for (int i = 0; i < (int)ResourceType.MAX_VALUE; i++)
        {
            resources[i] += deposit[i];
        }
        if (resources[(int)ResourceType.Wood] >= SimVars.VARS.houseBuildCost)
        {
            TownCenter.TC.buildingGenerator.PlaceBuilding(BuildingType.House);
            resources[(int)ResourceType.Wood] -= SimVars.VARS.houseBuildCost;
        }
        int neededFood = Mathf.Min((int)((villager.maxVitality - villager.vitality) / SimVars.VARS.vitalityPerFood), resources[(int)ResourceType.Food]);
        if (resources[(int)ResourceType.Food] >= neededFood)
        {
            resources[(int)ResourceType.Food] -= neededFood;
            villager.vitality += neededFood * SimVars.VARS.vitalityPerFood;
        }
        TownCenter.TC.OnDeposit();
        TownCenter.TC.AssignVillagerJob(villager);
    }
}
