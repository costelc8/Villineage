using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class Building : Targetable, ISelectable
{
    public Villager assignedVillager;
    [SyncVar]
    public int requiredWood;
    [SyncVar]
    public int currentWood;
    public int stage;
    public BuildingType buildingType;
    public GameObject cartPrefab;
    public GameObject cartInspectorParent;
    public Storage storageParent;

    [SyncVar(hook = nameof(ProgressHook))]
    public int buildProgress;

    public override bool Progress(Villager villager)
    {
        buildProgress++;
        UpdateStage();
        if (buildProgress >= requiredWood)
        {
            buildProgress = requiredWood;
            switch(buildingType)
            {
                case BuildingType.House: 
                    BuildingGenerator.AddHouse(this); 
                    break;
                case BuildingType.Outpost:
                    BuildingGenerator.AddOutpost(this);
                    // cart for outpost
                    RandomNavmeshPoint.RandomPointFromCenterSphere(transform.position, 1, out Vector3 point, 5, 1, 1000);
                    GameObject cart = Instantiate(cartPrefab, point, Quaternion.identity, cartInspectorParent.transform);
                    Storage storage = GetComponent<Storage>();
                    storage.enabled = true;
                    storage.Initialize();
                    cart.GetComponent<NavMeshAgent>().avoidancePriority = Random.Range(0, 20);
                    cart.GetComponent<Cart>().hub = storage;
                    NetworkServer.Spawn(cart);
                    break;
            }
            UntargetAll(true);
            return true;
        }
        if (buildProgress >= currentWood)
        {
            buildProgress = currentWood;
            return false;
        }
        return true;
    }

    public void ContributeWood(Villager villager)
    {
        int neededWood = requiredWood - currentWood;
        int usedWood = Math.Min(neededWood, villager.inventory[(int)ResourceType.Wood]);
        currentWood += usedWood;
        villager.inventory[(int)ResourceType.Wood] -= usedWood;
        villager.totalResources -= usedWood;
    }

    private void ProgressHook(int oldProgress, int newProgress)
    {
        if (oldProgress == 0) priority *= 1.5f;
        UpdateStage();
    }

    private void UpdateStage()
    {
        int newStage = Mathf.Clamp((buildProgress * (transform.childCount - 1) / requiredWood), 0, transform.childCount - 1);
        if (newStage != stage)
        {
            stage = newStage;
            for (int i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).gameObject.SetActive(i == stage);
            }
        }
    }

    public void OnDestroy()
    {
        BuildingGenerator.RemoveBuilding(this);
        Selection.Selector.RemoveSelectable(this);
        UnitHUD.HUD.RemoveUnitHUD(gameObject);
    }

    public void Start()
    {
        Selection.Selector.AddSelectable(this);
    }

    public void OnSelect()
    {
        if (buildProgress < requiredWood)
        {
            GameObject HUD = UnitHUD.HUD.AddUnitHUD(gameObject, UnitHUD.HUD.buildingHUD, 4f);
            HUD.GetComponent<BuildingDisplay>().building = this;
        }
    }

    public void OnDeselect()
    {
        UnitHUD.HUD.RemoveUnitHUD(gameObject);
    }
}