using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.GraphicsBuffer;

[RequireComponent(typeof(NavMeshAgent))]
public class Villager : MonoBehaviour, ISelectable
{
    private NavMeshAgent agent;
    public TownCenter townCenter;
    public bool selected;

    [Header("Jobs")]
    public VillagerJob job;
    public float workSpeed = 1.0f;  // Speed of resource extraction
    public Dictionary<ResourceType, int> resources = new Dictionary<ResourceType, int>(); // Amount of resources being carried
    public int totalResources = 0; //Total number of resources across all types
    public int capacity = 3;  // Maximum amount of resources it can carry
    public Resource targetResource;

    [Header("Movement")]
    public float selectionRange = 10.0f;  // Selection range for random movement
    private float distance;

    [Header("States")]
    public bool working = false;  // Are they currently working on something?
    public bool walking = false;  // Are they wandering around?
    public bool returning = false;

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        // If not walking or working, find a new destination
        if (!walking && !working && !returning)
        {
            Debug.Log("Finding New Destination");
            FindNewDestination();
        }
        else if (!agent.pathPending && agent.remainingDistance <= 0.5f)
        {
            // If close to current target, you're no longer walking
            walking = false;
            if (working)
            {
                ResourceType result = targetResource.Harvest(workSpeed * Time.deltaTime);
                if (result != ResourceType.None)
                {
                    if (resources.ContainsKey(result)) resources[result] += 1;
                    else resources.Add(result, 1);
                    totalResources += 1;
                    Debug.Log("Resource Harvested");
                    if (totalResources >= capacity)
                    {
                        returning = true;
                        SetDestination(townCenter.transform.position);
                    }
                    working = false;
                    targetResource = null;
                }
            }
            else if (returning)
            {
                townCenter.DepositResources(resources);
                resources.Clear();
                totalResources = 0;
                returning = false;
            }
        }
    }

    public void FindNewDestination()
    {
        if (job == VillagerJob.Nitwit)
        {
            if (RandomNavmeshPoint.RandomPoint(transform.position, selectionRange, out Vector3 targetPosition))
            {
                SetDestination(targetPosition);
            }
        }
        else if (job == VillagerJob.Lumberjack)
        {
            List<Resource> trees = ResourceGenerator.GetTrees();
            float lowestDistance = float.MaxValue;
            foreach (Resource tree in trees)
            {
                if (tree != null)
                {
                    distance = Vector3.Distance(transform.position, tree.transform.position);
                    if (distance < lowestDistance && tree.selected < tree.selectionLimit)
                    {
                        lowestDistance = distance;
                        targetResource = tree;
                    }
                }
            }
            if (targetResource != null)
            {
                targetResource.selected += 1;
                SetDestination(targetResource.transform.position);
                working = true;
            }
            else
            {
                returning = true;
                SetDestination(townCenter.transform.position);
            }
        }
        walking = true;
    }

    private void SetDestination(Vector3 position)
    {
        agent.SetDestination(position + (transform.position - position).normalized * 0.1f);
    }

    public void OnSelect()
    {
        selected = true;
        UnitHUD.HUD.AddUnitHUD(gameObject, UnitHUD.HUD.dummyHUD, 1f);
    }

    public void OnDeselect()
    {
        selected = false;
        UnitHUD.HUD.RemoveUnitHUD(gameObject);
    }
}
