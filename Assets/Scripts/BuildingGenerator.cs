using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingGenerator : MonoBehaviour
{
    public List<GameObject> buildingPrefabs;
    private Vector3 spacingSizeSmall;
    private Vector3 spacingSizeLarge;
    private int spacing = 2;
    private static List<Targetable> pendingBuildings = new List<Targetable>();
    private static List<Building> buildings = new List<Building>();
    private static List<Targetable> hubs = new List<Targetable>();
    private GameObject buildingParent;

    // Start is called before the first frame update
    private void Awake()
    {
        spacingSizeSmall = buildingPrefabs[0].GetComponent<BoxCollider>().size / 2 + new Vector3(spacing, spacing, spacing);
        buildingParent = new GameObject("Buildings");
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            //PlaceHouse();
        }
    }

    public void PlaceBuilding(BuildingType building)
    {

        // Grab point on NavMesh
        if (RandomNavmeshPoint.RandomPointFromCenterBox(transform.position, spacingSizeSmall, out Vector3 point, 8f, 3f, 1000f))
        {
            // Make a building
            Quaternion rotation = Quaternion.Euler(0, 90 * Random.Range(0, 4), 0);
            Building house = Instantiate(buildingPrefabs[0], point, Quaternion.identity * rotation, buildingParent.transform).GetComponent<Building>();
            house.maxBuildTime = SimVars.VARS.houseBuildTime;
            NetworkServer.Spawn(house.gameObject);
            pendingBuildings.Add(house);
        }
    }

    public static List<Targetable> GetPendingBuildings()
    {
        return pendingBuildings;
    }

    public static List<Targetable> GetHubs()
    {
        return hubs;
    }

    public static void AddBuilding(Building building)
    {
        if (pendingBuildings.Contains(building))
        {
            pendingBuildings.Remove(building);
            buildings.Add(building);
        }
    }
}
