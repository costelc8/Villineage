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
    public List<Targetable> hubs;  // The different outposts/alternatives to town center
    public bool selected;  // Have they been selected?

    [Header("Stats")]
    public bool alive = true;  // Are they alive?
    public float vitality;  // How much hunger do they have left
    public float maxVitality = 100f;  // The highest their hunger value can be (full)
    public float vitalityThreshold;
    public float huntingRange = 10f;
    private float workSpeed;  // Speed of resource extraction
    private float hungerRate;  // How fast they lose hunger (hungerRate points a second)

    [Header("Jobs")]
    [SyncVar(hook = nameof(JobHook))]
    public VillagerJob job;  // Their current role
    public readonly SyncList<int> inventory = new SyncList<int>();  // Their resources inventory
    public int totalResources = 0; //Total number of resources across all types
    public Targetable target;  // The object they are targeting for their role
    private int capacity;  // Maximum amount of resources it can carry

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
    public GameObject backWood;
    public GameObject backFood;
    public GameObject backOther;

    private void Awake()
    {
        // Get anim and agent objects and set up hunger variables
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponentInChildren<Animator>();
        vitality = maxVitality;
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
            hungerRate = SimVars.VARS.villagerHungerRate;
            workSpeed = SimVars.VARS.villagerWorkSpeed;
            agent.speed = SimVars.VARS.villagerMoveSpeed;
            capacity = SimVars.VARS.villagerCarryCapacity;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!isServer || !alive) return;

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
                        Vector3 towards = target.transform.position - transform.position;
                        towards.y = 0;
                        transform.rotation = Quaternion.LookRotation(towards);
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
            if (target != null && target.movingTarget) SetNewTarget(target);
            if (state == VillagerState.Working) state = VillagerState.Walking;
        }

        // Decrease hunger:
        vitality -= hungerRate * Time.deltaTime;

        if (vitality <= vitalityThreshold)
        {
            vitalityThreshold = 0;
            SetNewTarget(townCenter);
            ChangeState(VillagerState.Returning);
        }

        // If starved to death
        if (vitality <= 0) 
        {
            vitality = 0;
            Die();
        }
    }

    public void TakeDamage(float damage)
    {
        vitality -= damage;
    }

    // Like it says on the tin.
    private void Die()
    {
        // Set alive to false and trigger the death animation.
        alive = false;
        townCenter.RemoveVillager(this);
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
            if (bestCandidate != null)
            {
                float distanceHome = Vector3.Distance(bestCandidate.transform.position, townCenter.transform.position) * 1.2f;
                vitalityThreshold = (distanceHome / agent.speed) + 5;
            }
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
            if (state == VillagerState.Returning)
            {
                if (job == VillagerJob.Lumberjack) backWood.SetActive(true);
                else if (job == VillagerJob.Gatherer) backFood.SetActive(true);
            }
            else
            {
                backWood.SetActive(false);
                backFood.SetActive(false);
                backOther.SetActive(false);
            }
        }
        else if (newState == VillagerState.Working)
        {
            agent.updateRotation = false;
            if (target != null && target != townCenter)
            {
                Vector3 towards = target.transform.position - transform.position;
                towards.y = 0;
                transform.rotation = Quaternion.LookRotation(towards);
            }
            anim.SetBool("Working",true);
            anim.SetFloat("Walking",0);
        }
    }

    // Find opening at new target
    private void SetNewTarget(Targetable newTarget)
    {
        if (newTarget == null)
        {
            newTarget = townCenter;
            ChangeState(VillagerState.Returning);
        }
        if (target != null) target.ReturnTargetPosition(this);
        target = newTarget;
        if (target != null) agent.SetDestination(target.GetTargetPosition(this));
        if (target != null && target.movingTarget) agent.stoppingDistance = huntingRange;
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
        if (townCenter != null) townCenter.RemoveVillager(this);
        Selection.Selector.RemoveSelectable(this);
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