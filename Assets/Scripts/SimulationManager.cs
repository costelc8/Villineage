using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimulationManager : NetworkManager
{
    [Header("Simulation Managers")]
    public bool runSimulation;
    public TerrainGenerator terrainGenerator;
    public ResourceGenerator resourceGenerator;
    public TownCenter townCenter;

    public void StartSimulation()
    {
        terrainGenerator.GenerateTerrain();
        townCenter.PlaceOnGround();
        resourceGenerator.GenerateDefaultForest();
        resourceGenerator.GenerateBerries();
        resourceGenerator.GenerateAnimals(townCenter.transform.position, SimVars.VARS.startingSheep, 20f, 30f);
        townCenter.SpawnVillagers(SimVars.VARS.startingVillagers);
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        StartSimulation();
    }
}
