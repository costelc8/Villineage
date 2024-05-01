using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TownCenter : MonoBehaviour
{
    public GameObject villagerPrefab;
    public int startingVillagers = 5;
    public List<Villager> villagers;

    private Dictionary<ResourceType, int> resources = new Dictionary<ResourceType, int>();
    private Dictionary<ResourceType, int> resourceWeights = new Dictionary<ResourceType, int>();
    private Dictionary<VillagerJob, int> villagerJobs = new Dictionary<VillagerJob, int>();

    public int totalVilagers;
    public int foodWeight;
    public int woodWeight;

    [Header("Editor Tools")]
    public bool updateVillagerJobs;
    public ResourceType resourceQuery = ResourceType.None;
    public int resourceQuantity = 0;

    private void Start()
    {
        for (int i = 0; i < startingVillagers; i++) SpawnVillager();
    }

    private void Update()
    {
        if (resourceQuery != ResourceType.None)
        {
            if (!resources.TryGetValue(resourceQuery, out resourceQuantity)) resourceQuantity = 0;
        }
    }

    public void SpawnVillager()
    {
        if (RandomNavmeshPoint.RandomPointFromCenterCapsule(transform.position, 0.5f, 2f, out Vector3 position, 4f, 1f, 100f))
        {
            Villager villager = Instantiate(villagerPrefab, position, Quaternion.identity).GetComponent<Villager>();
            villagers.Add(villager);
            villager.townCenter = this;
            Selection.Selector.AddSelectable(villager);
        }
        Physics.SyncTransforms();
    }

    public void DepositResources(Dictionary<ResourceType, int> deposit)
    {
        foreach(ResourceType resource in deposit.Keys)
        {
            if (resources.ContainsKey(resource)) resources[resource] += deposit[resource];
            else resources.Add(resource, deposit[resource]);
        }
    }

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
            ResourceType mostNeeded = MostNeededResource();
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
        resourceWeights.Add(ResourceType.Food, foodWeight);
        resourceWeights.Add(ResourceType.Wood, woodWeight);
    }

    // This method returns whichever ResourceType in the resourceWeights dictionary currently has the largest weight
    private ResourceType MostNeededResource()
    {
        ResourceType resource = ResourceType.None;
        float maxWeight = float.MinValue;
        foreach (ResourceType r in resourceWeights.Keys)
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
    private VillagerJob GetJobForResource(ResourceType resource)
    {
        switch (resource)
        {
            case ResourceType.Food: return VillagerJob.Gatherer;
            case ResourceType.Wood: return VillagerJob.Lumberjack;
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

public enum ResourceType
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
    Lumberjack,
    Builder
}
