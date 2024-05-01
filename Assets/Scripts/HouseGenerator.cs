using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HouseGenerator : MonoBehaviour
{
    public GameObject housePrefab;
    public TerrainGenerator terrainGen;
    Vector3 center;
    Vector3 houseSize;
    Vector3 halfHouse;

    // Start is called before the first frame update
    void Start()
    {
        Camera.main.transform.parent.parent.position = transform.position;
        center = new Vector3(terrainGen.size/2, 0, terrainGen.size/2);
        Vector3 houseSize = housePrefab.GetComponent<BoxCollider>().size;
        halfHouse = new Vector3(houseSize.x / 2, houseSize.y / 2, houseSize.z / 2);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            // test grabbing a point on the navmesh
            if (RandomNavmeshPoint.RandomPointFromCenterBox(center, halfHouse, out Vector3 point, 5f, 5f, 1000f))
            {
                // got a point
                print("house");
                GameObject house = Instantiate(housePrefab, new Vector3(point.x, point.y + 2.5f, point.z), Quaternion.identity);
            }
            else
            {
                // failed
                print("house failure");
            }
        }
            
        
    }
}
