using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadPainter : MonoBehaviour
{
    private Terrain terrain;
    private TerrainData terrainData;
    private Vector3 terrainPos;
    private Vector2Int brushPosition;
    private Vector2Int brush;
    private float[,,] map;

    // Start is called before the first frame update
    void Start()
    {
        terrain = TerrainGenerator.terrain;
        terrainData = terrain.terrainData;
       
        int textureCount = terrainData.alphamapLayers;
        map = new float[1, 1, textureCount];
        
    }

    // Update is called once per frame
    void Update()
    {
        int x = (int)transform.position.x;
        int y = (int)transform.position.y;
        map[x, y, 0] = 0f;
        map[x, y, 1] = 1f;
        terrainData.SetAlphamaps(0, 0, map);
    }
}
