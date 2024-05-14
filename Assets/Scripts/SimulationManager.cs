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
        resourceGenerator.GenerateBerries();
        resourceGenerator.GenerateAnimals(townCenter.transform.position, 10, 20f, 30f);
        townCenter.PlaceOnGround();
        townCenter.SpawnVillagers(4);
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        StartSimulation();
    }
}
