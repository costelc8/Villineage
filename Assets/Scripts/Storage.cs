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

    public void VillagerDeposit(Villager villager)
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
        TownCenter.TC.HouseSpawnCheck();
        TownCenter.TC.VillagerSpawnCheck();
        TownCenter.TC.AssignVillagerJob(villager);
    }

    public void VillagerCollect(Villager villager, ResourceType resource)
    {
        int available = resources[(int)resource];
        int canHold = villager.capacity - villager.totalResources;

        int takes = Math.Min(available, canHold); 
            
        resources[(int)resource] -= takes;
        villager.inventory[(int)resource] += takes;
        villager.totalResources += takes;
    }

    public void CartDeposit(Cart cart)
    {
        for (int i = 0; i < (int)ResourceType.MAX_VALUE; i++)
        {
            int storing = cart.inventory[i];
            resources[i] += storing;
            cart.inventory[i] -= storing;
            cart.totalResources -= storing;
        }
        TownCenter.TC.HouseSpawnCheck();
        TownCenter.TC.VillagerSpawnCheck();
    }

    public void CartCollect(Cart cart, ResourceType resource, int amount)
    {
        int available = resources[(int)resource];
        int canHold = cart.capacity - cart.totalResources;

        int takes = Math.Min(Math.Min(available, canHold), amount);

        resources[(int)resource] -= takes;
        cart.inventory[(int)resource] += takes;
        cart.totalResources += takes;
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        if (SimVars.VARS != null) Gizmos.DrawWireSphere(transform.position, SimVars.VARS.GetMaxVillagerRange() / 2f);
    }
}
