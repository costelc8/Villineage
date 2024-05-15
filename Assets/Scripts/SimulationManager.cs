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

    public void StartSimulation()
    {
        simVars.Initialize();
        terrainGenerator.GenerateTerrain();
        townCenter.PlaceOnGround();
        resourceGenerator.GenerateDefaultForest();
        resourceGenerator.GenerateBerries();
        resourceGenerator.GenerateSheep(townCenter.transform.position, SimVars.VARS.startingSheep, 20f, 30f);
        resourceGenerator.GenerateGoats(townCenter.transform.position, SimVars.VARS.startingGoats, 30f, 45f);
        townCenter.SpawnVillagers(SimVars.VARS.startingVillagers);
        Time.timeScale = SimVars.VARS.timeScale;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        StartSimulation();
    }
}
