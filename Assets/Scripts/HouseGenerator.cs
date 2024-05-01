using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HouseGenerator : MonoBehaviour
{
    public GameObject housePrefab;
    public TerrainGenerator terrainGen;
    Vector3 center;
    Vector3 halfExtents;

    // Start is called before the first frame update
    void Start()
    {
        Camera.main.transform.parent.parent.position = transform.position;
        center = new Vector3(terrainGen.size, 0, terrainGen.size);
        Collider houseCollider = housePrefab.GetComponent<Collider>();
        float houseSize = houseCollider.gameObject.transform.localScale.x;
        halfExtents = new Vector3(houseSize, houseSize, houseSize);
    }

    // Update is called once per frame
    void Update()
    {
        
        
            // test grabbing a point on the navmesh
            if (RandomNavmeshPoint.RandomPointFromCenterBox(center, halfExtents, out Vector3 point, 5f, 5f, 1000f))
            {
                // got a point
                print("house");
                GameObject house = Instantiate(housePrefab, new Vector3(point.x, point.y + 2.5f, point.z), Quaternion.identity);
            }
            else
            {
                // failed
            }
        
    }
}
