using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HouseGenerator : MonoBehaviour
{
    public GameObject housePrefab;

    private Vector3 center;
    private Vector3 spacingSize;
    private int spacing = 2;

    // Start is called before the first frame update
    void Start()
    {
        Camera.main.transform.parent.parent.position = transform.position;
        print(transform.position);
        center = transform.position;
        spacingSize = housePrefab.GetComponent<BoxCollider>().size / 2 + new Vector3(spacing, spacing, spacing);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            // test grabbing a point on the navmesh
            if (RandomNavmeshPoint.RandomPointFromCenterBox(center, spacingSize, out Vector3 point, 6f, 6f, 1000f))
            {
                // got a point

                GameObject house = Instantiate(housePrefab, point, Quaternion.identity);
            }
            else
            {
                // failed

            }
        }
            
        
    }
}
