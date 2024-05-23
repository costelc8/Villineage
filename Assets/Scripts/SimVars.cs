using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SimVars : NetworkBehaviour
{
    public static SimVars VARS;

    [Header("Timescale")]
    [SyncVar] public float timeScale = 1f;
    [SyncVar] public bool logSim = false;

    [Header("Simulation Seed")]
    [Tooltip("Terrain seed, entering 0 will generate one")]
    [SyncVar] public int seed = 0;

    [Header("Job Weights")]
    [SyncVar] public int lumberjackWeight = 2;
    [SyncVar] public int gathererWeight = 1;
    [SyncVar] public int hunterWeight = 1;
    [SyncVar] public int builderWeight = 1;

    [Header("Villager Variables")]
    [SyncVar] public int startingVillagers = 4;
    [SyncVar] public float villagerSpawnTime = 30f;
    [SyncVar] public int villagerCarryCapacity = 10;
    [SyncVar(hook = nameof(VillagerMoveSpeedHook))] public float villagerMoveSpeed = 4f;
    [SyncVar] public float villagerWorkSpeed = 1f;
    [SyncVar] public float villagerHungerRate = 1f;
    [SyncVar] public float vitalityPerFood = 10f;

    [Header("Resource Variables")]
    [SyncVar] public int startingSheep = 4;
    [SyncVar] public int startingGoats = 6;
    [SyncVar] public int startingWolves = 2;
    [SyncVar] public int woodPerTree = 60;
    [SyncVar] public int foodPerBerry = 60;
    [SyncVar] public float berryRespawnTime = 300f;
    [SyncVar] public int foodPerSheep = 60;
    [SyncVar] public int foodPerGoat = 60;

    [Header("Building Variables")]
    [SyncVar] public int houseBuildCost = 60;
    [SyncVar] public int outpostBuildCost = 60;
    [SyncVar] public int villagerSpawnCost = 30;

    [Header("Terrain Variables")]
    [Tooltip("Map scale, map size will be 2^(4+scale)")]
    [Range(1, 4)]
    [SyncVar] public int terrainScale = 3;
    [HideInInspector]
    [SyncVar] public int terrainSize = 32;
    [Tooltip("Terrain hill height")]
    [SyncVar] public int terrainDepth = 8;

    public void Initialize()
    {
        if (VARS == null || VARS == this) VARS = this;
        else Destroy(this);
        terrainSize = 128;
        for (int i = 1; i < terrainScale; i++) terrainSize *= 2;
        terrainSize++;
    }

    public int GetSeed()
    {
        if (seed == 0) seed = Random.Range(int.MinValue, int.MaxValue);
        return seed;
    }

    public float GetMaxVillagerRange()
    {
        return 50f * villagerMoveSpeed / villagerHungerRate;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (!isServer) Initialize();
    }

    private void Update()
    {
        Time.timeScale = timeScale;
    }

    public void AddResources(int sheepIncrement, int goatsIncrement, int woodIncrement, int berryIncrement)
    {
        startingSheep += sheepIncrement;
        startingGoats += goatsIncrement;
        woodPerTree += woodIncrement;
        foodPerBerry += berryIncrement;
    }

    public void DecResources(int sheepIncrement, int goatsIncrement, int woodIncrement, int berryIncrement)
    {
        startingSheep -= sheepIncrement;
        startingGoats -= goatsIncrement;
        woodPerTree -= woodIncrement;
        foodPerBerry -= berryIncrement;
    }

    public void AddHunger(float rate)
    {
        villagerHungerRate+=rate;
    }

    public void DecHunger(float rate)
    {
        villagerHungerRate-=rate;
    }

    // void OnStartingSheepChanged(int oldValue, int newValue)
    // {
    //     startingSheep = newValue;
    //     // Update logic when startingSheep changes
    // }

    // void OnStartingGoatsChanged(int oldValue, int newValue)
    // {
    //     startingGoats = newValue;
    //     // Update logic when startingGoats changes
    // }

    // void OnWoodPerTreeChanged(int oldValue, int newValue)
    // {
    //     woodPerTree = newValue;
    //     // Update logic when woodPerTree changes
    // }

    // void OnFoodPerBerryChanged(int oldValue, int newValue)
    // {
    //     foodPerBerry = newValue;
    //     // Update logic when foodPerBerry changes
    // }

    void VillagerMoveSpeedHook(float oldSpeed, float newSpeed)
    {
        foreach(Villager v in TownCenter.TC.villagers)
        {
            NavMeshAgent agent = v.GetComponent<NavMeshAgent>();
            if (agent != null)
            {
                agent.speed = newSpeed;
                agent.acceleration = newSpeed;
            }
        }
    }
}
