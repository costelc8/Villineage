using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    [SyncVar] public float villagerSpawnTime = 30.0f;
    [SyncVar] public int villagerCarryCapacity = 10;
    [SyncVar] public float villagerMoveSpeed = 4f;
    [SyncVar] public float villagerWorkSpeed = 1f;
    [SyncVar] public float villagerHungerRate = 1f;
    [SyncVar] public float vitalityPerFood = 10f;

    [Header("Resource Variables")]
    [SyncVar] public int startingSheep = 4;
    [SyncVar] public int startingGoats = 6;
    [SyncVar] public int woodPerTree = 60;
    [SyncVar] public int foodPerBerry = 60;
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
}
