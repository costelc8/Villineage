using Mirror;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Villager : NetworkBehaviour, ISelectable
{
    private NavMeshAgent agent;  // Navigates the villager
    private Animator anim;  // Animates the villager
    public TownCenter townCenter;  // The town center object
    public bool selected;  // Have they been selected?

    [Header("Stats")]
    public bool alive;  // Are they alive?
    public float workSpeed = 1f;  // Speed of resource extraction
    public float hunger;  // How much hunger do they have left
    public float hungerRate;  // How fast they lose hunger (hungerRate points a second)
    public float hungerThreshold;  // How low their hunger can get before they need to return home.
    public float maxHunger;  // The highest their hunger value can be (full)
    public float huntingRange = 10f;

    [Header("Jobs")]
    [SyncVar(hook = nameof(JobHook))]
    public VillagerJob job;  // Their current role
    public readonly SyncList<int> inventory = new SyncList<int>();  // Their resources inventory
    public int totalResources = 0; //Total number of resources across all types
    public int capacity = 3;  // Maximum amount of resources it can carry
    public Targetable target;  // The object they are targeting for their role

    [Header("Movement")]
    public float selectionRange = 10.0f;  // Selection range for random movement
    private float distance;  // The distance to their target
    private float targetPriority; // Priority of target object

    [Header("States")]
    [SyncVar(hook = nameof(StateHook))]
    public VillagerState state;  // The state of the villager (see enum below)

    [Header("Tools")]  // For animation purposes
    public GameObject axe;
    public GameObject hammer;
    public GameObject bow;
    public GameObject quiver;

    private void Awake()
    {
        // Get anim and agent objects, set up hunger variables,
        // and mark this villager as alive
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponentInChildren<Animator>();
        maxHunger = 100.0f;
        hungerRate = 1.0f;
        hungerThreshold = maxHunger / 5.0f;
        hunger = maxHunger;
        alive = true;
    }

    public void Start()
    {
        // Add to list of selectables
        Selection.Selector.AddSelectable(this);
        if (isServer)
        {
            // If this is the server, initialize the agents.
            agent.enabled = true;
            for (int i = 0; i < (int)ResourceType.MAX_VALUE; i++) inventory.Add(0);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!isServer || !alive) return;

        //if (target != null && target.liveAnimal && job == VillagerJob.Hunter)
        //{
        //    agent.stoppingDistance = 10f;
        //    SetNewTarget(target);
        //}
        //else agent.stoppingDistance = 0.1f;

        if (state == VillagerState.Pending) // If no job/target is assigned
        {
            FindNewDestination();
            if (target == null) // Edge case handling
            {
                ChangeState(VillagerState.Pending);
                SetNewTarget(townCenter);
            }
        }
        else if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance) // If the target has been reached
        {
            if (target == null) ChangeState(VillagerState.Pending); // If the target is gone, switch to pending
            else
            {
                if (state == VillagerState.Walking) ChangeState(VillagerState.Working); // If walking, start working
                else if (state == VillagerState.Working) // If working, do appropriate work based on job
                {
                    bool progress = target.Progress(this, workSpeed * Time.deltaTime);
                    if (progress && (job == VillagerJob.Builder || totalResources >= capacity))
                    {
                        ChangeState(VillagerState.Returning);
                        SetNewTarget(townCenter);
                    }
                    else if (job == VillagerJob.Hunter)
                    {
                        SetNewTarget(target);
                        transform.rotation = Quaternion.LookRotation(target.transform.position - transform.position);
                    }
                }
                else if (state == VillagerState.Returning) // If returning, deposit resources
                {
                    townCenter.DepositResources(this, inventory.ToArray());
                    for (int i = 0; i < (int)ResourceType.MAX_VALUE; i++) inventory[i] = 0;
                    totalResources = 0;
                    ChangeState(VillagerState.Pending);
                }
            }
        }
        else // If the target has not been reached
        {
            if (target != null && target.liveAnimal) SetNewTarget(target);
            if (state == VillagerState.Working) state = VillagerState.Walking;
        }

        // Decrease hunger:
        hunger -= hungerRate * Time.deltaTime;

        if (hunger <= hungerThreshold)
        {
            SetNewTarget(townCenter);
            ChangeState(VillagerState.Returning);
        }

        // If starved to death
        if (hunger <= 0) 
        {
            hunger = 0;
            Death();
        }
    }

    // Like it says on the tin.
    private void Death()
    {
        // Set alive to false and trigger the death animation.
        alive = false;
        agent.isStopped = true;
        anim.SetBool("Working",false);
        anim.SetFloat("Walking",0);
        anim.SetBool("Dead",true);
        float deathValue = UnityEngine.Random.Range(-1f, 1f);
        anim.SetFloat("Death anim", deathValue);
    }

    public void FindNewDestination()
    {
        // If the villager has no job, wander randomly.
        if (job == VillagerJob.Nitwit)
        {
            if (RandomNavmeshPoint.RandomPoint(transform.position, selectionRange, out Vector3 targetPosition))
            {
                agent.SetDestination(targetPosition);
            }
        }
        else
        {
            // Get list of possible targets based on role
            List<Targetable> candidates = new List<Targetable>();
            if (job == VillagerJob.Builder) candidates = BuildingGenerator.GetPendingBuildings();
            else if (job == VillagerJob.Lumberjack) candidates = ResourceGenerator.GetTrees();
            else if (job == VillagerJob.Gatherer) candidates = ResourceGenerator.GetBerries();
            else if (job == VillagerJob.Hunter) candidates = ResourceGenerator.GetAnimals();
            Targetable bestCandidate = null;
            float lowestDistance = float.MaxValue;  // Lowest distance of all candidates
            // For each candidate, if they exist:
            foreach (Targetable candidate in candidates)
            {
                if (candidate != null)
                {
                    // Find their distance and store the lowest
                    // With high priority objects appearing closer
                    distance = Vector3.Distance(transform.position, candidate.transform.position);
                    distance = distance / candidate.priority;
                    if (distance < lowestDistance && candidate.HasValidPositions())
                    {
                        lowestDistance = distance;
                        bestCandidate = candidate;
                        targetPriority = candidate.priority;
                    }
                }
            }
            // Use this new distance (without priority) to find the hunger threshold
            float distanceHome = Vector3.Distance(bestCandidate.transform.position, townCenter.transform.position);
            hungerThreshold = distanceHome / agent.speed;
            hungerThreshold += UnityEngine.Random.Range(1, 5);
            SetNewTarget(bestCandidate);
        }
        ChangeState(VillagerState.Walking);
    }

    // Change villager state via hook
    private void StateHook(VillagerState oldState, VillagerState newState)
    {
        ChangeState(newState);
    }

    // Update the villager's state to newState
    private void ChangeState(VillagerState newState)
    {
        state = newState;
        // Can update the Animator here
        if (newState == VillagerState.Pending)
        {
            agent.updateRotation = true;
            anim.SetBool("Working",false);
            anim.SetFloat("Walking",0);
        }
        else if (newState == VillagerState.Walking || newState == VillagerState.Returning)
        {
            agent.updateRotation = true;
            anim.SetBool("Working",false);
            anim.SetFloat("Walking",1);
        }
        else if (newState == VillagerState.Working)
        {
            agent.updateRotation = false;
            if (target != null && target != townCenter) transform.rotation = Quaternion.LookRotation(target.transform.position - transform.position);
            anim.SetBool("Working",true);
            anim.SetFloat("Walking",0);
        }
    }

    // Find opening at new target
    private void SetNewTarget(Targetable newTarget)
    {
        if (target != null) target.ReturnTargetPosition(this);
        target = newTarget;
        if (target != null) agent.SetDestination(target.GetTargetPosition(this));
        if (target != null && target.liveAnimal) agent.stoppingDistance = huntingRange;
        else agent.stoppingDistance = 0.1f;
    }

    // Change villager job via hook
    private void JobHook(VillagerJob oldJob, VillagerJob newJob)
    {
        ChangeJob(newJob);
    }

    // Update the villager's job
    public void ChangeJob(VillagerJob job)
    {
        this.job = job;
        axe.SetActive(job == VillagerJob.Lumberjack);
        hammer.SetActive(job == VillagerJob.Builder);
        bow.SetActive(job == VillagerJob.Hunter);
        quiver.SetActive(job == VillagerJob.Hunter);
    }

    // Getter for villager's job
    public VillagerJob Job()
    {
        return job;
    }

    // When selected, display ui
    public void OnSelect()
    {
        selected = true;
        GameObject HUD = UnitHUD.HUD.AddUnitHUD(gameObject, UnitHUD.HUD.villagerHUD, 1f);
        HUD.GetComponent<DisplayController>().villager = this;
    }

    // When deselected, stop displaying ui
    public void OnDeselect()
    {
        selected = false;
        UnitHUD.HUD.RemoveUnitHUD(gameObject);
    }

    // When destroyed, remove villager from other objects
    private void OnDestroy()
    {
        Selection.Selector.RemoveSelectable(this);
        townCenter.RemoveVillager(this);
        UnitHUD.HUD.RemoveUnitHUD(gameObject);
    }
}
public enum VillagerState
{
    Pending,
    Walking,
    Working,
    Returning,
}