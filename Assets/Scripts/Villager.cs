using Mirror;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Villager : NetworkBehaviour, ISelectable
{
    private NavMeshAgent agent;
    private Animator anim;
    public TownCenter townCenter;
    public bool selected;

    [Header("Stats")]
    public bool alive;
    public float workSpeed = 1f;  // Speed of resource extraction
    public float hunger;
    public float hungerRate = 1f;
    public float maxHunger = 100f;
    public float huntingRange = 10f;

    [Header("Jobs")]
    [SyncVar(hook = nameof(JobHook))]
    public VillagerJob job;
    public readonly SyncList<int> inventory = new SyncList<int>();
    public int totalResources = 0; //Total number of resources across all types
    public int capacity = 3;  // Maximum amount of resources it can carry
    public Targetable target;

    [Header("Movement")]
    public float selectionRange = 10.0f;  // Selection range for random movement
    private float distance;

    [Header("States")]
    [SyncVar(hook = nameof(StateHook))]
    public VillagerState state;

    [Header("Tools")]
    public GameObject axe;
    public GameObject hammer;
    public GameObject bow;
    public GameObject quiver;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponentInChildren<Animator>();
        hunger = maxHunger;
        alive = true;
    }

    public void Start()
    {
        Selection.Selector.AddSelectable(this);
        if (isServer)
        {
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

        if (hunger <= (maxHunger / 5))
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

    private void Death()
    {
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
        if (job == VillagerJob.Nitwit)
        {
            if (RandomNavmeshPoint.RandomPoint(transform.position, selectionRange, out Vector3 targetPosition))
            {
                agent.SetDestination(targetPosition);
            }
        }
        else
        {
            List<Targetable> candidates = new List<Targetable>();
            if (job == VillagerJob.Builder) candidates = BuildingGenerator.GetPendingBuildings();
            else if (job == VillagerJob.Lumberjack) candidates = ResourceGenerator.GetTrees();
            else if (job == VillagerJob.Gatherer) candidates = ResourceGenerator.GetBerries();
            else if (job == VillagerJob.Hunter) candidates = ResourceGenerator.GetAnimals();
            Targetable bestCandidate = null;
            float lowestDistance = float.MaxValue;
            foreach (Targetable candidate in candidates)
            {
                if (candidate != null)
                {
                    distance = Vector3.Distance(transform.position, candidate.transform.position);
                    distance = distance / candidate.priority;
                    if (distance < lowestDistance && candidate.HasValidPositions())
                    {
                        lowestDistance = distance;
                        bestCandidate = candidate;
                    }
                }
            }
            SetNewTarget(bestCandidate);
        }
        ChangeState(VillagerState.Walking);
    }

    private void StateHook(VillagerState oldState, VillagerState newState)
    {
        ChangeState(newState);
    }

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

    private void SetNewTarget(Targetable newTarget)
    {
        if (target != null) target.ReturnTargetPosition(this);
        target = newTarget;
        if (target != null) agent.SetDestination(target.GetTargetPosition(this));
        if (target != null && target.liveAnimal) agent.stoppingDistance = huntingRange;
        else agent.stoppingDistance = 0.1f;
    }

    private void JobHook(VillagerJob oldJob, VillagerJob newJob)
    {
        ChangeJob(newJob);
    }

    public void ChangeJob(VillagerJob job)
    {
        this.job = job;
        axe.SetActive(job == VillagerJob.Lumberjack);
        hammer.SetActive(job == VillagerJob.Builder);
        bow.SetActive(job == VillagerJob.Hunter);
        quiver.SetActive(job == VillagerJob.Hunter);
    }

    public VillagerJob Job()
    {
        return job;
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