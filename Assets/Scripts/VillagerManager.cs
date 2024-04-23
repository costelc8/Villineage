using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VillagerManager : MonoBehaviour
{
    private Dictionary<Resource, int> resourceWeights = new Dictionary<Resource, int>();
    private Dictionary<VillagerJob, int> villagerJobs = new Dictionary<VillagerJob, int>();
    public int totalVilagers;
    public int foodWeight;
    public int woodWeight;

    [Header("Editor Tools")]
    public bool updateVillagerJobs;

    // Re-distributes villager jobs
    public void UpdateVillagerJobs()
    {
        if (totalVilagers <= 0)
        {
            Debug.LogWarning("No villagers to update jobs");
            return;
        }

        CalculateResourceWeights();
        villagerJobs.Clear();
        int totalWeight = 0;
        foreach (int i in resourceWeights.Values) totalWeight += i;
        int villagerWeight = Mathf.CeilToInt((float)totalWeight / totalVilagers);

        for (int i = totalVilagers; i > 0; i--)
        {
            Resource mostNeeded = MostNeededResource();
            resourceWeights[mostNeeded] -= villagerWeight;
            VillagerJob job = GetJobForResource(mostNeeded);
            if (villagerJobs.ContainsKey(job)) villagerJobs[job] += 1;
            else villagerJobs.Add(job, 1);
        }
    }

    // Placeholder until we have more complex formulas to determine how much of each resource is desired
    public void CalculateResourceWeights()
    {
        resourceWeights.Clear();
        resourceWeights.Add(Resource.Food, foodWeight);
        resourceWeights.Add(Resource.Wood, woodWeight);
    }

    // This method returns whichever Resource in the resourceWeights dictionary currently has the largest weight
    private Resource MostNeededResource()
    {
        Resource resource = Resource.None;
        float maxWeight = float.MinValue;
        foreach (Resource r in resourceWeights.Keys)
        {
            if (resourceWeights[r] > maxWeight)
            {
                resource = r;
                maxWeight = resourceWeights[r];
            }
        }
        return resource;
    }

    // This method exists for the future, where there will be multiple methods of gathering a resource
    // Eg. Hunting, Gathering, Farming are all jobs that can gather food, and this method
    // will allow us to decide how to choose between those different food roles
    private VillagerJob GetJobForResource(Resource resource)
    {
        switch (resource)
        {
            case Resource.Food: return VillagerJob.Gatherer;
            case Resource.Wood: return VillagerJob.Lumberjack;
        }
        return VillagerJob.Nitwit;
    }

    // Editor-only method for displaying resource distribution
    private void OnValidate()
    {
        if (updateVillagerJobs)
        {
            updateVillagerJobs = false;
            UpdateVillagerJobs();
            foreach (VillagerJob job in villagerJobs.Keys)
            {
                Debug.Log(job.ToString() + ": " + villagerJobs[job]);
            }
        }
    }
}

public enum Resource
{
    None,
    Food,
    Wood
}

// "Nitwit" is the placeholder name for "villager without a job"
// Will likely go unused
public enum VillagerJob
{
    Nitwit,
    Gatherer,
    Lumberjack
}
