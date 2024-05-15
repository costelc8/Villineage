using Mirror;
using System;
using System.Collections.Generic;
using UnityEngine;


public class TownCenter : NetworkBehaviour
{
    public GameObject villagerPrefab;
    public List<Villager> villagers;
    //public TerrainGenerator terrainGenerator;


    public float[] jobWeights;
    public int[] neededJobs;
    public int[] currentJobs;

    //public int lumberjackWeight;
    //public int gathererWeight;
    //public int hunterWeight;

    private bool initialized;

    private void Awake()
    {
        if (!initialized) Initialize();
    }

    private void Initialize()
    {

        jobWeights = new float[(int)VillagerJob.MAX_VALUE];
        neededJobs = new int[(int)VillagerJob.MAX_VALUE];
        currentJobs = new int[(int)VillagerJob.MAX_VALUE];
        initialized = true;
    }

    public void PlaceOnGround()
    {
        if (!initialized) Initialize();
        Debug.Log("Placing Town Center");
        transform.position = new Vector3(SimVars.VARS.terrainSize / 2, SimVars.VARS.terrainDepth, SimVars.VARS.terrainSize / 2);
        Physics.SyncTransforms();
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hitInfo, 100f, LayerMask.GetMask("Ground")))
        {
            transform.position = hitInfo.point;
        }
        CenterCamera();
    }

    public void CenterCamera()
    {
        Camera.main.transform.parent.parent.position = transform.position;
    }

    public void SpawnVillagers(int villagerCount)
    {
        if (!initialized) Initialize();
        Debug.Log("Spawning Villagers");
        for (int i = 0; i < villagerCount; i++) SpawnVillager();
        AssignAllVillagerJobs();
    }

    public void SpawnVillager()
    {
        if (RandomNavmeshPoint.RandomPointFromCenterCapsule(transform.position, 0.5f, 2f, out Vector3 position, 4f, 1f, 1000f))
        {
            Villager villager = Instantiate(villagerPrefab, position, Quaternion.identity).GetComponent<Villager>();
            NetworkServer.Spawn(villager.gameObject);
            villagers.Add(villager);
            villager.townCenter = this;
        }
        Physics.SyncTransforms();
    }

    public void DepositResources(Villager villager, int[] deposit)
    {
        
        AssignVillagerJob(villager);
    }

    private void AssignAllVillagerJobs()
    {
        CalculateNeededJobs();
        Array.Clear(currentJobs, 0, currentJobs.Length);
        foreach (Villager villager in villagers)
        {
            villager.ChangeJob(GetMostNeededJob());
            currentJobs[(int)villager.Job()]++;
            CalculateNeededJobs();
        }
        CalculateJobWeights();
    }

    private void AssignVillagerJob(Villager villager)
    {
        CalculateNeededJobs();
        if (neededJobs[(int)villager.job] >= 0) return;
        VillagerJob job = GetMostNeededJob();
        if (job != VillagerJob.Nitwit)
        {
            currentJobs[(int)villager.Job()]--;
            villager.ChangeJob(job);
            currentJobs[(int)villager.Job()]++;
        }
        CalculateJobWeights();
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
            neededJobs[(int)villager.Job()]--;
        }
    }

    // Placeholder until we have more complex formulas to determine how much of each resource is desired
    private void CalculateJobWeights()
    {
        jobWeights[(int)VillagerJob.Lumberjack] = SimVars.VARS.lumberjackWeight;
        jobWeights[(int)VillagerJob.Gatherer] = SimVars.VARS.gathererWeight;
        jobWeights[(int)VillagerJob.Hunter] = SimVars.VARS.hunterWeight;
        jobWeights[(int)VillagerJob.Builder] = BuildingGenerator.GetPendingBuildings().Count;
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
        villagers.Remove(villager);
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        CenterCamera();
    }
}

public enum ResourceType
{
    None,
    Wood,
    Food,
    MAX_VALUE,
}

// "Nitwit" is the placeholder name for "villager without a job"
// Will likely go unused
public enum VillagerJob
{
    Nitwit,
    Builder,
    Lumberjack,
    Hunter,
    Gatherer,
    Farmer,
    MAX_VALUE,
}

public enum BuildingType
{
    House,
    Outpost,
}
