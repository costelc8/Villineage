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
    public int depth = 8;
    public int seed;

    public int size;
    private float randomX;
    private float randomY;

    public float scale = 10f;
    public float[,] perlin;

    private void Start()
    {
        terrain = GetComponent<Terrain>();
        GenerateTerrain(terrain.terrainData);
        NavMeshSurface surface = GetComponent<NavMeshSurface>();
        surface.BuildNavMesh();
    }

    void GenerateTerrain(TerrainData data)
    {
        size = data.heightmapResolution;
        data.size = new Vector3(size - 1, depth, size - 1);
        perlin = PerlinGenerator.GeneratePerlin(size, size, scale, seed);
        data.SetHeights(0, 0, perlin);
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