using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadPainter : MonoBehaviour
{
    private Terrain terrain;
    private TerrainData terrainData;
    private Vector3 terrainPos;
    private float[,,] map;
    private Vector3 lastPosition;
    private Vector3 position; 

    // Start is called before the first frame update
    void Start()
    {
        terrain = TerrainGenerator.terrain;
        terrainData = terrain.terrainData;
       
        int textureCount = terrainData.alphamapLayers;
        map = new float[1, 1, textureCount];
        lastPosition = transform.position;
        position = lastPosition;

    }

    // Update is called once per frame
    void Update()
    {
        position = transform.position;
        int lastX = (int)lastPosition.x;
        int lastY = (int)lastPosition.z;
        int x = (int)position.x;
        int y = (int)position.z;

        if (x != lastX || y != lastY)
        {
            float[,,] currentMap = terrainData.GetAlphamaps(x, y, 1, 1);
            map[0, 0, 0] = currentMap[0, 0, 0] - 0.1f;
            map[0, 0, 1] = currentMap[0, 0, 1] + 0.1f;
            terrainData.SetAlphamaps((int)position.x, (int)position.z, map);
       
        }
        lastPosition = position;

    }
}
