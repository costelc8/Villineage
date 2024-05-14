using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Runtime.CompilerServices;
using Unity.AI.Navigation;
using UnityEngine;

[RequireComponent(typeof(NavMeshSurface))]
public class TerrainGenerator : NetworkBehaviour
{
    private Terrain terrain;
    private TerrainData terrainData;
    private NavMeshSurface navMesh;
    public bool generateTerrainOnStart;
    private bool initialized;
    private bool generated = false;

    [HideInInspector]
    public int size;

    [Tooltip("Perlin image scale size, a smaller scale will generate more hilly terrain")]
    [Range(0.5f, 2f)]
    public float perlinScale = 1f;
    private float[,] perlin;

    private void Awake()
    {
        if (!initialized) Initialize();
    }

    private void Start()
    {
        if (generateTerrainOnStart) GenerateTerrain();
    }

    private void Initialize()
    {
        terrain = GetComponent<Terrain>();
        terrainData = terrain.terrainData;
        navMesh = GetComponent<NavMeshSurface>();
        initialized = true;
    }

    private void SeedHook(int oldSeed, int newSeed)
    {
        Debug.Log("Seed Changed");
        if (!generated) GenerateTerrain();
    }

    public void GenerateTerrain(int terrainScale = 5, int terrainDepth = 8, int seed = 0)
    {
        if (!initialized) Initialize();
        Debug.Log("Generating Terrain");
        size = 32;
        for (int i = 1; i < terrainScale; i++) size *= 2;
        size++;
        terrainData.heightmapResolution = size;
        terrainData.size = new Vector3(size - 1, terrainDepth, size - 1);
        if (seed == 0) seed = Random.Range(int.MinValue, int.MaxValue);
        perlin = PerlinGenerator.GeneratePerlin(size, size, perlinScale, seed);
        terrainData.SetHeights(0, 0, perlin);
        gameObject.SetActive(true);
        navMesh.BuildNavMesh();
        generated = true;
    }

    public float GetTerrainHeight(Vector3 position)
    {
        return terrain.terrainData.GetHeight(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.z));
    }

    public float GetTerrainSteepness(Vector3 position)
    {
        return terrain.terrainData.GetSteepness(position.x / size, position.z / size);
    }
}