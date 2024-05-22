using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        Application.targetFrameRate = 60;
        simVars.Initialize();
        townCenter.Initialize();
        terrainGenerator.Initialize();
        resourceGenerator.Initialize();
    }

    public void StartSimulation()
    {
        terrainGenerator.GenerateTerrain();
        townCenter.PlaceOnGround();
        resourceGenerator.GenerateDefaultForest();
        resourceGenerator.GenerateBerries();
        resourceGenerator.GenerateSheep(townCenter.transform.position, SimVars.VARS.startingSheep, 20f, 30f);
        resourceGenerator.GenerateGoats(townCenter.transform.position, SimVars.VARS.startingGoats, 30f, 45f);
        resourceGenerator.GenerateWolves(townCenter.transform.position, SimVars.VARS.startingWolves, 40f, 60f);
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
