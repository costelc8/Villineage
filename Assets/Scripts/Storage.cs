using Mirror;
using Mono.CecilX;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Storage : Targetable, ISelectable
{
    public readonly SyncList<int> resources = new SyncList<int>();
    public List<Villager> villagers = new List<Villager>();
    public int[] neededResources;
    public int[] requestedResources;

    public void Start()
    {
        Selection.Selector.AddSelectable(this);
        neededResources = new int[(int)ResourceType.MAX_VALUE];
        requestedResources = new int[(int)ResourceType.MAX_VALUE];
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

    public void Deposit(Villager villager)
    {
        for (int i = 0; i < (int)ResourceType.MAX_VALUE; i++)
        {
            int storing = villager.inventory[i];
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
        UpdateResourceRequest();
        TownCenter.TC.HouseSpawnCheck();
        TownCenter.TC.VillagerSpawnCheck();
        TownCenter.TC.AssignVillagerJob(villager);
    }

    public void Deposit(Cart cart)
    {
        for (int i = 0; i < (int)ResourceType.MAX_VALUE; i++)
        {
            int storing = cart.inventory[i];
            resources[i] += storing;
            cart.inventory[i] -= storing;
        }
        UpdateResourceRequest();
        TownCenter.TC.HouseSpawnCheck();
        TownCenter.TC.VillagerSpawnCheck();
    }

    public void Request(Villager villager, ResourceType resource)
    {
        int available = resources[(int)resource];
        int canHold = SimVars.VARS.villagerCarryCapacity - villager.totalResources;

        int takes = Math.Min(available, canHold); 
            
        resources[(int)resource] -= takes;
        villager.inventory[(int)resource] += takes;
        villager.totalResources += takes;
    }

    public void Request(Cart cart, int[] request)
    {
        for (int i = 0; i < (int)ResourceType.MAX_VALUE; i++)
        {
            int available = Mathf.Max(resources[i] - neededResources[i], 0);
            int canHold = (SimVars.VARS.villagerCarryCapacity * 10) - cart.inventory[i];

            int takes = Mathf.Min(available, canHold, request[i]);

            resources[i] -= takes;
            cart.inventory[i] += takes;
        }
    }

    public void Collect(Cart cart)
    {
        for (int i = 0; i < (int)ResourceType.MAX_VALUE; i++)
        {
            int available = Mathf.Max(resources[i] - neededResources[i], 0);
            int canHold = (SimVars.VARS.villagerCarryCapacity * 10) - cart.inventory[i];

            int takes = Math.Min(available, canHold);

            resources[i] -= takes;
            cart.inventory[i] += takes;
        }
    }

    private void UpdateResourceRequest()
    {
        neededResources[(int)ResourceType.Food] = (int)(villagers.Count * (100 / SimVars.VARS.vitalityPerFood));
        neededResources[(int)ResourceType.Wood] = SimVars.VARS.houseBuildCost + SimVars.VARS.outpostBuildCost;
        for (int i = 0; i < (int)ResourceType.MAX_VALUE; i++)
        {
            requestedResources[i] = Mathf.Max(neededResources[i] - resources[i], 0);
        }
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        if (SimVars.VARS != null) Gizmos.DrawWireSphere(transform.position, SimVars.VARS.GetMaxVillagerRange() / 2f);
    }
}
