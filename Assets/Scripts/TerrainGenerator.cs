using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Runtime.CompilerServices;
using Unity.AI.Navigation;
using UnityEngine;

[RequireComponent(typeof(NavMeshSurface))]
public class TerrainGenerator : MonoBehaviour
{
    private Terrain terrain;
    private TerrainData terrainData;
    private NavMeshSurface navMesh;
    public bool generateTerrainOnStart;
    [Tooltip("Terrain hill height")]
    public int depth = 8;
    [Tooltip("Terrain seed, entering 0 will generate one")]
    public int seed;

    [HideInInspector]
    public int size;

    [Tooltip("Perlin image scale size, entering 0 will be flat")]
    [Range(0.5f, 1.5f)]
    public float scale = 1f;
    public float[,] perlin;

    private void Awake()
    {
        terrain = GetComponent<Terrain>();
        terrainData = terrain.terrainData;
        navMesh = GetComponent<NavMeshSurface>();
    }

    private void Start()
    {
        if (generateTerrainOnStart)
        {
            GenerateTerrain();
        }
    }

    public void GenerateTerrain()
    {
        size = terrainData.heightmapResolution;
        terrainData.size = new Vector3(size - 1, depth, size - 1);
        perlin = PerlinGenerator.GeneratePerlin(size, size, scale, seed);
        terrainData.SetHeights(0, 0, perlin);
        navMesh.BuildNavMesh();
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