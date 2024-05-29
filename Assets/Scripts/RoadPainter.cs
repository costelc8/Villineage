using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class RoadPainter : MonoBehaviour
{
    private Terrain terrain;
    private TerrainData terrainData;
    private Vector3 terrainPos;
    private float[,,] map;
    private Vector3 lastPosition;
    private Vector3 position;
    private NavMeshAgent agent;
    private bool drawing;

    // Start is called before the first frame update
    void Start()
    {
        terrain = TerrainGenerator.terrain;
        terrainData = terrain.terrainData;
        agent = GetComponent<NavMeshAgent>();

        int textureCount = terrainData.alphamapLayers;
        map = new float[1, 1, textureCount];
        lastPosition = transform.position;
        position = lastPosition;
        drawing = false;

    }
    // Update is called once per frame
    private void Update()
    {
        if (agent.speed > 0 && !drawing)
        {
            // moving
            drawing = true;
            InvokeRepeating("DrawPath", 0, 4);

        } 
        else if (agent.speed == 0 && drawing)
        {
            // stopped
            CancelInvoke("DrawPath");
            drawing = false;
        }

    }


    void DrawPath()
    {
        position = transform.position;
        int lastX = (int)lastPosition.x;
        int lastY = (int)lastPosition.z;
        int x = (int)position.x;
        int y = (int)position.z;

        if (x != lastX || y != lastY)
        {
            float[,,] currentMap = terrainData.GetAlphamaps(x, y, 1, 1);
            float grass = currentMap[0, 0, 0];
            if (grass > 0f)
            {
                // not fully sand
                grass -= 0.05f;
                map[0, 0, 0] = grass;
                map[0, 0, 1] = 1 - grass;
                terrainData.SetAlphamaps((int)position.x, (int)position.z, map);
            }
        }
        lastPosition = position;
    }
}
