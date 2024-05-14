using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimVars : NetworkBehaviour
{
    public static SimVars VARS;

    [Header("Timescale")]
    [SyncVar]
    public float timeScale = 1f;

    [Header("Simulation Seed")]
    [Tooltip("Terrain seed, entering 0 will generate one")]
    //[SyncVar(hook = nameof(SeedHook))]
    public int seed = 0;

    [Header("Villager Variables")]
    public int startingVillagers = 4;
    public int villagerCarryCapacity = 5;
    public float villagerMoveSpeed = 4f;
    public float villagerWorkSpeed = 1f;
    public float villagerHungerRate = 1f;

    [Header("Resource Variables")]
    public int startingSheep = 4;
    public int startingGoats = 4;
    public int woodPerTree = 60;
    public int foodPerBerry = 60;
    public int foodPerSheep = 60;
    public int foodPerGoat = 120;

    [Header("Terrain Variables")]
    [Tooltip("Map scale, map size will be 2^(4+scale)")]
    [Range(2, 8)]
    public int terrainScale = 5;
    [HideInInspector]
    public int terrainSize = 32;
    [Tooltip("Terrain hill height")]
    public int terrainDepth = 8;

    private void Awake()
    {
        if (VARS == null || VARS == this) VARS = this;
        else Destroy(this);
        Time.timeScale = timeScale;
        terrainSize = 32;
        for (int i = 2; i < terrainScale; i++) terrainSize *= 2;
        terrainSize++;
    }
}
