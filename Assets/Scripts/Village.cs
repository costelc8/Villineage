using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Village : MonoBehaviour
{
    private Dictionary<ResourceType, int> resources = new Dictionary<ResourceType, int>();
    public GameObject villagerPrefab;
    public List<Villager> villagers;

    // Start is called before the first frame update
    void Start()
    {
        // Spawn 5 villagers for testing selection
        for (int i = 0; i < 5; i++) SpawnVillager();
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
            }
            else
            {
                // failed
            }
        }
    }

    public void SpawnVillager()
    {
        Vector2 offset = Random.insideUnitCircle.normalized * Random.Range(5, 10);
        GameObject villager = Instantiate(villagerPrefab, new Vector3(transform.position.x + offset.x, 1, transform.position.y + offset.y), Quaternion.identity);
        villagers.Add(villager.GetComponent<Villager>());
        Selection.Selector.AddSelectable(villager.GetComponent<ISelectable>());
    }
}
