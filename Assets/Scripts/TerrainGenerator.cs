using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using Unity.AI.Navigation;
using UnityEngine;

[RequireComponent(typeof(NavMeshSurface))]
public class TerrainGenerator : MonoBehaviour
{
    private Terrain terrain;
    public int depth = 8;
    public int seed;

    private int size;
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
        GenerateRandomOffsets();
        perlin = GeneratePerlin();
        data.SetHeights(0, 0, perlin);
    }

    float[,] GeneratePerlin()
    {
        float[,] perlin = new float[size, size];
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                perlin[x, y] = CalculatePerlin(x, y);
            }
        }
        return perlin;
    }

    float CalculatePerlin(int x, int y)
    {
        float xCoord = x / (size * scale / 10) + randomX;
        float yCoord = y / (size * scale / 10) + randomY;

        return Mathf.PerlinNoise(xCoord, yCoord);
    }

    void GenerateRandomOffsets()
    {
        if (seed == 0) seed = Random.Range(int.MinValue, int.MaxValue);
        Random.State state = Random.state; // Save Random's state
        Random.InitState(seed); // Seed Random's state
        randomX = Random.Range(-10000 * scale, 10000 * scale);
        randomY = Random.Range(-10000 * scale, 10000 * scale);
        Random.state = state; // Restore Random's state
    }

    public float GetTerrainHeight(Vector3 position)
    {
        return terrain.terrainData.GetHeight(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.z));
    }
}