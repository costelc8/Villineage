using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class SimulationManager : NetworkManager
{
    [Header("Simulation Managers")]
    public bool runSimulation;
    public SimVars simVars;
    public TerrainGenerator terrainGenerator;
    public ResourceGenerator resourceGenerator;
    public TownCenter townCenter;
    public SimLogs simLogs;

    public override void Awake()
    {
        simVars.Initialize();
        townCenter.Initialize();
        terrainGenerator.Initialize();
        resourceGenerator.Initialize();
    }

    public void StartSimulation()
    {
        terrainGenerator.GenerateTerrain();
        townCenter.PlaceOnGround();
        resourceGenerator.GeneratePerlinTrees();
        resourceGenerator.GeneratePerlinBerries();
        resourceGenerator.GeneratePerlinAnimals();
        //resourceGenerator.GenerateSheep(townCenter.transform.position, SimVars.VARS.startingSheep, 6f * SimVars.VARS.terrainScale, 9f * SimVars.VARS.terrainScale);
        //resourceGenerator.GenerateGoats(townCenter.transform.position, SimVars.VARS.startingGoats, 12f * SimVars.VARS.terrainScale, 18f * SimVars.VARS.terrainScale);
        //resourceGenerator.GenerateWolves(townCenter.transform.position, SimVars.VARS.startingWolves, 18f * SimVars.VARS.terrainScale, 24f * SimVars.VARS.terrainScale);
        townCenter.SpawnVillagers(SimVars.VARS.startingVillagers);
        if (SimVars.VARS.logSim) simLogs.StartLogging();
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        StartSimulation();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        Time.timeScale = SimVars.VARS.timeScale;
    }

    public override void OnStopServer()
    {
        base.OnStopServer();
        simLogs.StopLogging();
    }
}
