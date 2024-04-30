using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    public int depth = 8;
    public int width = 256;
    public int length = 256;
    public float scale = 10f;

    private void Start()
    {
        Terrain terrain = GetComponent<Terrain>();
        terrain.terrainData = GenerateTerrain(terrain.terrainData);
        //transform.position = new Vector3(-width/2, 0, -length/2);
    }

    TerrainData GenerateTerrain(TerrainData data)
    {
        data.heightmapResolution = width + 1;
        data.size = new Vector3(width, depth, length);
        data.SetHeights(0, 0, GeneratePerlin());
        return data;
    }

    float[,] GeneratePerlin()
    {
        float[,] perlin = new float[width, length];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < length; y++)
            {
                perlin[x, y] = CalculatePerlin(x, y);
            }
        }
        return perlin;
    }

    float CalculatePerlin(int x, int y)
    {
        float xCoord = (float)x / width * scale;
        float yCoord = (float)y / length * scale;

        return Mathf.PerlinNoise(xCoord, yCoord);
    }
}
