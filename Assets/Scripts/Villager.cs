using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Villager : MonoBehaviour, ISelectable
{
    private NavMeshAgent agent;
    private Animator anim;
    public TownCenter townCenter;
    public bool selected;

    [Header("Stats")]
    public bool alive;
    public float workSpeed = 1.0f;  // Speed of resource extraction
    public float hunger;
    public float hungerRate = 10.0f;
    public float maxHunger = 100.0f;

    [Header("Jobs")]
    private VillagerJob job;
    public int[] resources = new int[(int)ResourceType.MAX_VALUE]; // Amount of resources being carried
    public int totalResources = 0; //Total number of resources across all types
    public int capacity = 3;  // Maximum amount of resources it can carry
    public Targetable target;

    [Header("Movement")]
    public float selectionRange = 10.0f;  // Selection range for random movement
    private float distance;

    [Header("States")]
    //public bool working = false;  // Are they currently working on something?
    //public bool walking = false;  // Are they wandering around?
    //public bool returning = false;
    public VillagerState state;

    [Header("Tools")]
    public GameObject axe;
    public GameObject hammer;

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponentInChildren<Animator>();
        hunger = maxHunger;
        alive = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (!alive) { return; }

        if (state == VillagerState.Pending)
        {
            FindNewDestination();
            if (target == null) // Edge case handling
            {
                ChangeState(VillagerState.Pending);
                SetNewTarget(townCenter);
            }
        }
        else if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            if (target == null) ChangeState(VillagerState.Pending);
            else if (state == VillagerState.Walking)
            {
                ChangeState(VillagerState.Working);
            }
            else if (state == VillagerState.Working)
            {
                bool progress = target.Progress(this, workSpeed * Time.deltaTime);
                if (progress && (job == VillagerJob.Builder || totalResources >= capacity))
                {
                    ChangeState(VillagerState.Returning);
                    SetNewTarget(townCenter);
                }
            }
            else if (state == VillagerState.Returning)
            {
                townCenter.DepositResources(this, resources);
                Array.Clear(resources, 0, resources.Length);
                totalResources = 0;
                ChangeState(VillagerState.Pending);
            }
        }

        // Decrease hunger:
        hunger -= hungerRate * Time.deltaTime;

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
        anim.SetFloat("Death anim",-1);
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

    private void ChangeState(VillagerState newState)
    {
        state = newState;
        // Can update the Animator here
        if (newState == VillagerState.Pending)
        {
            agent.updateRotation = true;
            anim.SetBool("Working",false);
            anim.SetFloat("Walking",0);
        } else if (newState == VillagerState.Walking || newState == VillagerState.Returning)
        {
            agent.updateRotation = true;
            anim.SetBool("Working",false);
            anim.SetFloat("Walking",1);
        }
        else if (newState == VillagerState.Working)
        {
            transform.rotation = Quaternion.LookRotation(target.transform.position - transform.position);
            anim.SetBool("Working",true);
            anim.SetFloat("Walking",0);
            agent.updateRotation = false;
        }
    }

    private void SetNewTarget(Targetable newTarget)
    {
        if (target != null) target.ReturnTargetPosition(this);
        target = newTarget;
        if (target != null) agent.SetDestination(target.GetTargetPosition(this));
    }

    public void ChangeJob(VillagerJob job)
    {
        this.job = job;
        axe.SetActive(job == VillagerJob.Lumberjack);
        hammer.SetActive(job == VillagerJob.Builder);
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

    public enum VillagerState
    {
        Pending,
        Walking,
        Working,
        Returning,
    }
}