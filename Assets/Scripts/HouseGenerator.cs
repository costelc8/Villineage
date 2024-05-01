using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HouseGenerator : MonoBehaviour
{
    public GameObject housePrefab;

    // Start is called before the first frame update
    void Start()
    {
        Camera.main.transform.parent.parent.position = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0)) 
        {
            // test grabbing a point on the navmesh
            if (RandomNavmeshPoint.RandomPoint(transform.position, 5, out Vector3 point))
            {
                // got a point
                GameObject house = Instantiate(housePrefab, new Vector3(point.x, point.y + 2.5f, point.z), Quaternion.identity);
            }
            else
            {
                // failed
            }
        }
    }
}
