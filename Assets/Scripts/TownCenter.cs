using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Unity.Jobs.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.AI;

public class TownCenter : MonoBehaviour
{
    public GameObject villagerPrefab;
    public int startingVillagers = 5;
    public List<Villager> villagers;
    private BuildingGenerator buildingGenerator;

    //private Dictionary<ResourceType, int> resources = new Dictionary<ResourceType, int>();
    //private Dictionary<ResourceType, float> resourceWeights = new Dictionary<ResourceType, float>();
    //private Dictionary<VillagerJob, int> villagerJobs = new Dictionary<VillagerJob, int>();
    //private Dictionary<VillagerJob, int> neededJobs = new Dictionary<VillagerJob, int>();
    private int[] resources = new int[(int)ResourceType.MAX_VALUE];
    private float[] jobWeights = new float[(int)VillagerJob.MAX_VALUE];
    private int[] neededJobs = new int[(int)VillagerJob.MAX_VALUE];
    private int[] currentJobs = new int[(int)VillagerJob.MAX_VALUE];

    public int foodWeight;
    public int woodWeight;

    [Header("Editor Tools")]
    public bool updateVillagerJobs;
    public ResourceType resourceQuery = ResourceType.None;
    public int resourceQuantity = 0;

    private void Start()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hitInfo, 100f, LayerMask.GetMask("Ground")))
        {
            transform.position = hitInfo.point;
            Camera.main.transform.parent.parent.position = transform.position;
        }
        buildingGenerator = GetComponent<BuildingGenerator>();
        for (int i = 0; i < startingVillagers; i++) SpawnVillager();
        AssignAllVillagerJobs();
    }

    private void Update()
    {
        resourceQuantity = resources[(int)resourceQuery];
    }

    public void SpawnVillager()
    {
        if (RandomNavmeshPoint.RandomPointFromCenterCapsule(transform.position, 0.5f, 2f, out Vector3 position, 4f, 1f, 1000f))
        {
            Villager villager = Instantiate(villagerPrefab, position, Quaternion.identity).GetComponent<Villager>();
            villagers.Add(villager);
            villager.townCenter = this;
            Selection.Selector.AddSelectable(villager);
        }
        Physics.SyncTransforms();
    }

    public void DepositResources(Villager villager, int[] deposit)
    {
        for (int i = 0; i < (int)ResourceType.MAX_VALUE; i++)
        {
            resources[i] += deposit[i];
        }
        if (resources[(int)ResourceType.Wood] >= 50)
        {
            buildingGenerator.PlaceHouse();
            resources[(int)ResourceType.Wood] -= 50;
        }
        AssignVillagerJob(villager);
    }

    private void AssignAllVillagerJobs()
    {
        CalculateNeededJobs();
        Array.Clear(currentJobs, 0, currentJobs.Length);
        foreach (Villager villager in villagers)
        {
            villager.job = GetMostNeededJob();
            currentJobs[(int)villager.job]++;
        }
    }

    private void AssignVillagerJob(Villager villager)
    {
        CalculateNeededJobs();
        VillagerJob job = GetMostNeededJob();
        if (job != VillagerJob.Nitwit)
        {
            currentJobs[(int)villager.job]--;
            villager.job = job;
            currentJobs[(int)villager.job]++;
        }
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
    }

    // Placeholder until we have more complex formulas to determine how much of each resource is desired
    private void CalculateJobWeights()
    {
        jobWeights[(int)VillagerJob.Gatherer] = foodWeight;
        jobWeights[(int)VillagerJob.Lumberjack] = woodWeight;
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

    //// This method returns whichever ResourceType in the resourceWeights dictionary currently has the largest weight
    //private ResourceType MostNeededResource()
    //{
    //    ResourceType resource = ResourceType.None;
    //    float maxWeight = 0;
    //    foreach (ResourceType r in resourceWeights.Keys)
    //    {
    //        if (resourceWeights[r] > maxWeight)
    //        {
    //            resource = r;
    //            maxWeight = resourceWeights[r];
    //        }
    //    }
    //    return resource;
    //}

    

    //private void AssignAllVillagerJobs()
    //{
    //    villagerJobs.Clear();
    //    foreach(Villager v in villagers)
    //    {
    //        v.job = MostNeededJob();
    //        if (villagerJobs.ContainsKey(v.job)) villagerJobs[v.job]++;
    //        else villagerJobs.Add(v.job, 1);
    //    }
    //}

    //private VillagerJob MostNeededJob()
    //{
    //    VillagerJob job = VillagerJob.Nitwit;
    //    int maxNeed = 0;
    //    foreach (VillagerJob j in neededJobs.Keys)
    //    {
    //        int needed = villagerJobs.ContainsKey(j) ? neededJobs[j] - villagerJobs[j] : neededJobs[j];
    //        if (needed > maxNeed)
    //        {
    //            maxNeed = needed;
    //            job = j;
    //        }
    //    }
    //    return job;
    //}

    //// This method exists for the future, where there will be multiple methods of gathering a resource
    //// Eg. Hunting, Gathering, Farming are all jobs that can gather food, and this method
    //// will allow us to decide how to choose between those different food roles
    //private VillagerJob GetJobForResource(ResourceType resource)
    //{
    //    switch (resource)
    //    {
    //        case ResourceType.Building: return VillagerJob.Builder;
    //        case ResourceType.Food: return VillagerJob.Gatherer;
    //        case ResourceType.Wood: return VillagerJob.Lumberjack;
    //    }
    //    return VillagerJob.Nitwit;
    //}

    //// Editor-only method for displaying resource distribution
    //private void OnValidate()
    //{
    //    if (updateVillagerJobs)
    //    {
    //        updateVillagerJobs = false;
    //        CalculateNeededJobs();
    //        foreach (VillagerJob job in neededJobs.Keys)
    //        {
    //            Debug.Log(job.ToString() + ": " + neededJobs[job]);
    //        }
    //    }
    //}
}

public enum ResourceType
{
    None,
    Food,
    Wood,
    MAX_VALUE,
}

// "Nitwit" is the placeholder name for "villager without a job"
// Will likely go unused
public enum VillagerJob
{
    Nitwit,
    Gatherer,
    Lumberjack,
    Builder,
    MAX_VALUE,
}
