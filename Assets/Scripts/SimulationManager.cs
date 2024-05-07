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
        resourceGenerator.GenerateDefaultForest();
        townCenter.PlaceOnGround();
        townCenter.SpawnVillagers(5);
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        StartSimulation();
    }
}
