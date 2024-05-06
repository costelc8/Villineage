using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimulationManager : MonoBehaviour
{
    private NetworkManager networkManager;
    public bool runSimulation;
    public TerrainGenerator terrainGenerator;
    public ResourceGenerator resourceGenerator;
    public TownCenter townCenter;

    private void Awake()
    {
        networkManager = GetComponent<NetworkManager>();
    }

    public void StartSimulation()
    {
        terrainGenerator.GenerateTerrain();
        resourceGenerator.GenerateDefaultForest();
        townCenter.PlaceOnGround();
        townCenter.SpawnVillagers(5);
    }
}
