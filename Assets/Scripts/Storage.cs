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

    public void Store(int[] deposit)
    {
        for (int i = 0; i < (int)ResourceType.MAX_VALUE; i++)
        {
            resources[i] += deposit[i];
        }
    }

    public void Deposit(Villager villager, int[] deposit)
    {
        Store(deposit);
        int neededFood = Mathf.Min((int)((villager.maxVitality - villager.vitality) / SimVars.VARS.vitalityPerFood), resources[(int)ResourceType.Food]);
        if (resources[(int)ResourceType.Food] >= neededFood)
        {
            // eat
            resources[(int)ResourceType.Food] -= neededFood;
            villager.vitality += neededFood * SimVars.VARS.vitalityPerFood;
        }
        TownCenter.TC.CheckSpawnHouse();
        TownCenter.TC.SpawnCheck();
        TownCenter.TC.AssignVillagerJob(villager);
    }

    public void Collect(Villager villager, ResourceType resource)
    {
        villager.inventory[(int)resource] += SimVars.VARS.villagerCarryCapacity;
        resources[(int)resource] -= SimVars.VARS.villagerCarryCapacity;
        villager.totalResources = SimVars.VARS.villagerCarryCapacity;
    }

    private void OnDrawGizmosSelected()
    {
        if (SimVars.VARS != null) Gizmos.DrawWireSphere(transform.position, SimVars.VARS.GetMaxVillagerRange() / 2f);
    }
}
