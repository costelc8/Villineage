using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TownCenter : MonoBehaviour
{
    public GameObject villagerPrefab;
    public int startingVillagers = 5;
    public List<Villager> villagers;
    private BuildingGenerator buildingGenerator;

    private Dictionary<ResourceType, int> resources = new Dictionary<ResourceType, int>();
    private Dictionary<ResourceType, float> resourceWeights = new Dictionary<ResourceType, float>();
    private Dictionary<VillagerJob, int> villagerJobs = new Dictionary<VillagerJob, int>();
    private Dictionary<VillagerJob, int> neededJobs = new Dictionary<VillagerJob, int>();

    public int foodWeight;
    public int woodWeight;

    [Header("Editor Tools")]
    public bool updateVillagerJobs;
    public ResourceType resourceQuery = ResourceType.None;
    public int resourceQuantity = 0;

    private void Start()
    {
        buildingGenerator = GetComponent<BuildingGenerator>();
        for (int i = 0; i < startingVillagers; i++) SpawnVillager();
        CalculateNeededJobs();
        AssignAllVillagerJobs();
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
        if (RandomNavmeshPoint.RandomPointFromCenterCapsule(transform.position, 0.5f, 2f, out Vector3 position, 4f, 1f, 1000f))
        {
            Villager villager = Instantiate(villagerPrefab, position, Quaternion.identity).GetComponent<Villager>();
            villagers.Add(villager);
            villager.townCenter = this;
            Selection.Selector.AddSelectable(villager);
        }
        Physics.SyncTransforms();
    }

    public void DepositResources(Villager villager, Dictionary<ResourceType, int> deposit)
    {
        foreach(ResourceType resource in deposit.Keys)
        {
            if (resources.ContainsKey(resource)) resources[resource] += deposit[resource];
            else resources.Add(resource, deposit[resource]);
        }
        if (resources[ResourceType.Wood] >= 50)
        {
            buildingGenerator.PlaceHouse();
            resources[ResourceType.Wood] -= 50;
        }
        AssignVillagerJob(villager);
    }

    // Re-distributes villager jobs
    public void CalculateNeededJobs()
    {
        if (villagers.Count <= 0)
        {
            Debug.LogWarning("No villagers to update jobs");
            return;
        }

        CalculateResourceWeights();
        neededJobs.Clear();
        int totalWeight = 0;
        foreach (int i in resourceWeights.Values) totalWeight += i;
        float villagerWeight = (float)totalWeight / villagers.Count;

        for (int i = villagers.Count; i > 0; i--)
        {
            ResourceType mostNeeded = MostNeededResource();
            if (mostNeeded != ResourceType.None) resourceWeights[mostNeeded] -= villagerWeight;
            VillagerJob job = GetJobForResource(mostNeeded);
            if (neededJobs.ContainsKey(job)) neededJobs[job]++;
            else neededJobs.Add(job, 1);
        }
    }

    // Placeholder until we have more complex formulas to determine how much of each resource is desired
    public void CalculateResourceWeights()
    {
        resourceWeights.Clear();
        resourceWeights.Add(ResourceType.Food, foodWeight);
        resourceWeights.Add(ResourceType.Wood, woodWeight);
        resourceWeights.Add(ResourceType.Building, BuildingGenerator.GetPendingBuildings().Count);
    }

    // This method returns whichever ResourceType in the resourceWeights dictionary currently has the largest weight
    private ResourceType MostNeededResource()
    {
        ResourceType resource = ResourceType.None;
        float maxWeight = 0;
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

    private void AssignVillagerJob(Villager villager)
    {
        CalculateNeededJobs();
        VillagerJob job = MostNeededJob();
        if (job != VillagerJob.Nitwit)
        {
            villagerJobs[villager.job]--;
            villager.job = job;
            if (villagerJobs.ContainsKey(job)) villagerJobs[job]++;
            else villagerJobs.Add(job, 1);
        }
    }

    private void AssignAllVillagerJobs()
    {
        villagerJobs.Clear();
        foreach(Villager v in villagers)
        {
            v.job = MostNeededJob();
            if (villagerJobs.ContainsKey(v.job)) villagerJobs[v.job]++;
            else villagerJobs.Add(v.job, 1);
        }
    }

    private VillagerJob MostNeededJob()
    {
        VillagerJob job = VillagerJob.Nitwit;
        int maxNeed = 0;
        foreach (VillagerJob j in neededJobs.Keys)
        {
            int needed = villagerJobs.ContainsKey(j) ? neededJobs[j] - villagerJobs[j] : neededJobs[j];
            if (needed > maxNeed)
            {
                maxNeed = needed;
                job = j;
            }
        }
        return job;
    }

    // This method exists for the future, where there will be multiple methods of gathering a resource
    // Eg. Hunting, Gathering, Farming are all jobs that can gather food, and this method
    // will allow us to decide how to choose between those different food roles
    private VillagerJob GetJobForResource(ResourceType resource)
    {
        switch (resource)
        {
            case ResourceType.Building: return VillagerJob.Builder;
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
            CalculateNeededJobs();
            foreach (VillagerJob job in neededJobs.Keys)
            {
                Debug.Log(job.ToString() + ": " + neededJobs[job]);
            }
        }
    }
}

public enum ResourceType
{
    None,
    Building,
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
