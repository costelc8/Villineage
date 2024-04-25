using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Villager : MonoBehaviour, ISelectable
{
    public bool selected;
    public bool randomMovement = true;

    [Header("Jobs")]
    public VillagerJob job;
    public float workSpeed = 5.0f;  // Speed of resource extraction
    public float wood = 0.0f;  // Amount of wood being carried
    public float capacity = 50.0f;  // Maximum amount of wood it can carry
    public List <GameObject> targets = new List<GameObject>();

    [Header("Movement")]
    public float selectionRange = 10.0f;  // Selection range for random movement
    private float lowestDistance;
    private float distance;
    public Vector3 target;  // Current navigation target

    [Header("States")]
    public bool working = false;  // Are they currently working on something?
    public bool wandering = false;  // Are they wandering around?
    public bool full = false;  // Is wood >= capacity?
    private NavMeshAgent agent; 
    private RandomNavmeshPoint manager;  // Random point selectr
    private ResourceGeneration generator;

    // Start is called before the first frame update
    void Start()
    {
        agent = this.GetComponent<UnityEngine.AI.NavMeshAgent>();
        //manager = GameObject.FindWithTag("Manager").GetComponent<RandomNavmeshPoint>();
        generator = GameObject.FindWithTag("Ground").GetComponent<ResourceGeneration>();
        targets = generator.GetAllTrees();
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

    //public void OnTriggerStay(Collider other) {
    //    // If a tree enters your trigger, the villager is now working
    //    // They will move to the tree to cut it down
    //    Debug.Log("OnTriggerStay");
    //    if (other.gameObject.CompareTag("Tree") && !working) {
    //        print("Tree Spotted");
    //        target = other.gameObject.transform.position;
    //        agent.destination = target;
    //        working = true;
    //    }
    //}

    public void FindNewDestination() {
        // Use the Random Point fn to find a new target position
        lowestDistance = float.MaxValue;
        if (randomMovement) {
            if (manager.RandomPoint(Vector3.zero, selectionRange, out target)) {
                agent.destination = target;
                wandering = true;
            }
        //Otherwise, look to the list of resources to gather
        } else {
            foreach (GameObject canidate in targets) {
                distance = Vector3.Distance(this.transform.position, canidate.transform.position);
                if (distance < lowestDistance) {
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
