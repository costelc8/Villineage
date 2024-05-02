using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building : MonoBehaviour
{
    private GameObject stage0;
    private GameObject stage1;
    private GameObject stage2;
    public Villager assignedVillager;
    public float maxbuildTime = 10f;
    private float buildTime;
    public int stage;

    // Start is called before the first frame update
    void Start()
    {
        stage0 = transform.GetChild(0).gameObject;
        stage1 = transform.GetChild(1).gameObject;
        stage2 = transform.GetChild(2).gameObject;
    }

    private void Update()
    {
        //Progress(Time.deltaTime);
    }

    public bool Progress(float progressValue)
    {
        buildTime += progressValue;
        stage = Mathf.Clamp((int)(buildTime * 2 / maxbuildTime), 0, 2);
        stage0.SetActive(stage == 0);
        stage1.SetActive(stage == 1);
        stage2.SetActive(stage == 2);
        if (buildTime >= maxbuildTime)
        {
            buildTime = maxbuildTime;
            BuildingGenerator.AddBuilding(this);
            return true;
        }
        return false;
    }
}
