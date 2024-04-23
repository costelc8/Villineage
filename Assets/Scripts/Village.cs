using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Village : MonoBehaviour
{
    private Dictionary<Resource, int> resources = new Dictionary<Resource, int>();
    public GameObject villagerPrefab;
    public List<Villager> villagers;

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < 4; i++)
        {
            SpawnVillager();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SpawnVillager()
    {
        Vector2 offset = Random.insideUnitCircle.normalized * Random.Range(5, 10);
        GameObject villager = Instantiate(villagerPrefab, new Vector3(transform.position.x + offset.x, 1, transform.position.y + offset.y), Quaternion.identity);
        villagers.Add(villager.GetComponent<Villager>());
        Selection.Selector.AddSelectable(villager.GetComponent<ISelectable>());
    }
}
