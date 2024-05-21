using Mirror;
using Mono.CecilX;
using System;
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

    public void VillagerDeposit(Villager villager, int[] items)
    {
        for (int i = 0; i < (int)ResourceType.MAX_VALUE; i++)
        {
            int storing = (int)items[i];
            resources[i] += storing;
            villager.inventory[i] -= storing;
            villager.totalResources -= storing;
        }
        int neededFood = Mathf.Min((int)((villager.maxVitality - villager.vitality) / SimVars.VARS.vitalityPerFood), resources[(int)ResourceType.Food]);
        if (resources[(int)ResourceType.Food] >= neededFood)
        {
            // eat
            resources[(int)ResourceType.Food] -= neededFood;
            villager.vitality += neededFood * SimVars.VARS.vitalityPerFood;
        }
        TownCenter.TC.HouseSpawnCheck();
        TownCenter.TC.VillagerSpawnCheck();
        TownCenter.TC.AssignVillagerJob(villager);
    }

    public void VillagerCollect(Villager villager, int[] items)
    {
        for (int i = 0; i < (int)ResourceType.MAX_VALUE; i++)
        {
            int wants = items[i];
            int available = resources[i];
            int canHold = SimVars.VARS.villagerCarryCapacity - villager.totalResources;

            int takes = Math.Min(canHold, Math.Min(wants, available)); 
            
            resources[i] -= takes;
            villager.inventory[i] += takes;
            villager.totalResources += takes;
        }
        
    }

    private void OnDrawGizmosSelected()
    {
        if (SimVars.VARS != null) Gizmos.DrawWireSphere(transform.position, SimVars.VARS.GetMaxVillagerRange() / 2f);
    }
}
