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
    public static Terrain terrain;
    private TerrainData terrainData;
    private NavMeshSurface navMesh;
    public bool generateTerrainOnStart;
    private bool generated;

    [Tooltip("Perlin image scale size, a smaller scale will generate more hilly terrain")]
    [Range(0.5f, 2f)]
    public float perlinScale = 1f;
    private float[,] perlin;

    private void Start()
    {
        if (generateTerrainOnStart) GenerateTerrain();
    }

    private void Update()
    {
        if (!generated && !isServer && SimVars.VARS != null) GenerateTerrain();
    }

    public void Initialize()
    {
        terrain = GetComponent<Terrain>();
        terrainData = terrain.terrainData;
        navMesh = GetComponent<NavMeshSurface>();
    }

    public void GenerateTerrain()
    {
        Debug.Log("Generating Terrain");
        int size = SimVars.VARS.terrainSize;
        int depth = SimVars.VARS.terrainDepth;
        terrainData.heightmapResolution = size + 1;
        terrainData.size = new Vector3(size, depth, size);
        perlin = PerlinGenerator.GeneratePerlin(size + 1, size + 1, perlinScale, SimVars.VARS.GetSeed());
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
        return terrain.terrainData.GetSteepness(position.x / SimVars.VARS.terrainSize, position.z / SimVars.VARS.terrainSize);
    }
}