using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.GraphicsBuffer;

public class Villager : MonoBehaviour, ISelectable
{
    public bool selected;
    public bool randomMovement = true;

    [Header("Jobs")]
    public VillagerJob job;
    public float workSpeed = 1.0f;  // Speed of resource extraction
    // public int wood = 0;  // Amount of wood being carried (Replaced by `resources` below)
    public Dictionary<ResourceType, int> resources; // Amount of resources being carried, instead of having a separate variable for each resource
    public int capacity = 10;  // Maximum amount of resources it can carry

    /// <summary>
    /// You can check capacity by iterating through ResourceTypes, eg.
    /// int totalResources = 0;
    /// foreach(ResourceType resource in resources.Keys) {
    ///     totalResources += resources[resource];
    /// }
    /// if (totalResources >= capacity) {
    ///     full = true;
    /// }
    /// Or something of the sort
    /// </summary>

    [Header("Movement")]
    public float selectionRange = 10.0f;  // Selection range for random movement
    private float distance;
    public Vector3 target;  // Current navigation target

    [Header("States")]
    public bool working = false;  // Are they currently working on something?
    public bool wandering = false;  // Are they wandering around?
    public bool full = false;  // Is wood >= capacity?
    private NavMeshAgent agent;

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        // If not wandering or working, find a new destination
        if (!wandering && !working) {
            FindNewDestination();
        }

        // If close to current target and not working, you're no longer wandering
        if (agent.remainingDistance <= 0.2f && !working) {
            wandering = false;
        }
    }

    public void OnSelect()
    {
        Debug.Log(name + " Selected");
        foreach(Transform t in transform) t.gameObject.SetActive(true);
        selected = true;
    }

    public void OnDeselect()
    {
        Debug.Log(name + " Deselected");
        foreach (Transform t in transform) t.gameObject.SetActive(false);
        selected = false;
    }

    public void FindNewDestination() {
        // Use the Random Point fn to find a new target position
        if (randomMovement) {
            if (RandomNavmeshPoint.RandomPoint(Vector3.zero, selectionRange, out target)) {
                agent.destination = target;
                wandering = true;
            }
        //Otherwise, look to the list of resources to gather
        } else {
            if (job == VillagerJob.Lumberjack) {
                List<GameObject> targets = ResourceGenerator.Resources.GetAllTrees();
                float lowestDistance = float.MaxValue;
                foreach (GameObject canidate in targets)
                {
                    distance = Vector3.Distance(transform.position, canidate.transform.position);
                    if (distance < lowestDistance)
                    {
                        lowestDistance = distance;
                        target = canidate.transform.position;
                    }
                }
                agent.destination = target;
                wandering = true;
                working = true;
            }
        }

    }
}
