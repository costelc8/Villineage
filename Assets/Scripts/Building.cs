using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building : Targetable
{
    public Villager assignedVillager;
    public int maxBuildProgress;
    public int buildCap;
    public int stage;
    public BuildingType buildingType;

    [SyncVar(hook = nameof(ProgressHook))]
    public float buildTime;

    public override bool Progress(Villager villager, float progressValue)
    {
        if (buildTime < maxBuildProgress)
        {
            //if (buildTime >= buildCap)
            //{
            //    buildTime = buildCap;
            //    UntargetAll();
            //}
            buildTime += progressValue;
            UpdateStage();
            if (buildTime >= maxBuildProgress)
            {
                buildTime = maxBuildProgress;
                BuildingGenerator.AddBuilding(this);
                UntargetAll();
                return true;
            }
            else return false;
        }
        else return true;
    }

    public int ContributeWood(int woodAmount)
    {
        int neededWood = maxBuildProgress - buildCap;
        int usedWood = Math.Min(neededWood, woodAmount);
        buildCap += usedWood;
        return usedWood;
    }

    private void ProgressHook(float oldProgress, float newProgress)
    {
        UpdateStage();
    }

    private void UpdateStage()
    {
        int newStage = Mathf.Clamp((int)(buildTime * (transform.childCount - 1) / maxBuildProgress), 0, transform.childCount - 1);
        if (newStage != stage)
        {
            stage = newStage;
            for (int i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).gameObject.SetActive(i == stage);
            }
        }
    }

    private void OnDestroy()
    {
        BuildingGenerator.RemoveBuilding(this);
    }
}