using Mirror;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;


public class TownCenter : NetworkBehaviour
{
    public static TownCenter TC;
    private Storage storage;
    public GameObject villagerPrefab;
    public List<Villager> villagers;
    public BuildingGenerator buildingGenerator;
    public GameObject villagerParent;
    public GameObject deadVillagerParent;

    public bool spawning = false;
    public float timer = 0.0f;

    public float[] jobWeights;
    public int[] neededJobs;
    public int[] currentJobs;

    public void Initialize()
    {
        if (TC == null || TC == this) TC = this;
        else Destroy(this);
        buildingGenerator = GetComponent<BuildingGenerator>();
        storage = GetComponent<Storage>();
        storage.Initialize();
        BuildingGenerator.AddHub(storage);
        villagerParent = new GameObject("Villagers");
        deadVillagerParent = new GameObject("Dead");
        jobWeights = new float[(int)VillagerJob.MAX_VALUE];
        neededJobs = new int[(int)VillagerJob.MAX_VALUE];
        currentJobs = new int[(int)VillagerJob.MAX_VALUE];
    }

    public void PlaceOnGround()
    {
        Debug.Log("Placing Town Center");
        transform.position = new Vector3(SimVars.VARS.terrainSize / 2, SimVars.VARS.terrainDepth, SimVars.VARS.terrainSize / 2);
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hitInfo, 100f, LayerMask.GetMask("Ground")))
        {
            transform.position = hitInfo.point;
        }
        CenterCamera();
        Physics.SyncTransforms();
    }

    public void CenterCamera()
    {
        Vector3 pos = transform.position;
        pos.y = SimVars.VARS.terrainDepth / 2;
        Camera.main.transform.parent.parent.position = pos;
    }

    public void SpawnVillagers(int villagerCount)
    {
        Debug.Log("Spawning Villagers");
        for (int i = 0; i < villagerCount; i++) SpawnVillager(transform.position);
        AssignAllVillagerJobs();
    }

    public void VillagerSpawnCheck()
    {
        bool enoughFood = storage.resources[(int)ResourceType.Food] >= 10 * villagers.Count;
        bool enoughHouses = villagers.Count < SimVars.VARS.startingVillagers + BuildingGenerator.GetHouses().Count;
        if (!spawning && enoughFood && enoughHouses)
        {
            //print(SimVars.VARS.startingVillagers + BuildingGenerator.GetHouses().Count);
            if (storage.resources[(int)ResourceType.Food] >= SimVars.VARS.villagerSpawnCost)
            {
                //print("Begin Spawning");
                storage.resources[(int)ResourceType.Food] -= SimVars.VARS.villagerSpawnCost;
                spawning = true;
            }
        }
    }

    public void SpawnVillager(Vector3 centerPosition, bool assignJob = false)
    {
        //print("Spawning Villager");
        if (RandomNavmeshPoint.RandomPointFromCenterCapsule(centerPosition, 0.5f, 2f, out Vector3 position, 4f, 1f, 1000f))
        {
            Villager villager = Instantiate(villagerPrefab, position, Quaternion.identity, villagerParent.transform).GetComponent<Villager>();
            NetworkServer.Spawn(villager.gameObject);
            villagers.Add(villager);
            villager.GetComponent<NavMeshAgent>().enabled = true;
            villager.GetComponent<NavMeshAgent>().avoidancePriority = Random.Range(20, 50);
            currentJobs[(int)VillagerJob.Nitwit]++;
            if (assignJob) AssignVillagerJob(villager);
        }
        Physics.SyncTransforms();
    }

    public void HouseSpawnCheck(Storage storage)
    {
        if (!storage.hasHouseInProgress && BuildingGenerator.GetHouses().Count < villagers.Count + BuildingGenerator.GetHubs().Count)
        {
            if (storage.resources[(int)ResourceType.Wood] >= SimVars.VARS.houseBuildCost)
            {
                buildingGenerator.PlaceBuilding(BuildingType.House, storage.transform.position, storage);
            }
        }
    }

    private void AssignAllVillagerJobs()
    {
        CalculateNeededJobs();
        Array.Clear(currentJobs, 0, currentJobs.Length);
        foreach (Villager villager in villagers)
        {
            villager.ChangeJob(GetMostNeededJob());
            currentJobs[(int)villager.job]++;
            CalculateNeededJobs();
        }
    }

    public void AssignVillagerJob(Villager villager)
    {
        CalculateNeededJobs();
        if (neededJobs[(int)villager.job] >= 0) return;
        VillagerJob job = GetMostNeededJob();
        // Debug.Log("Changing from " + villager.job + "(" + neededJobs[(int)villager.job] + ") to " + job + "(" + neededJobs[(int)job] + ")");
        if (job != VillagerJob.Nitwit) ChangeVillagerJob(villager, job);
        CalculateNeededJobs();
    }

    public void ChangeVillagerJob(Villager villager, VillagerJob job)
    {
        currentJobs[(int)villager.job]--;
        villager.ChangeJob(job);
        currentJobs[(int)villager.job]++;
    }

    private VillagerJob GetMostNeededJob()
    {
        VillagerJob job = VillagerJob.Nitwit;
        int maxNeed = 0;
        for (int i = 0; i < (int)VillagerJob.MAX_VALUE; i++)
        {
            if (neededJobs[i] > maxNeed)
            {
                maxNeed = neededJobs[i];
                job = (VillagerJob)i;
            }
        }
        return job;
    }

    // Re-distributes villager jobs
    private void CalculateNeededJobs()
    {
        if (villagers.Count <= 0)
        {
            Debug.LogWarning("No villagers to update jobs");
            return;
        }

        CalculateJobWeights();
        float totalWeight = 0;
        for (int i = 0; i < (int)VillagerJob.MAX_VALUE; i++)
        {
            neededJobs[i] = 0;
            totalWeight += jobWeights[i];
        }
        float villagerWeight = totalWeight / villagers.Count;

        foreach (Villager villager in villagers)
        {
            VillagerJob highestWeight = HighestWeightJob();
            jobWeights[(int)highestWeight] -= villagerWeight;
            neededJobs[(int)highestWeight]++;
            neededJobs[(int)villager.job]--;
        }
        CalculateJobWeights();
    }

    // Placeholder until we have more complex formulas to determine how much of each resource is desired
    private void CalculateJobWeights()
    {
        float woodWeight, foodWeight, averageWeight;
        if (storage.resources.Count > 0)
        {
            woodWeight = 1000f / (1000f + storage.resources[(int)ResourceType.Wood]);
            foodWeight = 1000f / (1000f + storage.resources[(int)ResourceType.Food]);
            averageWeight = (woodWeight + foodWeight) / 2f;
        }
        else
        {
            woodWeight = 1;
            foodWeight = 1;
            averageWeight = 1;
        }
        
        bool treesExist = ResourceGenerator.GetTrees().Count > 0;
        bool berriesExist = ResourceGenerator.GetBerries().Count > 0;
        bool animalsExist = ResourceGenerator.GetAnimals().Count > 0;
        bool buildingsExist = BuildingGenerator.GetPendingBuildings().Count > 0;
        foodWeight *= (1 + (berriesExist ? 0 : 1) + (animalsExist ? 0 : 1));
        jobWeights[(int)VillagerJob.Hunter] = animalsExist ? SimVars.VARS.hunterWeight * foodWeight : 0;
        jobWeights[(int)VillagerJob.Gatherer] = berriesExist ? SimVars.VARS.gathererWeight * foodWeight : 0;
        jobWeights[(int)VillagerJob.Lumberjack] = treesExist ? SimVars.VARS.lumberjackWeight * woodWeight : 0;
        jobWeights[(int)VillagerJob.Builder] = buildingsExist ? SimVars.VARS.builderWeight * averageWeight : 0;
    }

    private VillagerJob HighestWeightJob()
    {
        VillagerJob job = VillagerJob.Nitwit;
        float maxWeight = 0;
        for (int i = 0; i < (int)VillagerJob.MAX_VALUE; i++)
        {
            if (jobWeights[i] > maxWeight)
            {
                maxWeight = jobWeights[i];
                job = (VillagerJob)i;
            }
        }
        return job;
    }

    public void RemoveVillager(Villager villager)
    {
        currentJobs[(int)villager.job]--;
        villagers.Remove(villager);
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        CenterCamera();
    }

    public void Update()
    {
        if (spawning)
        {
            timer += Time.deltaTime;
            if (timer > SimVars.VARS.villagerSpawnTime * 2 / (1 + BuildingGenerator.GetHubs().Count))
            {
                spawning = false;
                timer = 0.0f;
                SpawnVillager(this.transform.position, true);
            }
        }
    }
}

public enum ResourceType
{
    Wood,
    Food,
    MAX_VALUE,
}

// "Nitwit" is the placeholder name for "villager without a job"
// Will likely go unused
public enum VillagerJob
{
    Nitwit,
    Hunter,
    Gatherer,
    Lumberjack,
    Builder,
    MAX_VALUE,
}

public enum BuildingType
{
    House,
    Outpost,
    MAX_VALUE,
}

public enum ResourceSourceType
{
    None,
    Tree,
    Berry,
    Sheep,
    Goat,
    Wolf,
    MAX_VALUE,
}
