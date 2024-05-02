using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Villager : MonoBehaviour, ISelectable
{
    private NavMeshAgent agent;
    public TownCenter townCenter;
    public bool selected;

    [Header("Jobs")]
    public VillagerJob job;
    public float workSpeed = 1.0f;  // Speed of resource extraction
    public int[] resources = new int[(int)ResourceType.MAX_VALUE]; // Amount of resources being carried
    public int totalResources = 0; //Total number of resources across all types
    public int capacity = 3;  // Maximum amount of resources it can carry
    public Resource targetResource;
    public Building targetBuilding;

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
            //Debug.Log("Finding New Destination"); // i removed this for my sanity -abby
            FindNewDestination();
        }
        else if (!agent.pathPending && agent.remainingDistance <= 0.5f)
        {
            // If close to current target, you're no longer walking
            walking = false;
            if (working)
            {
                if (job == VillagerJob.Builder)
                {
                    if (targetBuilding == null) working = false;
                    else
                    {
                        if (targetBuilding.Progress(workSpeed * Time.deltaTime))
                        {
                            returning = true;
                            working = false;
                            targetBuilding.assignedVillager = null;
                            targetBuilding = null;
                            SetDestination(townCenter.transform.position);
                        }
                    }
                }
                else
                {
                    if (targetResource == null) working = false;
                    else
                    {
                        ResourceType result = targetResource.Harvest(workSpeed * Time.deltaTime);
                        if (result != ResourceType.None)
                        {
                            resources[(int)result]++;
                            totalResources += 1;
                            if (totalResources >= capacity)
                            {
                                returning = true;
                                working = false;
                                targetResource.assignedVillager = null;
                                targetResource = null;
                                SetDestination(townCenter.transform.position);
                            }
                        }
                    }
                }
            }
            else if (returning)
            {
                townCenter.DepositResources(this, resources);
                Array.Clear(resources, 0, resources.Length);
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
        else if (job == VillagerJob.Builder)
        {
            List<Building> buildings = BuildingGenerator.GetPendingBuildings();
            float lowestDistance = float.MaxValue;
            foreach (Building building in buildings)
            {
                if (buildings != null)
                {
                    distance = Vector3.Distance(transform.position, building.transform.position);
                    if (distance < lowestDistance && building.assignedVillager == null)
                    {
                        lowestDistance = distance;
                        targetBuilding = building;
                    }
                }
            }
            if (targetBuilding != null)
            {
                targetBuilding.assignedVillager = this;
                SetDestination(targetBuilding.transform.position);
                working = true;
            }
            else
            {
                returning = true;
                SetDestination(townCenter.transform.position);
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
                    if (distance < lowestDistance && tree.assignedVillager == null)
                    {
                        lowestDistance = distance;
                        targetResource = tree;
                    }
                }
            }
            if (targetResource != null)
            {
                targetResource.assignedVillager = this;
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
        GameObject HUD = UnitHUD.HUD.AddUnitHUD(gameObject, UnitHUD.HUD.dummyHUD, 1f);
        HUD.GetComponent<DisplayController>().villager = this;
    }

    public void OnDeselect()
    {
        selected = false;
        UnitHUD.HUD.RemoveUnitHUD(gameObject);
    }
}
