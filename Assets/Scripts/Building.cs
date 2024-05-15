using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building : Targetable
{
    public Villager assignedVillager;
    public float maxBuildTime;
    public int stage;

    [SyncVar(hook = nameof(ProgressHook))]
    public float buildTime;

    public override bool Progress(Villager villager, float progressValue)
    {
        buildTime += progressValue;
        UpdateStage();
        if (buildTime >= maxBuildTime)
        {
            buildTime = maxBuildTime;
            BuildingGenerator.AddBuilding(this);
            return true;
        }
        return false;
    }

    private void ProgressHook(float oldProgress, float newProgress)
    {
        UpdateStage();
    }

    private void UpdateStage()
    {
        int newStage = Mathf.Clamp((int)(buildTime * (transform.childCount - 1) / maxBuildTime), 0, transform.childCount - 1);
        if (newStage != stage)
        {
            stage = newStage;
            for (int i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).gameObject.SetActive(i == stage);
            }
        }
    }
}
