using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.GraphicsBuffer;

public class Villager : MonoBehaviour, ISelectable
{
    public bool selected;
    public bool randomMovement = false;

    [Header("Jobs")]
    public VillagerJob job;
    public float workSpeed = 1.0f;  // Speed of resource extraction
    // public int wood = 0;  // Amount of wood being carried (Replaced by `resources` below)
    public Dictionary<ResourceType, int> resources; // Amount of resources being carried, instead of having a separate variable for each resource
    public int totalResources = 0; //Total number of resources across all types
    public int capacity = 3;  // Maximum amount of resources it can carry
    private ResourceType result;
    public TreeBehaviour target;
    private TreeBehaviour current;

    [Header("Movement")]
    public float selectionRange = 10.0f;  // Selection range for random movement
    private float distance;
    public Vector3 targetPosition;  // Current navigation target
    public List<GameObject> targets;

    [Header("States")]
    public bool working = false;  // Are they currently working on something?
    public bool wandering = false;  // Are they wandering around?
    public bool full = false;  // Is the villager at their resource carrying limit?
    private NavMeshAgent agent;

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        resources = new Dictionary<ResourceType, int>();
    }

    // Update is called once per frame
    void Update()
    {
        // If not wandering or working, find a new destination
        if (!wandering && !working) {
            Debug.Log("Finding New Destination");
            FindNewDestination();
        }

        // If close to current target and not working, you're no longer wandering
        //if (agent.remainingDistance <= 0.2f && !working) {
        //    wandering = false;
        //}

        //If you are close to the target but are working, begin to harvest
        if (agent.remainingDistance <= 0.01f && working) {
            Debug.Log("Cutting down tree");
            wandering = false;
            result = target.Harvest(workSpeed * Time.deltaTime);
            if (result != ResourceType.None) {
                if (resources.ContainsKey(result)) {
                    resources[result] += 1;
                    totalResources += 1;
                } else {
                    resources.Add(result, 1);
                    totalResources += 1;
                }
                Debug.Log("Tree Cut Down");
                if (totalResources >= capacity) {
                    full = true;
                }
                working = false;
                target = null;
            }
        }
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

    public void FindNewDestination() {
        // Use the Random Point fn to find a new target position
        if (randomMovement) {
            if (RandomNavmeshPoint.RandomPoint(Vector3.zero, selectionRange, out targetPosition)) {
                agent.destination = targetPosition;
                wandering = true;
            }
        //Otherwise, look to the list of resources to gather
        } else {
            if (full) {
                targetPosition = Vector3.zero;
                agent.destination = targetPosition;
                wandering = true;
            } else if (job == VillagerJob.Lumberjack) {
                targets = ResourceGenerator.Trees;
                float lowestDistance = float.MaxValue;
                foreach (GameObject canidate in targets)
                {
                    if (canidate != null) {
                        current = canidate.GetComponent<TreeBehaviour>();
                        distance = Vector3.Distance(transform.position, canidate.transform.position);
                        if (distance < lowestDistance && current.selected < current.selectionLimit)
                        {
                            lowestDistance = distance;
                            targetPosition = canidate.transform.position;
                            target = current;
                        }
                    }
                }
                target.selected += 1;
                agent.destination = targetPosition;
                wandering = true;
                working = true;
            }
        }

    }
}
