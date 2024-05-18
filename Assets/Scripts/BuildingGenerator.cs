using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingGenerator : MonoBehaviour
{
    public GameObject housePrefab;
    public GameObject outpostPrefab;
    public GameObject farmPrefab;
    private Vector3 spacingSizeSmall;
    private Vector3 spacingSizeLarge;
    private int spacing = 2;
    private static List<Targetable> pendingBuildings = new List<Targetable>();
    private static List<Building> houses = new List<Building>();
    private static List<Building> hubs = new List<Building>();
    private GameObject buildingParent;
  

    // Start is called before the first frame update
    private void Awake()
    {
        spacingSizeSmall = housePrefab.GetComponent<BoxCollider>().size / 2 + new Vector3(spacing, spacing, spacing);
        buildingParent = new GameObject("Buildings");
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            PlaceBuilding(BuildingType.Outpost);
        }
    }

    public bool PlaceBuilding(BuildingType buildingType, Vector3 center = new Vector3())
    {
        float starveRange = SimVars.VARS.GetMaxVillagerRange();
        GameObject buildingPrefab = null;
        Vector3 spawnCenter;
        if (center ==  Vector3.zero)
        {
            // spawn @ current position
            spawnCenter = transform.position;
        } else
        {
            // spawn @ given position
            spawnCenter = center;
        }
        Vector3 point = new Vector3();
        bool gotPoint = false;
        Quaternion rotation = Quaternion.Euler(0, 90 * Random.Range(0, 4), 0);
        int buildCost = 0;
        int priority = 0;

        switch (buildingType)
        {
            case BuildingType.House:
                buildingPrefab = housePrefab;
                buildCost = SimVars.VARS.houseBuildCost;
                gotPoint = RandomNavmeshPoint.RandomPointFromCenterBox(spawnCenter, spacingSizeSmall, out point, 8f, 3f, starveRange);
                priority = 1;
                break;
            case BuildingType.Outpost:
                buildingPrefab = outpostPrefab;
                buildCost = SimVars.VARS.outpostBuildCost;
                gotPoint = RandomNavmeshPoint.RandomPointFromCenterBox(spawnCenter, spacingSizeSmall, out point, 3f, 3f, starveRange);
                priority = 100;
                break;
        }

        if (buildingPrefab != null && gotPoint) 
        {
            // Make the building
            Building building = Instantiate(buildingPrefab, point, Quaternion.identity * rotation, buildingParent.transform).GetComponent<Building>();
            building.maxBuildProgress = buildCost;
            building.buildingType = buildingType;
            building.priority = priority;
            NetworkServer.Spawn(building.gameObject);
            pendingBuildings.Add(building);
            return true;

        }
        return false;
    }

    public static List<Targetable> GetPendingBuildings()
    {
        return pendingBuildings;
    }

    public static List<Building> GetHouses()
    {
        return houses;
    }

    public static List<Building> GetHubs()
    {
        return hubs;
    }

    public static void AddBuilding(Building building)
    {
        if (pendingBuildings.Contains(building))
        {
            pendingBuildings.Remove(building);
            houses.Add(building);
        }
    }

    public static void RemoveBuilding(Building building)
    {
        pendingBuildings.Remove(building);
        houses.Remove(building);
    }
}
